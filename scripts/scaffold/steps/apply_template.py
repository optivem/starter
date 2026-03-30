"""Step 4: Clone repo and apply template files."""

from __future__ import annotations

import os
import shutil

from ..config import Config
from ..log import log, ok
from ..shell import GitHub
from ..templates import copy_version, copy_workflows, select_docker_compose


def clone_and_apply_template(cfg: Config, github: GitHub, **_: object) -> None:
    log("Step 4: Cloning repo and applying template files...")

    if cfg.dry_run:
        log("[DRY RUN] Would clone repo and copy template files")
        return

    repo_dir = os.path.join(cfg.workdir, "repo")
    starter = cfg.starter_path

    github.clone(repo_dir)
    ok(f"Cloned {cfg.full_repo}")

    os.makedirs(os.path.join(repo_dir, ".github", "workflows"), exist_ok=True)

    if cfg.arch == "monolith":
        _apply_monolith(cfg, repo_dir, starter)
    else:
        _apply_multitier(cfg, repo_dir, starter)

    cfg.repo_dir = repo_dir
    ok("Applied template files")


def _apply_monolith(cfg: Config, repo_dir: str, starter: str) -> None:
    lang = cfg.lang
    test_lang = cfg.test_lang
    assert lang is not None

    workflows = [
        f"monolith-{lang}-commit-stage.yml",
        f"monolith-{test_lang}-acceptance-stage.yml",
        f"monolith-{test_lang}-qa-stage.yml",
        f"monolith-{test_lang}-qa-signoff.yml",
        f"monolith-{test_lang}-prod-stage.yml",
    ]
    if lang == test_lang:
        workflows.append(f"monolith-{lang}-verify.yml")
    copy_workflows(workflows, starter, repo_dir)

    shutil.copytree(
        os.path.join(starter, "system", "monolith", lang),
        os.path.join(repo_dir, "system", "monolith", lang),
    )

    test_dst = os.path.join(repo_dir, "system-test", test_lang)
    shutil.copytree(os.path.join(starter, "system-test", test_lang), test_dst)
    select_docker_compose(test_dst, "single")
    copy_version(starter, repo_dir)


def _apply_multitier(cfg: Config, repo_dir: str, starter: str) -> None:
    backend_lang = cfg.backend_lang
    frontend_lang = cfg.frontend_lang
    test_lang = cfg.test_lang
    assert backend_lang is not None and frontend_lang is not None

    workflows = [
        f"multitier-backend-{backend_lang}-commit-stage.yml",
        f"multitier-frontend-{frontend_lang}-commit-stage.yml",
        f"multitier-system-{test_lang}-acceptance-stage.yml",
        f"multitier-system-{test_lang}-qa-stage.yml",
        f"multitier-system-{test_lang}-qa-signoff.yml",
        f"multitier-system-{test_lang}-prod-stage.yml",
    ]
    if backend_lang == test_lang:
        workflows.append(f"multitier-{backend_lang}-verify.yml")
    copy_workflows(workflows, starter, repo_dir)

    shutil.copytree(
        os.path.join(starter, "system", "multitier", f"backend-{backend_lang}"),
        os.path.join(repo_dir, "system", "multitier", f"backend-{backend_lang}"),
    )
    shutil.copytree(
        os.path.join(starter, "system", "multitier", f"frontend-{frontend_lang}"),
        os.path.join(repo_dir, "system", "multitier", f"frontend-{frontend_lang}"),
    )

    test_dst = os.path.join(repo_dir, "system-test", test_lang)
    shutil.copytree(os.path.join(starter, "system-test", test_lang), test_dst)
    select_docker_compose(test_dst, "multi")
    copy_version(starter, repo_dir)
