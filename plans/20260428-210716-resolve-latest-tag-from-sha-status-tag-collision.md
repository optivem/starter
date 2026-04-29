# Plan — Fix `resolve-latest-tag-from-sha` collision with promotion-status tags

**Date:** 2026-04-28
**Trigger:** Failing run [25057820020](https://github.com/optivem/shop/actions/runs/25057820020) — `meta-prerelease-stage` → `run / pipeline (monolith-dotnet)` → inner `verify / qa-stage` (run 25065077412) failed at "Simulate Deployment (QA Environment)" with `manifest unknown` from GHCR.
**Scope:** Stop-gap fix to unblock the prerelease pipeline when status-suffixed promotion tags exist on a tested SHA. Affects all flavors (monolith/multitier × dotnet/java/typescript). The durable answer — moving promotion / verification state out of git tags entirely — is tracked separately in [`20260429-070355-migrate-promotion-state-out-of-git-tags.md`](20260429-070355-migrate-promotion-state-out-of-git-tags.md). This plan ships the immediate stop-gap and closes; the migration plan is the long-term work.

---

## Symptom

`monolith-dotnet-qa-stage` tried to pull:

```
ghcr.io/optivem/shop/monolith-system-dotnet:monolith-dotnet-v1.0.56-rc.712-qa-deployed
```

GHCR returned `manifest unknown`. That image tag has no manifest because **status tags (`-qa-deployed`, `-qa-approved`, `-acceptance-tested`) are git-only markers** — they are never re-applied to Docker images. The image is only ever published with the bare rc tag (e.g. `:monolith-dotnet-v1.0.56-rc.712`).

## Root cause

`_prerelease-pipeline.yml:226-230` resolves the rc tag for the tested SHA via:

```yaml
- id: get-version
  uses: optivem/actions/resolve-latest-tag-from-sha@v1
  with:
    commit-sha: ${{ env.COMMIT_SHA }}
    pattern: ${{ inputs.prefix }}-v${{ needs.check-version.outputs.base-version }}-rc.*
```

`resolve-latest-tag-from-sha` (`actions/resolve-latest-tag-from-sha/action.yml:55-68`):

1. `git ls-remote --tags` → all tag refs.
2. Keep only tags whose SHA equals the requested SHA (so it IS strict about the SHA — no ancestor resolution).
3. Apply the bash glob `pattern` via `case`.
4. `sort -V | tail -1`.

In the failing run, `meta-prerelease-stage` pinned the pipeline to SHA `d35a6a1d` (resolved from `optivem/shop@main` at dispatch time). Three tags point at exactly that SHA:

```
monolith-dotnet-v1.0.56-rc.712
monolith-dotnet-v1.0.56-rc.712-qa-approved
monolith-dotnet-v1.0.56-rc.712-qa-deployed
```

The glob `monolith-dotnet-v1.0.56-rc.*` — because bash glob `*` happily crosses `-` boundaries — matches **all three**. `sort -V | tail -1` picks the lexicographically-largest one: `monolith-dotnet-v1.0.56-rc.712-qa-deployed`. That string is then handed to `qa-stage` as `version`, which composes a non-existent image URL and the deploy step blows up.

## Why this is a latent bug, not a new one

The same shape would have failed on **any** SHA that already carried a QA-promoted state, for any flavor. It only surfaces when the same SHA is re-run through the pipeline after promotion tags have been applied — which had not happened until this run. The rc.712 SHA had already been promoted to `qa-approved` and `qa-deployed` by an earlier QA run, then meta-prerelease re-targeted that SHA.

## Intention vs. behavior

The pattern `<prefix>-v<version>-rc.*` was meant to capture **release candidates** — the pre-promotion artifacts whose names are exactly `rc.<n>`. Status-suffixed tags are **promotion markers** layered on top of an rc; semantically they are not different rcs, they are state of the same rc.

The author's mental model: "for a given SHA, there's one bare rc tag matching `-rc.*`." Correct on a virgin SHA; broken once promotion has occurred. Bash glob does not have a way to express "ends with `rc.<digits>` and no further suffix", so the pattern silently widens the moment status tags appear.

The deeper issue — git tags being used to store mutable lifecycle state — is what the migration plan addresses. This plan only stops the immediate bleeding.

---

## Stop-gap fix

Add a post-resolve regex filter in `_prerelease-pipeline.yml` after the existing `get-version` step:

```yaml
- id: get-version-strict
  shell: bash
  env:
    RAW: ${{ steps.get-version.outputs.tag }}
    PATTERN: '^${{ inputs.prefix }}-v${{ needs.check-version.outputs.base-version }}-rc\.[0-9]+$'
  run: |
    if [[ "$RAW" =~ $PATTERN ]]; then
      echo "tag=$RAW" >> "$GITHUB_OUTPUT"
    else
      echo "tag=" >> "$GITHUB_OUTPUT"
    fi
```

Read `steps.get-version-strict.outputs.tag` instead of `steps.get-version.outputs.tag` for the rest of the pipeline.

### Behavior after fix

If `resolve-latest-tag-from-sha` returns `…-qa-deployed`, the strict filter drops it to empty and the downstream "Fail If No RC Tag On SHA" guard fires correctly. That is the right behavior — the SHA has already been QA-promoted; re-running the verification pipeline on it is meaningless and should refuse to proceed.

### Known false-negative

When a SHA carries both `rc.712` and `rc.712-qa-deployed`, the action returns `…-qa-deployed`, the post-filter rejects it, and the guard fires — even though `rc.712` is right there and would have been correct.

Acceptable for a stop-gap because failing loud is better than wrong-image-pull. Goes away once the migration plan stops new status-suffix tags from being written (no SHA will carry both anymore).

### Verification

Re-dispatch `meta-prerelease-stage` against the failing SHA. Expectation: pipeline now fails at "Fail If No RC Tag On SHA" instead of pulling a non-existent image.

---

## Out of scope

- Moving promotion / verification state out of git tags entirely. Tracked in [`20260429-070355-migrate-promotion-state-out-of-git-tags.md`](20260429-070355-migrate-promotion-state-out-of-git-tags.md).
- Why `meta-prerelease-stage` re-targets a SHA that is already QA-promoted instead of refusing up-front. Possibly desirable (re-verification), possibly a misconfiguration. Track separately.
- Adding regex / exclude-pattern support to `resolve-latest-tag-from-sha` itself (originally proposed as options A/B in this plan). Becomes redundant once the migration plan completes — no new status-suffix tags will be written, so the glob has nothing to collide with. Discardable.

---

## Risk assessment

- **Stop-gap:** very low. Strictly narrows accepted output. Worst case: pipeline fails loudly on a SHA where it would otherwise have done the wrong thing. Reversible by removing one step.
- **Doing nothing:** high. Any future re-run of `meta-prerelease-stage` against an already-promoted SHA reproduces the failure for any flavor, blocking the meta-prerelease-stage tag-meta-rc job and silently confusing operators.
