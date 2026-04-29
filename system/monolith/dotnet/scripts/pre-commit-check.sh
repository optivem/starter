#!/usr/bin/env bash
#
# Pre-commit check for system/monolith/dotnet
# Runs build (compile) then format verification.
#
set -euo pipefail

cd "$(dirname "$0")/.."

SLN_FILE=$(ls *.slnx 2>/dev/null || ls *.sln 2>/dev/null || true)
if [ -z "$SLN_FILE" ]; then
    echo "  [dotnet monolith] ERROR: no .slnx or .sln file found"
    exit 1
fi

echo "  [dotnet monolith] build..."
dotnet build "$SLN_FILE" --nologo --verbosity quiet

echo "  [dotnet monolith] format check..."
dotnet format "$SLN_FILE" --verify-no-changes --verbosity quiet
