"""Steps 7-12: README, SonarCloud, commit/push, verify, cleanup."""

from __future__ import annotations

import os
import shutil
import time

from ..config import Config
from ..log import fail, fatal, log, ok
from ..shell import GitHub, SonarCloud, run


def get_sonar_project_keys(cfg: Config) -> list[str]:
    prefix = f"{cfg.owner}_{cfg.repo}"
    if cfg.arch == "monolith":
        return [f"{prefix}-monolith-{cfg.lang}"]
    else:
        return [
            f"{prefix}-multitier-backend-{cfg.backend_lang}",
            f"{prefix}-multitier-frontend-{cfg.frontend_lang}",
        ]


# ─── Step 7: Update README ──────────────────────────────────────────────────


def update_readme(cfg: Config, **_: object) -> None:
    log("Step 7: Generating README...")

    if cfg.dry_run:
        log("[DRY RUN] Would generate README.md")
        return

    badges = _generate_badges(cfg)
    readme = (
        f"# {cfg.system_name}\n\n"
        f"{badges}\n"
        f"## License\n\nMIT License\n\n"
        f"## Contributors\n\n"
        f"- [{cfg.owner}](https://github.com/{cfg.owner})\n"
    )

    with open(os.path.join(cfg.repo_dir, "README.md"), "w", encoding="utf-8", newline="\n") as f:
        f.write(readme)
    ok("Generated README.md")


def _generate_badges(cfg: Config) -> str:
    base = f"https://github.com/{cfg.full_repo}/actions/workflows"

    if cfg.arch == "monolith":
        lang, test_lang = cfg.lang, cfg.test_lang
        items = [
            (f"monolith-{lang}-commit-stage.yml", "commit-stage"),
            (f"monolith-{test_lang}-acceptance-stage.yml", "acceptance-stage"),
            (f"monolith-{test_lang}-qa-stage.yml", "qa-stage"),
            (f"monolith-{test_lang}-qa-signoff.yml", "qa-signoff"),
            (f"monolith-{test_lang}-prod-stage.yml", "prod-stage"),
        ]
    else:
        bl, fl, tl = cfg.backend_lang, cfg.frontend_lang, cfg.test_lang
        items = [
            (f"multitier-backend-{bl}-commit-stage.yml", "backend-commit-stage"),
            (f"multitier-frontend-{fl}-commit-stage.yml", "frontend-commit-stage"),
            (f"multitier-system-{tl}-acceptance-stage.yml", "acceptance-stage"),
            (f"multitier-system-{tl}-qa-stage.yml", "qa-stage"),
            (f"multitier-system-{tl}-qa-signoff.yml", "qa-signoff"),
            (f"multitier-system-{tl}-prod-stage.yml", "prod-stage"),
        ]

    return "\n".join(
        f"[![{label}]({base}/{wf}/badge.svg)]({base}/{wf})"
        for wf, label in items
    ) + "\n"


# ─── Step 8: Create SonarCloud projects ─────────────────────────────────────


def create_sonarcloud_projects(cfg: Config, sonarcloud: SonarCloud, **_: object) -> None:
    log("Step 8: Creating SonarCloud projects...")

    if cfg.dry_run:
        log("[DRY RUN] Would create SonarCloud org and project(s)")
        return

    sonarcloud.create_org()
    for key in get_sonar_project_keys(cfg):
        sonarcloud.create_project(key)


# ─── Step 9: Commit and push ────────────────────────────────────────────────


def commit_and_push(cfg: Config, **_: object) -> None:
    log("Step 9: Committing and pushing...")

    if cfg.dry_run:
        log("[DRY RUN] Would git add, commit, push")
        return

    run("git add -A", cwd=cfg.repo_dir)
    run('git commit -m "Apply pipeline template"', cwd=cfg.repo_dir)
    run("git push", cwd=cfg.repo_dir)
    ok(f"Pushed template to {cfg.full_repo}")


# ─── Step 10: Verify commit stage ───────────────────────────────────────────


def verify_commit_stage(cfg: Config, github: GitHub, **_: object) -> None:
    log("Step 10: Verifying commit stage workflow...")

    if cfg.dry_run:
        log("[DRY RUN] Would wait for commit stage workflow")
        return

    time.sleep(5)
    _verify_workflow(github, "Commit stage")


# ─── Step 11: Verify acceptance stage ────────────────────────────────────────


def verify_acceptance_stage(cfg: Config, github: GitHub, **_: object) -> None:
    log("Step 11: Triggering and verifying acceptance stage...")

    if cfg.dry_run:
        log("[DRY RUN] Would trigger and wait for acceptance stage workflow")
        return

    test_lang = cfg.test_lang
    if cfg.arch == "monolith":
        wf = f"monolith-{test_lang}-acceptance-stage.yml"
    else:
        wf = f"multitier-system-{test_lang}-acceptance-stage.yml"

    _verify_workflow(github, "Acceptance stage", trigger_workflow=wf)


def _verify_workflow(github: GitHub, label: str, trigger_workflow: str | None = None) -> None:
    if trigger_workflow:
        github.workflow_run(trigger_workflow)
        time.sleep(5)

    result = github.run_watch()
    if result.returncode != 0:
        fail(f"{label} failed!")
        fatal(f"{label} workflow failed. Check: https://github.com/{github.repo}/actions")
    ok(f"{label} passed!")


# ─── Step 12: Cleanup ───────────────────────────────────────────────────────


def cleanup(cfg: Config, github: GitHub, sonarcloud: SonarCloud) -> None:
    if not cfg.test_mode:
        return

    should_cleanup = cfg.cleanup
    if should_cleanup == "ask":
        answer = input(f"\nDelete test repository {cfg.full_repo}? [y/N] ").strip().lower()
        should_cleanup = "yes" if answer in ("y", "yes") else "no"

    if should_cleanup == "yes":
        log(f"Cleaning up: deleting {cfg.full_repo}...")
        github.delete()
        ok(f"Deleted repository {cfg.full_repo}")

        for key in get_sonar_project_keys(cfg):
            sonarcloud.delete_project(key)

        if cfg.repo_dir and os.path.exists(cfg.repo_dir):
            shutil.rmtree(cfg.repo_dir, ignore_errors=True)
        ok("Cleanup complete")
    else:
        log(f"Keeping repository: https://github.com/{cfg.full_repo}")
