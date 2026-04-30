#!/usr/bin/env python3
"""For each test-* job (test-smoke-*, test-acceptance-*, test-contract-*, test-e2e-*)
in the 6 cloud acceptance workflows, insert `permissions:\\n      contents: read`
between `runs-on:` and `steps:` IF no permissions block exists for the job.
"""
import re
from pathlib import Path

FILES = [
    "monolith-dotnet-acceptance-stage-cloud.yml",
    "monolith-java-acceptance-stage-cloud.yml",
    "monolith-typescript-acceptance-stage-cloud.yml",
    "multitier-dotnet-acceptance-stage-cloud.yml",
    "multitier-java-acceptance-stage-cloud.yml",
    "multitier-typescript-acceptance-stage-cloud.yml",
]

base = Path("C:/Users/valen/Documents/GitHub/optivem/academy/shop/.github/workflows")
TEST_JOB_RE = re.compile(r"^  (test-[a-z0-9-]+):\s*$")
JOB_BOUNDARY_RE = re.compile(r"^  [a-z][a-z0-9-]*:\s*$")  # any top-level job
PERM_LINE = re.compile(r"^    permissions:")
STEPS_LINE = re.compile(r"^    steps:\s*$")
RUNS_ON_LINE = re.compile(r"^    runs-on:")

inserts_total = 0
for fname in FILES:
    path = base / fname
    lines = path.read_text(encoding='utf-8').split("\n")
    new_lines = []
    i = 0
    while i < len(lines):
        line = lines[i]
        m = TEST_JOB_RE.match(line)
        if not m:
            new_lines.append(line)
            i += 1
            continue
        # Found a test-* job. Capture the block.
        block_start = i
        new_lines.append(line)
        i += 1
        # Walk until next job boundary or end
        block_lines = []
        while i < len(lines):
            if JOB_BOUNDARY_RE.match(lines[i]):
                break
            block_lines.append(lines[i])
            i += 1
        # Check if block already has permissions:
        has_perms = any(PERM_LINE.match(bl) for bl in block_lines)
        if has_perms:
            new_lines.extend(block_lines)
            continue
        # Insert "    permissions:\n      contents: read\n" before the "    steps:" line
        inserted = False
        for bl in block_lines:
            if not inserted and STEPS_LINE.match(bl):
                new_lines.append("    permissions:")
                new_lines.append("      contents: read")
                new_lines.append(bl)
                inserted = True
                inserts_total += 1
            else:
                new_lines.append(bl)
        if not inserted:
            # No steps: line found?? Just append as-is and warn
            print(f"WARN: {fname} job at line {block_start} has no steps:")
    path.write_text("\n".join(new_lines), encoding='utf-8')

print(f"Inserted permissions in {inserts_total} test-* jobs across {len(FILES)} files")
