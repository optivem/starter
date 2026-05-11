#!/usr/bin/env bash
# Compile every shop variant defined by a gh-optivem-*.yaml in the repo root.
#
# Each YAML is one architecture/language variant of the shop template (e.g.
# monolith-java, multitier-dotnet). For each YAML we invoke
# `gh optivem compile -c <yaml>`, which dispatches per-language compile
# commands (dotnet build / gradlew compileJava compileTestJava / npm ci +
# tsc --noEmit) against the tiers listed in that YAML. The per-language
# logic lives in `internal/compiler` in the gh-optivem repo; this script
# only fans out across variants.
#
# Adding a new variant: drop a new gh-optivem-<arch>-<lang>.yaml in the
# repo root — no changes to this script. To run a single variant on its
# own use `gh optivem compile -c gh-optivem-<arch>-<lang>.yaml` directly.
#
# `*-legacy.yaml` variants are excluded — they share `system.config` and
# `system.path` with their non-legacy sibling and differ only in the
# `system_test.config` pointer, so compile is pure duplicate work.
#
# Exits non-zero on any failure (zero-failures policy). Continues past the
# first failure so a single run reports every broken variant.

set -uo pipefail

REPO_ROOT="$(cd "$(dirname "$0")" && pwd)"
cd "$REPO_ROOT"

shopt -s nullglob extglob
configs=(gh-optivem-!(*-legacy).yaml)
shopt -u nullglob extglob

if [ ${#configs[@]} -eq 0 ]; then
  echo "ERROR: no gh-optivem-*.yaml files found in $REPO_ROOT" >&2
  exit 1
fi

# Result rows: "config|status|duration_seconds"
declare -a RESULTS=()
OVERALL_START=$(date +%s)

for cfg in "${configs[@]}"; do
  echo
  echo "=================================================================="
  echo "  $cfg"
  echo "=================================================================="

  start=$(date +%s)
  status="PASSED"
  gh optivem compile -c "$cfg" || status="FAILED"
  end=$(date +%s)
  RESULTS+=("$cfg|$status|$((end - start))")
done

OVERALL_END=$(date +%s)

# Summary
printf "\n==================================================================\n"
printf "  SUMMARY\n"
printf "==================================================================\n\n"
printf "%-44s %-10s %s\n" "Config" "Result" "Duration"
printf -- "------------------------------------------------------------------\n"

failures=0
for row in "${RESULTS[@]}"; do
  IFS='|' read -r cfg status dur <<< "$row"
  printf "%-44s %-10s %02d:%02d\n" "$cfg" "$status" $((dur/60)) $((dur%60))
  if [ "$status" = "FAILED" ]; then failures=$((failures+1)); fi
done

printf -- "------------------------------------------------------------------\n"
total_dur=$((OVERALL_END - OVERALL_START))
printf "Total duration: %02d:%02d\n" $((total_dur/60)) $((total_dur%60))

if [ "$failures" -gt 0 ]; then
  printf "\n%d variant(s) FAILED to compile.\n" "$failures" >&2
  exit 1
fi
printf "\nAll variants compiled cleanly.\n"
