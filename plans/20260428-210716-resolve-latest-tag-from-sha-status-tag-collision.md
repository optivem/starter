# Plan — Fix `resolve-latest-tag-from-sha` collision with promotion-status tags

**Date:** 2026-04-28
**Trigger:** Failing run [25057820020](https://github.com/optivem/shop/actions/runs/25057820020) — `meta-prerelease-stage` → `run / pipeline (monolith-dotnet)` → inner `verify / qa-stage` (run 25065077412) failed at "Simulate Deployment (QA Environment)" with `manifest unknown` from GHCR.
**Scope:** Bug in the way the prerelease pipeline resolves the rc tag for a tested SHA when status-suffixed promotion tags also exist on that SHA. Affects all flavors (monolith/multitier × dotnet/java/typescript), not just monolith-dotnet.

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

The race-tolerance comment at `_prerelease-pipeline.yml:222-225` — "resolve the rc tag that actually points at the tested SHA — whether our dispatched run created it or a concurrent scheduled run did" — also reveals the assumption: only rc-creating workflows tag the SHA. The status-tag-creating workflow (qa-stage publish-tag) violates that assumption.

---

## Proposed fix

The rc-tag pattern needs to mean **"`rc.<digits>`, with no further suffix"**. The four candidates, ranked:

### A. (Preferred) Add regex support to `resolve-latest-tag-from-sha`

Add a `pattern-format: glob|regex` input (default `glob` for back-compat). Callers needing anchored matches use regex:

```yaml
- id: get-version
  uses: optivem/actions/resolve-latest-tag-from-sha@v1
  with:
    commit-sha: ${{ env.COMMIT_SHA }}
    pattern: '^${{ inputs.prefix }}-v${{ needs.check-version.outputs.base-version }}-rc\.[0-9]+$'
    pattern-format: regex
```

**Pros:** clean, expressive, future-proof. Fixes the same trap for every other current and future caller of this action — glob is structurally too weak for tag schemes that use `-` as a structural separator.
**Cons:** small API surface change in the shared action; needs a release of `optivem/actions`.
**Risk:** low — back-compat preserved by default-glob.

### B. Add an `exclude-pattern` input to `resolve-latest-tag-from-sha`

Action filters out tags matching `exclude-pattern` after the include-match:

```yaml
exclude-pattern: '*-rc.*-*'
```

**Pros:** smaller change than (A); preserves glob semantics.
**Cons:** denylist; every new status suffix risks drift between this caller and `compose-prerelease-status` which owns the suffix vocabulary. Subtler to reason about.
**Risk:** medium — easy to forget to update when adding a new promotion stage.

### C. Post-filter in the pipeline (stop-gap)

Leave the action unchanged; in `_prerelease-pipeline.yml` after the resolve step, run a tighter regex filter on the captured tag and override `outputs.version` if it contains anything past `rc.<digits>`. One-liner:

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

Then read `steps.get-version-strict.outputs.tag` instead. If the resolve action returned `…-qa-deployed`, this drops it to empty and the downstream "Fail If No RC Tag On SHA" guard fires correctly.

**Pros:** zero changes to the shared action; ships immediately, unblocks the pipeline.
**Cons:** does not fix the bug for any other caller of the same action; pushes domain knowledge out of the shared action and into the caller. **Also has a subtle false-negative**: when a single SHA carries both `rc.712` and `rc.712-qa-deployed`, the action returns `…-qa-deployed`, the post-filter rejects it, and the guard fires — even though `rc.712` is right there and would have been correct. So (C) on its own can fail-loud where (A)/(B) succeed-correct. Acceptable for a stop-gap because failing loud is better than wrong-image-pull, but not acceptable as the long-term answer.
**Risk:** low — strictly narrows what the pipeline accepts.

### D. Stop using git tags for promotion state

Move promotion markers (`-qa-deployed`, `-qa-approved`, `-acceptance-tested`) out of git tags entirely — into GitHub Deployments, GHCR labels, or a dedicated state file.

**Pros:** removes the entire class of collision; matches industry practice (git tags = immutable artifact identity, not mutable lifecycle state).
**Cons:** large blast radius — every workflow that reads or filters tags would need to change; new infrastructure for state. Not the right fix for *this* bug.

---

## Recommendation

**Apply (A) in `optivem/actions` as the durable fix; ship (C) in `optivem/shop` immediately as a stop-gap so the pipeline isn't blocked while (A) is reviewed and released.**

Sequencing:

1. **Stop-gap, today** — patch `_prerelease-pipeline.yml:226-230` per (C). Verify by re-dispatching `meta-prerelease-stage` against the same SHA. Expectation: pipeline now fails loudly at "Fail If No RC Tag On SHA" because `…-qa-deployed` is rejected and no bare rc tag points at this SHA. That is correct behavior — the SHA has already been QA-promoted; re-running the verification pipeline on it is meaningless and should refuse to proceed.

2. **Durable fix** — add `pattern-format: regex` support to `optivem/actions/resolve-latest-tag-from-sha`. Cover with a unit case where the same SHA carries `rc.N`, `rc.N-qa-approved`, `rc.N-qa-deployed` and the regex `^…-rc\.[0-9]+$` returns `rc.N`.

3. **Migrate callers** — switch `_prerelease-pipeline.yml` (and any other call site of `resolve-latest-tag-from-sha` that wants strict matching) to `pattern-format: regex` with anchored patterns. Remove the (C) stop-gap once the action is updated.

4. **Audit other call sites** — grep `optivem/shop/.github/workflows` for `resolve-latest-tag-from-sha` and verify each pattern is robust against status-tag aliasing on the same SHA. Patterns that currently look like `…-rc.*` are all suspect.

## Out of scope

- Why `meta-prerelease-stage` re-targets a SHA that is already QA-promoted instead of refusing up-front. That is a separate question — possibly a desirable behavior (re-verification) and possibly a misconfiguration. Track separately.
- Whether status tags should remain in git at all (option D above). Track separately if the question matters; not blocking for this fix.

## Risk assessment

- **(C) stop-gap**: very low. Strictly narrows accepted output. Worst case: pipeline fails loudly on a SHA where it would otherwise have done the wrong thing.
- **(A) durable**: low. Back-compat default keeps existing callers working; only callers that opt into `pattern-format: regex` get the new behavior. Needs a test for the new code path.
- **Doing nothing**: high. Any future re-run of `meta-prerelease-stage` against an already-promoted SHA reproduces the failure for any flavor, blocking the meta-prerelease-stage tag-meta-rc job and silently confusing operators.
