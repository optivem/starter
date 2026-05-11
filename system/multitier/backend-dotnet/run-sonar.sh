#!/usr/bin/env bash
# Runs SonarScanner analysis on the multitier .NET backend.
#
# Local helper that pushes a SonarCloud analysis using your personal token.
# CI runs the same analysis from multitier-dotnet-acceptance-stage.yml;
# this script is for manual runs.
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

echo "Running SonarScanner for multitier .NET backend..."

dotnet tool install --global dotnet-sonarscanner 2>/dev/null || true

dotnet sonarscanner begin \
    /k:"optivem_shop-multitier-backend-dotnet" \
    /n:"shop-multitier-backend-dotnet" \
    /o:"optivem" \
    /d:sonar.host.url="https://sonarcloud.io" \
    /d:sonar.token="$TOKEN"

dotnet build MyCompany.MyShop.Backend.slnx --no-incremental

dotnet sonarscanner end /d:sonar.token="$TOKEN"

echo "Sonar analysis complete."
