#!/usr/bin/env bash
#
# Pre-commit check for system/multitier/frontend-react
# This project's `lint` script is `tsc --noEmit`, so compile == lint here.
# We run it once.
#
set -euo pipefail

cd "$(dirname "$0")/.."

echo "  [frontend-react] typecheck (compile + lint)..."
npx --no-install tsc --noEmit
