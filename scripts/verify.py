#!/usr/bin/env python3
"""
Deterministic verification script — the Python equivalent of verifier-manager + verifier agents.

Reads verifier-config.json, runs scaffold + full pipeline verification (commit, acceptance,
QA, production) for each scenario.

Usage:
  List batches:
    python verify.py --list

  Run a single batch:
    python verify.py --batch monolith-quick

  Run all batches:
    python verify.py --batch all

  Run a single scenario:
    python verify.py --scenario monolith-java

  Dry run:
    python verify.py --batch monolith-quick --dry-run
"""

from verify.config import (
    find_config_path,
    list_batches,
    load_config,
    parse_args,
    resolve_batches,
)
from verify.runner import ScenarioResult, run_scenario
from scaffold.log import fail, log, ok


def print_report(batch_name: str, batch_desc: str, results: list[ScenarioResult]) -> None:
    """Print the combined verification report for a batch."""
    passed = sum(1 for r in results if r.passed)
    total = len(results)

    print()
    print("=" * 60)
    print("  Verification Results")
    print("=" * 60)
    print(f"  Batch: {batch_name} — {batch_desc}")
    print()

    for r in results:
        lang = r.scenario_name
        print(f"  Scenario: {lang}")
        for step in r.step_results:
            print(f"    {step}")
        if r.repo_url:
            print(f"    Test Project: {r.repo_url}")
        if r.issue_url:
            print(f"    Setup Issue: {r.issue_url}")
        print()
        print("  ---")
        print()

    print(f"  Summary: {passed}/{total} scenarios passed")
    print()

    # Consolidated problems
    all_problems = []
    for r in results:
        for p in r.problems:
            all_problems.append(f"[{r.scenario_name}] {p}")
    if all_problems:
        print("  Challenges Found:")
        for i, p in enumerate(all_problems, 1):
            print(f"    {i}. {p}")
        print()

    # Consolidated fixes
    all_fixes = []
    for r in results:
        for f in r.fixes:
            all_fixes.append(f"[{r.scenario_name}] {f}")
    if all_fixes:
        print("  Fixes Applied:")
        for i, fx in enumerate(all_fixes, 1):
            print(f"    {i}. {fx}")
        print()


def main() -> None:
    args = parse_args()
    config_path = find_config_path(args.config)
    config = load_config(config_path)

    log(f"Config: {config_path}")

    if args.list:
        list_batches(config)
        return

    if not args.batch and not args.scenario:
        list_batches(config)
        print("Use --batch <name>, --batch all, or --scenario <name> to run.\n")
        return

    batches = resolve_batches(config, args.batch, args.scenario)
    cleanup = "yes" if args.cleanup else ("no" if args.no_cleanup else "no")

    all_passed = True

    for batch in batches:
        log(f"Running batch: {batch.name} — {batch.description} ({len(batch.scenarios)} scenarios)")

        results: list[ScenarioResult] = []
        batch_failed = False

        for scenario in batch.scenarios:
            result = run_scenario(
                scenario=scenario,
                random_suffix=args.random_suffix,
                cleanup=cleanup,
                dry_run=args.dry_run,
            )
            results.append(result)

            if not result.passed:
                batch_failed = True
                break  # stop on first failure within a batch

        print_report(batch.name, batch.description, results)

        if batch_failed:
            fail(f"Batch '{batch.name}' had failures — stopping.")
            all_passed = False
            break  # stop across batches too

    print()
    if all_passed:
        ok("All verification completed successfully!")
    else:
        fail("Verification completed with failures.")

    # Suggest cleanup
    defaults = config["defaults"]
    system_name = defaults["SYSTEM_NAME"]
    kebab_name = system_name.lower().replace(" ", "-")
    owner = defaults["GITHUB_OWNER"]
    print()
    print(f"  To clean up test repos, run:")
    print(f"  bash github-utils/scripts/delete-repos.sh {owner} --prefix {kebab_name}-")
    print()

    raise SystemExit(0 if all_passed else 1)


if __name__ == "__main__":
    main()
