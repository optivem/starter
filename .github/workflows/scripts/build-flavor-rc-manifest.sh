#!/usr/bin/env bash
# build-flavor-rc-manifest.sh — assemble the per-flavor RC manifest that
# meta-prerelease tag-meta-rc embeds in its annotated tag body, so that
# meta-release-stage can promote exactly the RCs that were tested together.
#
# Each flavor's flavor.VERSION is independent of the root meta.VERSION. For
# each flavor we read its own VERSION file, find the latest
# <flavor>-v<flavor-VERSION>-rc.* tag whose commit has a qa/signoff=success
# commit-status, and record that tag in the manifest.
#
# Usage:
#   META_RC=meta-v1.0.88-rc.317 REPO=optivem/shop GH_TOKEN=... \
#     build-flavor-rc-manifest.sh > manifest.json
#
# Hard fails if any canonical flavor has no qa-approved RC at its current
# flavor.VERSION — the meta-rc must not be tagged with an incomplete bundle.

set -euo pipefail

: "${META_RC:?must be set}"
: "${REPO:?must be set}"
: "${GH_TOKEN:?must be set}"

SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
# shellcheck disable=SC1091
source "$SCRIPT_DIR/gh-retry.sh"

FLAVORS_DEFAULT="monolith-java monolith-dotnet monolith-typescript multitier-java multitier-dotnet multitier-typescript"
FLAVORS="${FLAVORS:-$FLAVORS_DEFAULT}"

flavor_version_path() {
  case "$1" in
    monolith-java)        echo "system/monolith/java/VERSION" ;;
    monolith-dotnet)      echo "system/monolith/dotnet/VERSION" ;;
    monolith-typescript)  echo "system/monolith/typescript/VERSION" ;;
    multitier-java)       echo "system/multitier/java/VERSION" ;;
    multitier-dotnet)     echo "system/multitier/dotnet/VERSION" ;;
    multitier-typescript) echo "system/multitier/typescript/VERSION" ;;
    *) echo "::error::unknown flavor: $1" >&2; return 1 ;;
  esac
}

flavors_json=$(jq -n '{}')
for flavor in $FLAVORS; do
  path=$(flavor_version_path "$flavor")
  version=$(tr -d '[:space:]' < "$path")
  escaped_version="${version//./\\.}"

  mapfile -t rc_tags < <(git tag --list "${flavor}-v${version}-rc.*" \
    | awk -v p="^${flavor}-v${escaped_version}-rc\\.[0-9]+$" '$0 ~ p' \
    | awk -F'-rc[.]' '{print $NF " " $0}' \
    | sort -k1,1 -nr \
    | awk '{print $2}')

  rc=""
  for tag in "${rc_tags[@]}"; do
    sha=$(git rev-parse "${tag}^{}" 2>/dev/null || git rev-parse "${tag}")
    statuses=$(gh_retry api "repos/${REPO}/commits/${sha}/statuses" --paginate)
    approved=$(echo "$statuses" | jq '[.[] | select(.context=="qa/signoff" and .state=="success")] | length')
    if [[ "${approved:-0}" -gt 0 ]]; then
      rc="${tag}"
      break
    fi
  done
  if [[ -z "$rc" ]]; then
    echo "::error::No QA-approved RC (qa/signoff commit-status with state=success) found for ${flavor} at v${version}. Was the per-flavor acceptance pipeline run to completion and qa-signoff dispatched?" >&2
    exit 1
  fi
  flavors_json=$(jq --arg k "$flavor" --arg v "$rc" '. + {($k): $v}' <<< "$flavors_json")
  echo "Resolved ${flavor} -> ${rc}" >&2
done

now=$(date -u +%Y-%m-%dT%H:%M:%SZ)
jq -n \
  --arg meta_rc "$META_RC" \
  --arg tested_at "$now" \
  --argjson flavors "$flavors_json" \
  '{meta_rc: $meta_rc, tested_at: $tested_at, flavors: $flavors}'
