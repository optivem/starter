"""Configuration: reads verifier-config.json and CLI arguments."""

from __future__ import annotations

import argparse
import json
import os
from dataclasses import dataclass, field


@dataclass
class Scenario:
    """A single verification scenario with merged config values."""

    name: str
    github_owner: str
    system_domain: str
    system_name: str
    architecture: str
    repository_strategy: str
    system_language: str | None = None
    backend_language: str | None = None
    frontend_language: str | None = None
    system_test_language: str | None = None


@dataclass
class Batch:
    """A named batch of scenarios."""

    name: str
    description: str
    scenarios: list[Scenario] = field(default_factory=list)


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Verify scaffolded projects by running full pipeline stages.",
        formatter_class=argparse.RawDescriptionHelpFormatter,
    )
    group = parser.add_mutually_exclusive_group()
    group.add_argument("--batch", help='Batch name to run, or "all" for every batch')
    group.add_argument("--scenario", help="Single scenario name to run")
    parser.add_argument("--list", action="store_true", help="List available batches and exit")
    parser.add_argument("--random-suffix", action="store_true", default=True, help="Append random suffix to repo names (default: true)")
    parser.add_argument("--no-random-suffix", action="store_false", dest="random_suffix", help="Do not append random suffix")
    parser.add_argument("--cleanup", action="store_true", help="Auto-cleanup repos after verification")
    parser.add_argument("--no-cleanup", action="store_true", help="Keep repos after verification")
    parser.add_argument("--dry-run", action="store_true", help="Print actions without executing")
    parser.add_argument("--config", help="Path to verifier-config.json (default: auto-detect)")
    return parser.parse_args()


def find_config_path(override: str | None = None) -> str:
    """Locate verifier-config.json relative to the starter repo root."""
    if override:
        return override
    # Walk up from this file to find the repo root (has VERSION file)
    d = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
    config_path = os.path.join(d, ".claude", "agents", "verifier-config.json")
    if os.path.isfile(config_path):
        return config_path
    raise FileNotFoundError(f"Cannot find verifier-config.json (tried {config_path})")


def load_config(path: str) -> dict:
    """Load and return the raw config dict."""
    with open(path, encoding="utf-8") as f:
        return json.load(f)


def build_scenario(name: str, defaults: dict, values: dict) -> Scenario:
    """Merge defaults with scenario-specific values into a Scenario."""
    merged = {**defaults, **values}
    return Scenario(
        name=name,
        github_owner=merged["GITHUB_OWNER"],
        system_domain=merged["SYSTEM_DOMAIN"],
        system_name=merged["SYSTEM_NAME"],
        architecture=merged.get("ARCHITECTURE", "monolith"),
        repository_strategy=merged.get("REPOSITORY_STRATEGY", "monorepo"),
        system_language=merged.get("SYSTEM_LANGUAGE"),
        backend_language=merged.get("BACKEND_LANGUAGE"),
        frontend_language=merged.get("FRONTEND_LANGUAGE"),
        system_test_language=merged.get("SYSTEM_TEST_LANGUAGE"),
    )


def resolve_batches(config: dict, batch_name: str | None, scenario_name: str | None) -> list[Batch]:
    """Resolve which batches/scenarios to run based on CLI args."""
    defaults = config["defaults"]
    all_batches = config["batches"]

    if scenario_name:
        # Find the scenario across all batches
        for bname, bdata in all_batches.items():
            if scenario_name in bdata["scenarios"]:
                sc = build_scenario(scenario_name, defaults, bdata["scenarios"][scenario_name])
                return [Batch(name=bname, description=bdata["description"], scenarios=[sc])]
        raise ValueError(f"Scenario '{scenario_name}' not found in any batch")

    if batch_name == "all":
        result = []
        for bname, bdata in all_batches.items():
            scenarios = [
                build_scenario(sname, defaults, svals)
                for sname, svals in bdata["scenarios"].items()
            ]
            result.append(Batch(name=bname, description=bdata["description"], scenarios=scenarios))
        return result

    if batch_name:
        if batch_name not in all_batches:
            raise ValueError(f"Batch '{batch_name}' not found. Available: {', '.join(all_batches.keys())}")
        bdata = all_batches[batch_name]
        scenarios = [
            build_scenario(sname, defaults, svals)
            for sname, svals in bdata["scenarios"].items()
        ]
        return [Batch(name=batch_name, description=bdata["description"], scenarios=scenarios)]

    raise ValueError("No batch or scenario specified. Use --batch, --scenario, or --list.")


def list_batches(config: dict) -> None:
    """Print available batches and their scenario counts."""
    all_batches = config["batches"]
    total = 0
    print("\nAvailable batches:")
    for bname, bdata in all_batches.items():
        count = len(bdata["scenarios"])
        total += count
        print(f"  {bname}: {bdata['description']} ({count} scenarios)")
    print(f"  all: Run all batches ({total} scenarios)")
    print()
