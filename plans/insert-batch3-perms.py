#!/usr/bin/env python3
"""Insert per-job permissions into Batch 3 jobs (pipeline drivers + meta-release).

For each named job, inserts a permissions block immediately before the
`    steps:` line within that job. Job uniqueness is guaranteed by name
within each file. Skips jobs that already have permissions.
"""
import re
from pathlib import Path

JOB_PERMS = {
    "_prerelease-pipeline.yml": {
        "local": ["contents: read"],
        "commit-stage": ["contents: read", "actions: write"],
        "acceptance-stage": ["contents: read", "actions: write"],
        "acceptance-stage-legacy": ["contents: read", "actions: write"],
        "qa-stage": ["contents: read", "actions: write"],
        "qa-signoff": ["contents: read", "actions: write"],
    },
    "_meta-prerelease-pipeline.yml": {
        "local": ["contents: read", "actions: write"],
        "commit": ["contents: read", "actions: write"],
        "pipeline": ["contents: read", "actions: write"],
    },
    "meta-release-stage.yml": {
        "promote-monolith-java": ["contents: read", "actions: write"],
        "promote-monolith-dotnet": ["contents: read", "actions: write"],
        "promote-monolith-typescript": ["contents: read", "actions: write"],
        "promote-multitier-java": ["contents: read", "actions: write"],
        "promote-multitier-dotnet": ["contents: read", "actions: write"],
        "promote-multitier-typescript": ["contents: read", "actions: write"],
        "trigger-gh-acceptance": ["contents: read"],
    },
    "bump-patch-version-multirepo.yml": {
        "dispatch": [],  # empty -> permissions: {}
    },
}

base = Path("C:/Users/valen/Documents/GitHub/optivem/academy/shop/.github/workflows")
JOB_BOUNDARY_RE = re.compile(r"^  ([a-z][a-z0-9-]*):\s*$")
PERM_LINE_RE = re.compile(r"^    permissions:")
STEPS_LINE_RE = re.compile(r"^    steps:\s*$")

inserted = 0
for fname, perm_map in JOB_PERMS.items():
    path = base / fname
    lines = path.read_text(encoding='utf-8').split("\n")
    new_lines = []
    i = 0
    while i < len(lines):
        line = lines[i]
        m = JOB_BOUNDARY_RE.match(line)
        if not m or m.group(1) not in perm_map:
            new_lines.append(line)
            i += 1
            continue
        # We're at a target job
        job_name = m.group(1)
        new_lines.append(line)
        i += 1
        block = []
        while i < len(lines):
            if JOB_BOUNDARY_RE.match(lines[i]):
                break
            block.append(lines[i])
            i += 1
        # Skip if already has permissions
        if any(PERM_LINE_RE.match(b) for b in block):
            new_lines.extend(block)
            continue
        # Build permission lines
        perms = perm_map[job_name]
        if not perms:
            perm_block = ["    permissions: {}"]
        else:
            perm_block = ["    permissions:"]
            for p in perms:
                perm_block.append(f"      {p}")
        # Insert before first `    steps:` line in block
        out_block = []
        ins = False
        for b in block:
            if not ins and STEPS_LINE_RE.match(b):
                out_block.extend(perm_block)
                out_block.append(b)
                ins = True
            else:
                out_block.append(b)
        if ins:
            inserted += 1
            new_lines.extend(out_block)
        else:
            print(f"WARN: {fname} job {job_name}: no steps: line found")
            new_lines.extend(block)
    path.write_text("\n".join(new_lines), encoding='utf-8')

print(f"Inserted permissions in {inserted} jobs across {len(JOB_PERMS)} files")
