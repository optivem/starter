#!/usr/bin/env bash
# Runs SonarScanner analysis on the monolith .NET app.
#
# Local helper that pushes a SonarCloud analysis using your personal token.
# CI runs the same analysis from monolith-dotnet-acceptance-stage.yml; this
# script is for manual runs.
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

echo "Running SonarScanner for monolith .NET..."

dotnet tool install --global dotnet-sonarscanner 2>/dev/null || true

dotnet sonarscanner begin \
    /k:"optivem_shop-monolith-dotnet" \
    /n:"shop-monolith-dotnet" \
    /o:"optivem" \
    /d:sonar.host.url="https://sonarcloud.io" \
    /d:sonar.token="$TOKEN"

dotnet build MyCompany.MyShop.Monolith.sln --no-incremental

dotnet sonarscanner end /d:sonar.token="$TOKEN"

echo "Sonar analysis complete."
