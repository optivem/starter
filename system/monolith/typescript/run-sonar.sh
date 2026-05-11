#!/usr/bin/env bash
# Runs SonarScanner analysis on the monolith TypeScript app.
#
# Local helper that pushes a SonarCloud analysis using your personal token.
# CI runs the same analysis from monolith-typescript-acceptance-stage.yml;
# this script is for manual runs.
# Ignore rules in sonar-project.properties (auto-loaded from this dir).
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

echo "Running SonarScanner for monolith TypeScript..."

npx -y sonarqube-scanner \
    "-Dsonar.projectKey=optivem_shop-monolith-typescript" \
    "-Dsonar.projectName=shop-monolith-typescript" \
    "-Dsonar.organization=optivem" \
    "-Dsonar.host.url=https://sonarcloud.io" \
    "-Dsonar.token=$TOKEN" \
    "-Dsonar.sources=src"

echo "Sonar analysis complete."
