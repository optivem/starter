#!/usr/bin/env bash
#
# Pre-commit check for system/monolith/typescript
# Runs typecheck (compile) then lint.
#
set -euo pipefail

cd "$(dirname "$0")/.."

echo "  [typescript monolith] typecheck..."
npx --no-install tsc --noEmit

echo "  [typescript monolith] lint..."
npm run lint --silent
