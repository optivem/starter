"""Single-scenario verification runner — the Python equivalent of verifier.md agent."""

from __future__ import annotations

import secrets
import subprocess
import sys
import time
from dataclasses import dataclass, field

from scaffold.log import fail, log, ok, warn
from scaffold.shell import run
from verify.config import Scenario
from verify.steps.pipeline import verify_qa_stage, verify_production_stage
from verify.tracking import IssueTracker


@dataclass
class ScenarioResult:
    """Result of running a single verification scenario."""

    scenario_name: str
    passed: bool = False
    step_results: list[str] = field(default_factory=list)
    problems: list[str] = field(default_factory=list)
    fixes: list[str] = field(default_factory=list)
    repo_url: str = ""
    issue_url: str = ""


def _to_kebab(s: str) -> str:
    """Convert 'Page Turner' to 'page-turner'."""
    return s.lower().replace(" ", "-")


def _build_scaffold_args(scenario: Scenario, random_suffix: bool, cleanup: str) -> list[str]:
    """Build CLI args for scaffold.py."""
    repo_name = _to_kebab(scenario.system_name)

    args = [
        "--owner", scenario.github_owner,
        "--system-name", scenario.system_name,
        "--repo", repo_name,
        "--arch", scenario.architecture,
    ]

    if scenario.architecture == "monolith":
        if scenario.system_language:
            args.extend(["--lang", scenario.system_language])
    else:
        if scenario.backend_language:
            args.extend(["--backend-lang", scenario.backend_language])
        if scenario.frontend_language:
            args.extend(["--frontend-lang", scenario.frontend_language])

    if scenario.system_test_language:
        args.extend(["--test-lang", scenario.system_test_language])

    if random_suffix:
        args.append("--random-suffix")

    args.append("--test")

    if cleanup == "yes":
        args.append("--cleanup")
    elif cleanup == "no":
        args.append("--no-cleanup")

    return args


def _get_step_names(arch: str) -> list[str]:
    """Return the list of step names for a given architecture."""
    base = [
        "Step 00: Prerequisites",
        "Step 01: Create Repository",
        "Step 02: Setup Environments",
        "Step 03: Setup Secrets & Variables",
        "Step 04: Clone & Apply Template",
        "Step 05: Replace References",
        "Step 06: Replace Namespaces",
        "Step 07: Update README",
        "Step 08: Create SonarCloud Projects",
        "Step 09: Commit & Push",
        "Step 10: Verify Commit Stage",
        "Step 11: Verify Acceptance Stage",
        "Step 12: Verify QA Stage",
        "Step 13: Verify Production Stage",
    ]
    return base


def run_scenario(
    scenario: Scenario,
    random_suffix: bool = True,
    cleanup: str = "no",
    dry_run: bool = False,
) -> ScenarioResult:
    """Run a full verification scenario: scaffold + QA + production stages."""
    result = ScenarioResult(scenario_name=scenario.name)
    step_names = _get_step_names(scenario.architecture)
    lang_label = scenario.system_language or scenario.backend_language or "unknown"
    test_lang = scenario.system_test_language or lang_label

    print()
    print("=" * 60)
    print(f"  Scenario: {scenario.name} [{lang_label}, {scenario.architecture}, {scenario.repository_strategy}]")
    print("=" * 60)
    print()

    # --- Step 00: Prerequisites check ---
    log("Step 00: Checking prerequisites...")
    prereq_ok = True
    for tool in ["gh", "git", "python"]:
        check = run(f"which {tool}", check=False, capture=True)
        if check.returncode != 0:
            fail(f"Prerequisite missing: {tool}")
            result.problems.append(f"[Step 00] Missing tool: {tool}")
            prereq_ok = False

    if not prereq_ok:
        result.step_results.append("Step 00: Prerequisites FAIL")
        return result

    result.step_results.append("Step 00: Prerequisites OK")

    # --- Steps 01-11: Run scaffold.py ---
    log("Steps 01-11: Running scaffold...")
    scaffold_args = _build_scaffold_args(scenario, random_suffix, cleanup="no")  # don't cleanup yet

    # Find scaffold.py relative to this file
    import os
    scripts_dir = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    scaffold_script = os.path.join(scripts_dir, "scaffold.py")

    if dry_run:
        log(f"[DRY RUN] python {scaffold_script} {' '.join(scaffold_args)}")
        for i in range(1, 12):
            result.step_results.append(f"{step_names[i]} OK (dry run)")
        result.step_results.append(f"{step_names[12]} OK (dry run)")
        result.step_results.append(f"{step_names[13]} OK (dry run)")
        result.passed = True
        return result

    cmd = [sys.executable, scaffold_script] + scaffold_args
    log(f"Running: {' '.join(cmd)}")

    scaffold_result = subprocess.run(cmd, capture_output=True, text=True)

    if scaffold_result.stdout:
        print(scaffold_result.stdout)
    if scaffold_result.stderr:
        print(scaffold_result.stderr)

    if scaffold_result.returncode != 0:
        fail("Scaffold failed!")
        result.problems.append(f"[Steps 01-11] Scaffold exited with code {scaffold_result.returncode}")
        result.step_results.append("Steps 01-11: Scaffold FAIL")
        return result

    for i in range(1, 12):
        result.step_results.append(f"{step_names[i]} OK")

    # Determine the actual repo name (scaffold may have added random suffix)
    repo_name = _detect_repo_name(scaffold_result.stdout, scenario)
    full_repo = f"{scenario.github_owner}/{repo_name}"
    result.repo_url = f"https://github.com/{full_repo}"

    # --- Create tracking issue ---
    tracker = IssueTracker(full_repo, scenario.name, step_names, dry_run=dry_run)
    try:
        tracker.create()
        for i in range(12):
            tracker.complete_step(i)
    except Exception as e:
        warn(f"Could not create tracking issue: {e}")

    # --- Step 12: QA Stage ---
    try:
        rc_version = verify_qa_stage(
            repo=full_repo,
            arch=scenario.architecture,
            test_lang=test_lang,
            dry_run=dry_run,
        )
        result.step_results.append(f"{step_names[12]} OK")
        tracker.complete_step(12)
    except SystemExit:
        result.step_results.append(f"{step_names[12]} FAIL")
        result.problems.append(f"[{step_names[12]}] QA stage verification failed")
        tracker.fail_step(step_names[12], "QA stage verification failed")
        return result

    # --- Step 13: Production Stage ---
    try:
        verify_production_stage(
            repo=full_repo,
            arch=scenario.architecture,
            test_lang=test_lang,
            rc_version=rc_version,
            dry_run=dry_run,
        )
        result.step_results.append(f"{step_names[13]} OK")
        tracker.complete_step(13)
    except SystemExit:
        result.step_results.append(f"{step_names[13]} FAIL")
        result.problems.append(f"[{step_names[13]}] Production stage verification failed")
        tracker.fail_step(step_names[13], "Production stage verification failed")
        return result

    # All passed
    result.passed = True
    tracker.close(success=True)

    if result.issue_url == "" and tracker.issue_number:
        result.issue_url = f"https://github.com/{full_repo}/issues/{tracker.issue_number}"

    ok(f"Scenario {scenario.name}: ALL STEPS PASSED")
    return result


def _detect_repo_name(scaffold_output: str, scenario: Scenario) -> str:
    """Extract the actual repo name from scaffold output (may include random suffix)."""
    for line in scaffold_output.splitlines():
        if "Repository:" in line and "github.com" in line:
            # Extract repo name from URL like https://github.com/owner/repo-name
            url = line.split("github.com/")[-1].strip()
            parts = url.split("/")
            if len(parts) >= 2:
                return parts[1]

    # Fallback: use kebab system name (scaffold may have added suffix, but we can't detect it)
    return _to_kebab(scenario.system_name)
