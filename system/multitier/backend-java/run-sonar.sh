#!/usr/bin/env bash
# Runs SonarScanner analysis on the multitier Java backend.
#
# Local helper that pushes a SonarCloud analysis using your personal token.
# CI runs the same analysis from multitier-java-acceptance-stage.yml;
# this script is for manual runs.
# Project key: optivem_shop-multitier-backend-java (config in build.gradle).
# Get token: https://sonarcloud.io/account/security
#
# Usage: ./run-sonar.sh [TOKEN]
#        SONAR_TOKEN=<t> ./run-sonar.sh

set -euo pipefail

TOKEN="${1:-${SONAR_TOKEN:-}}"
if [ -z "$TOKEN" ]; then
  echo "ERROR: Sonar token required. Set SONAR_TOKEN env var or pass as first arg." >&2
  echo "Get token: https://sonarcloud.io/account/security" >&2
  exit 1
fi

echo "Running SonarScanner for multitier Java backend..."

./gradlew build sonar --info "-Dsonar.token=$TOKEN"

echo "Sonar analysis complete."
