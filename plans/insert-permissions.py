#!/usr/bin/env python3
"""Insert `permissions: {}` at the top level of each workflow file in the
list, immediately after the `on:` block ends and before the next
top-level key (concurrency / env / defaults / jobs).

Skips the file if a top-level `permissions:` already exists.
"""
import sys
from pathlib import Path

FILES = [
    # 74 workflows with existing per-job permissions
    "_meta-prerelease-pipeline.yml",
    "_prerelease-pipeline.yml",
    "bump-patch-version.yml",
    "bump-patch-version-meta.yml",
    "bump-patch-version-monolith-dotnet.yml",
    "bump-patch-version-monolith-java.yml",
    "bump-patch-version-monolith-typescript.yml",
    "bump-patch-version-multitier-backend-dotnet.yml",
    "bump-patch-version-multitier-backend-java.yml",
    "bump-patch-version-multitier-backend-typescript.yml",
    "bump-patch-version-multitier-dotnet.yml",
    "bump-patch-version-multitier-frontend-react.yml",
    "bump-patch-version-multitier-java.yml",
    "bump-patch-version-multitier-typescript.yml",
    "cleanup.yml",
    "cross-lang-system-verification.yml",
    "meta-prerelease-dry-run.yml",
    "meta-prerelease-stage.yml",
    "meta-release-stage.yml",
    "monolith-dotnet-acceptance-stage.yml",
    "monolith-dotnet-acceptance-stage-cloud.yml",
    "monolith-dotnet-acceptance-stage-legacy.yml",
    "monolith-dotnet-commit-stage.yml",
    "monolith-dotnet-prod-stage.yml",
    "monolith-dotnet-prod-stage-cloud.yml",
    "monolith-dotnet-qa-signoff.yml",
    "monolith-dotnet-qa-stage.yml",
    "monolith-dotnet-qa-stage-cloud.yml",
    "monolith-java-acceptance-stage.yml",
    "monolith-java-acceptance-stage-cloud.yml",
    "monolith-java-acceptance-stage-legacy.yml",
    "monolith-java-commit-stage.yml",
    "monolith-java-prod-stage.yml",
    "monolith-java-prod-stage-cloud.yml",
    "monolith-java-qa-signoff.yml",
    "monolith-java-qa-stage.yml",
    "monolith-java-qa-stage-cloud.yml",
    "monolith-typescript-acceptance-stage.yml",
    "monolith-typescript-acceptance-stage-cloud.yml",
    "monolith-typescript-acceptance-stage-legacy.yml",
    "monolith-typescript-commit-stage.yml",
    "monolith-typescript-prod-stage.yml",
    "monolith-typescript-prod-stage-cloud.yml",
    "monolith-typescript-qa-signoff.yml",
    "monolith-typescript-qa-stage.yml",
    "monolith-typescript-qa-stage-cloud.yml",
    "multitier-backend-dotnet-commit-stage.yml",
    "multitier-backend-java-commit-stage.yml",
    "multitier-backend-typescript-commit-stage.yml",
    "multitier-dotnet-acceptance-stage.yml",
    "multitier-dotnet-acceptance-stage-cloud.yml",
    "multitier-dotnet-acceptance-stage-legacy.yml",
    "multitier-dotnet-prod-stage.yml",
    "multitier-dotnet-prod-stage-cloud.yml",
    "multitier-dotnet-qa-signoff.yml",
    "multitier-dotnet-qa-stage.yml",
    "multitier-dotnet-qa-stage-cloud.yml",
    "multitier-frontend-react-commit-stage.yml",
    "multitier-java-acceptance-stage.yml",
    "multitier-java-acceptance-stage-cloud.yml",
    "multitier-java-acceptance-stage-legacy.yml",
    "multitier-java-prod-stage.yml",
    "multitier-java-prod-stage-cloud.yml",
    "multitier-java-qa-signoff.yml",
    "multitier-java-qa-stage.yml",
    "multitier-java-qa-stage-cloud.yml",
    "multitier-typescript-acceptance-stage.yml",
    "multitier-typescript-acceptance-stage-cloud.yml",
    "multitier-typescript-acceptance-stage-legacy.yml",
    "multitier-typescript-prod-stage.yml",
    "multitier-typescript-prod-stage-cloud.yml",
    "multitier-typescript-qa-signoff.yml",
    "multitier-typescript-qa-stage.yml",
    "multitier-typescript-qa-stage-cloud.yml",
    # bump-patch-version-multirepo (no perms at all, but uses PAT not GITHUB_TOKEN)
    "bump-patch-version-multirepo.yml",
]

TOP_LEVEL_KEYS = ("concurrency:", "env:", "defaults:", "jobs:", "permissions:")

base = Path("C:/Users/valen/Documents/GitHub/optivem/academy/shop/.github/workflows")
changed = []
skipped = []
errors = []

for fname in FILES:
    path = base / fname
    if not path.exists():
        errors.append(f"{fname}: file not found")
        continue

    text = path.read_text()
    lines = text.split("\n")

    # Skip if file already has top-level permissions
    if any(line.startswith("permissions:") for line in lines):
        skipped.append(f"{fname}: already has top-level permissions")
        continue

    # Find first top-level key from TOP_LEVEL_KEYS that appears AFTER on:
    on_idx = None
    for i, line in enumerate(lines):
        if line.startswith("on:"):
            on_idx = i
            break
    if on_idx is None:
        errors.append(f"{fname}: no `on:` block found")
        continue

    insert_idx = None
    for i in range(on_idx + 1, len(lines)):
        line = lines[i]
        # Skip indented lines (children of on:) and blank lines
        if line.startswith(" ") or line.startswith("\t") or line == "":
            continue
        # First non-indented line — check if it's a known top-level key
        if any(line.startswith(k) for k in TOP_LEVEL_KEYS):
            insert_idx = i
            break
        # Some other top-level key we didn't expect — also valid insertion point
        if line and not line.startswith("#"):
            insert_idx = i
            break

    if insert_idx is None:
        errors.append(f"{fname}: could not find insertion point after on:")
        continue

    # Insert "permissions: {}\n\n" before the insertion line.
    # Split: lines[:insert_idx] + ["permissions: {}", ""] + lines[insert_idx:]
    new_lines = lines[:insert_idx] + ["permissions: {}", ""] + lines[insert_idx:]
    new_text = "\n".join(new_lines)

    path.write_text(new_text)
    changed.append(fname)

print(f"Changed: {len(changed)} files")
print(f"Skipped: {len(skipped)} files")
print(f"Errors:  {len(errors)} files")
for e in errors:
    print(f"  ERROR: {e}")
for s in skipped:
    print(f"  SKIP:  {s}")
