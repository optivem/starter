#!/usr/bin/env bash
#
# Pre-commit check for system/multitier/backend-java
# Runs compile (main + test sources) then checkstyle.
#
set -euo pipefail

cd "$(dirname "$0")/.."

echo "  [java multitier] compile..."
./gradlew --quiet compileJava compileTestJava

echo "  [java multitier] checkstyle..."
./gradlew --quiet checkstyleMain
