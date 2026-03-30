"""Template helpers: copy workflows, docker-compose selection, VERSION."""

from __future__ import annotations

import os
import shutil

from .log import warn


def copy_workflows(workflow_names: list[str], starter: str, repo_dir: str) -> None:
    """Copy workflow files from starter to repo."""
    wf_src = os.path.join(starter, ".github", "workflows")
    wf_dst = os.path.join(repo_dir, ".github", "workflows")
    for wf in workflow_names:
        src = os.path.join(wf_src, wf)
        if os.path.exists(src):
            shutil.copy2(src, os.path.join(wf_dst, wf))
        else:
            warn(f"Workflow not found: {wf}")


def select_docker_compose(test_dst: str, variant: str) -> None:
    """Keep the chosen docker-compose variant, remove the other.
    variant: 'single' for monolith, 'multi' for multitier.
    The kept file retains its original name (e.g. docker-compose.single.yml)
    because workflows reference the variant-specific filename.
    """
    remove = "multi" if variant == "single" else "single"
    remove_path = os.path.join(test_dst, f"docker-compose.{remove}.yml")

    if os.path.exists(remove_path):
        os.remove(remove_path)


def copy_version(starter: str, repo_dir: str) -> None:
    """Copy VERSION file from starter to repo."""
    src = os.path.join(starter, "VERSION")
    if os.path.exists(src):
        shutil.copy2(src, os.path.join(repo_dir, "VERSION"))
