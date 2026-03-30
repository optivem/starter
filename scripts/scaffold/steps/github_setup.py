"""Steps 1-3: Create repo, environments, secrets and variables."""

from __future__ import annotations

import time

from ..config import Config
from ..log import log, ok
from ..shell import GitHub


def create_repo(cfg: Config, github: GitHub, **_: object) -> None:
    log(f"Step 1: Creating repository {cfg.full_repo}...")

    if cfg.dry_run:
        log(f"[DRY RUN] gh repo create {cfg.full_repo} --public --add-readme --license mit")
        return

    github.create_repo()
    time.sleep(3)
    ok(f"Created repository: {cfg.full_repo}")


def setup_environments(cfg: Config, github: GitHub, **_: object) -> None:
    log("Step 2: Creating environments...")
    for env in ["acceptance", "qa", "production"]:
        github.create_environment(env)
    ok("Created environments: acceptance, qa, production")


def setup_secrets_and_variables(cfg: Config, github: GitHub, **_: object) -> None:
    log("Step 3: Setting secrets and variables...")

    github.secret_set("DOCKERHUB_TOKEN", cfg.dockerhub_token)
    github.secret_set("SONAR_TOKEN", cfg.sonar_token)
    github.variable_set("DOCKERHUB_USERNAME", cfg.dockerhub_username)
    github.variable_set("SYSTEM_URL", "http://localhost:8080")

    for env in ["acceptance", "qa", "production"]:
        github.variable_set("SYSTEM_URL", "http://localhost:8080", env=env)

    ok("Set secrets and variables")
