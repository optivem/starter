"""Step 4: Clone repo and apply template files."""

from __future__ import annotations

import os
import shutil

from ..config import Config
from ..files import replace_in_file
from ..log import log, ok
from ..shell import GitHub
from ..templates import copy_version, copy_workflows, select_docker_compose

# Internal port exposed by each language's Docker image
_INTERNAL_PORTS = {"java": 8080, "dotnet": 8080, "typescript": 3000}


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

    if lang != test_lang:
        _fixup_monolith_cross_lang(repo_dir, lang, test_lang)


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

    if backend_lang != test_lang:
        _fixup_multitier_cross_lang(repo_dir, backend_lang, test_lang)


# ─── Cross-language fixups ─────────────────────────────────────────────────


def _fixup_monolith_cross_lang(
    repo_dir: str, lang: str, test_lang: str,
) -> None:
    """Fix Docker image name and port when system language != test language."""
    old_image = f"monolith-{test_lang}-monolith"
    new_image = f"monolith-{lang}-monolith"

    targets = [
        os.path.join(repo_dir, ".github", "workflows", f"monolith-{test_lang}-acceptance-stage.yml"),
        os.path.join(repo_dir, ".github", "workflows", f"monolith-{test_lang}-qa-stage.yml"),
        os.path.join(repo_dir, ".github", "workflows", f"monolith-{test_lang}-prod-stage.yml"),
        os.path.join(repo_dir, "system-test", test_lang, "docker-compose.yml"),
    ]
    for path in targets:
        if os.path.isfile(path):
            replace_in_file(path, old_image, new_image)

    # Fix port mapping: template uses test_lang's port, but we need lang's port
    system_port = _INTERNAL_PORTS[lang]
    template_port = _INTERNAL_PORTS[test_lang]
    if system_port != template_port:
        compose = os.path.join(repo_dir, "system-test", test_lang, "docker-compose.yml")
        if os.path.isfile(compose):
            replace_in_file(compose, f"8080:{template_port}", f"8080:{system_port}")

    ok(f"Cross-language fixup: {old_image} -> {new_image}")


def _fixup_multitier_cross_lang(
    repo_dir: str, backend_lang: str, test_lang: str,
) -> None:
    """Fix Docker backend image name when backend language != test language."""
    old_image = f"multitier-backend-{test_lang}"
    new_image = f"multitier-backend-{backend_lang}"

    targets = [
        os.path.join(repo_dir, ".github", "workflows", f"multitier-system-{test_lang}-acceptance-stage.yml"),
        os.path.join(repo_dir, ".github", "workflows", f"multitier-system-{test_lang}-qa-stage.yml"),
        os.path.join(repo_dir, ".github", "workflows", f"multitier-system-{test_lang}-prod-stage.yml"),
        os.path.join(repo_dir, "system-test", test_lang, "docker-compose.yml"),
    ]
    for path in targets:
        if os.path.isfile(path):
            replace_in_file(path, old_image, new_image)

    ok(f"Cross-language fixup: {old_image} -> {new_image}")
