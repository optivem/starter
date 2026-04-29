#!/usr/bin/env bash
#
# Pre-commit check for system/monolith/java
# Runs compile (main + test sources) then checkstyle.
#
set -euo pipefail

cd "$(dirname "$0")/.."

echo "  [java monolith] compile..."
./gradlew --quiet compileJava compileTestJava

echo "  [java monolith] checkstyle..."
./gradlew --quiet checkstyleMain
