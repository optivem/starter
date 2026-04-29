#!/usr/bin/env bash
#
# Pre-commit check for system/multitier/backend-typescript
# Runs typecheck (compile) then lint.
#
set -euo pipefail

cd "$(dirname "$0")/.."

echo "  [typescript multitier backend] typecheck..."
npx --no-install tsc --noEmit

echo "  [typescript multitier backend] lint..."
npm run lint --silent
