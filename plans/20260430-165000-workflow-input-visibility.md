# Plan — Surface dispatch inputs on top-level user-dispatched workflows

**Status:** Applied
**Date:** 2026-04-30
**Trigger:** Debugging session on [prerelease-pipeline-monolith-dotnet run 25162768275](https://github.com/optivem/shop/actions/runs/25162768275) — needed to trace why `acceptance-stage-legacy` reported `skipped` while `acceptance-stage` ran. Root cause was `debug-skip-tests: true` propagating from the meta-pipeline through both stages, but the flag was not visible in the run title or in any early job log; it only surfaced inside the wretry-action's `github_context` dump deep in the legacy `check` job logs.

---

## Problem

For a manually dispatched run (or a run dispatched by a parent workflow), GitHub Actions exposes the inputs in two places only:

1. The Actions UI summary if the workflow declares a `run-name` that interpolates `inputs.*`.
2. The `with:` block dumped in each job's "Set up job" expandable section.

Neither is convenient when:

- A run was dispatched with debug or skip flags and you need to know at a glance whether the run is real or a debug iteration.
- A pipeline forks — e.g. the meta pipeline dispatches both `acceptance-stage.yml` and `acceptance-stage-legacy.yml`, and the two stages handle the same input differently (job-level skip vs. step-level skip). Without the inputs visible early, the asymmetry looks like a bug.
- A workflow is reached via several layers (meta → reusable → per-flavor → leaf). The dispatcher annotation surfaces the workflow filename but not the input payload.

We do not need a uniform "echo every input on every workflow" treatment. The reusable inner workflows already have their inputs in the implicit "Set up job" dump, and adding `echo` steps there just creates a third copy that must be kept in sync with input definitions.

---

## Recommendation

Apply the visibility pattern only at the **top of the dispatch graph** — workflows that humans dispatch directly with knobs that change behaviour:

1. **Rich `run-name`** that surfaces non-default flag values as suffix tokens, e.g. `[debug-skip-tests]`, `[skip-local]`, `[level:commit]`, `[pinned-sha]`. Cheap, no runner cost, immediately visible in the Actions UI run list.
2. **A single "Print Inputs" step** at the top of the first non-reusable job (where one exists) — duplicates `event`, `schedule`, and the key flags so a single click into the run also shows them. Adds one short step, no extra runner.

Skip both treatments on:

- Reusable workflows (`_meta-prerelease-pipeline.yml`, `_prerelease-pipeline.yml`) — caller's `with:` block is the source of truth and is already logged in their "Set up job" section.
- Leaf stage workflows (`*-acceptance-stage*.yml`, `*-commit-stage.yml`, `*-qa-stage.yml`, `*-prod-stage.yml`) — their inputs come from the per-flavor pipelines, which are themselves now annotated; the dispatcher annotation in the parent run gives the same information.
- Per-flavor pipelines that have no jobs of their own (just a `uses:` to a reusable) — `run-name` is enough; there is no first-job to host a Print Inputs step without creating a dummy runner-consuming job.

---

## Applied changes

### 1. `meta-prerelease-stage.yml`

- Already had `run-name` surfacing `[debug-skip-tests]` and `[fail-fast]`.
- Added a `Print Inputs` step at the head of the existing `check` job — emits `event`, `schedule`, `debug-skip-tests`, `fail-fast` so the same information is also visible inside the run.

### 2. Per-flavor `prerelease-pipeline-*.yml` (6 files)

All six per-flavor pipelines (`prerelease-pipeline-{monolith,multitier}-{java,dotnet,typescript}.yml`) had no `run-name`. They are thin dispatchers (only a `verify` job that invokes `_prerelease-pipeline.yml` via `uses:`), so there is no jobsite for a Print Inputs step without creating a dummy runner-consuming job. They received a `run-name` only:

```yaml
run-name: >-
  prerelease-pipeline-<flavor>${{ inputs.level && inputs.level != 'qa' && format(' [level:{0}]', inputs.level) || '' }}${{ inputs.skip-local-stage && ' [skip-local]' || '' }}${{ inputs.skip-commit-stage && ' [skip-commit]' || '' }}${{ inputs.skip-acceptance-legacy && ' [skip-acceptance-legacy]' || '' }}${{ inputs.debug-skip-tests && ' [debug-skip-tests]' || '' }}${{ inputs.commit-sha && ' [pinned-sha]' || '' }}
```

`commit-sha` is rendered as a plain `[pinned-sha]` marker rather than its full value because GitHub Actions expressions have no string-slice helper and a 40-char SHA in the run title is noisy.

### 3. `meta-prerelease-dry-run.yml`

Already had a rich `run-name` covering `variant`, `skip-local`, `skip-commit`, `skip-acceptance-legacy`, `auto-trigger-stage`. No `check` job exists (top-level `run` is itself a `uses:` of the reusable pipeline), so no Print Inputs step was added — the existing run-name is sufficient.

### 4. Reusable / leaf workflows — intentionally untouched

`_meta-prerelease-pipeline.yml`, `_prerelease-pipeline.yml`, all `*-acceptance-stage*.yml`, `*-commit-stage.yml`, `*-qa-stage.yml`, `*-qa-signoff.yml`, `*-prod-stage.yml`. The implicit `with:` dump in the reusable workflow's "Set up job" section, plus the dispatcher annotation in the parent run, provide the same information without the maintenance overhead of a third copy.

---

## Verification

Manual: next time `meta-prerelease-stage` is dispatched manually with `debug-skip-tests=true`, the run title in the Actions UI should read `meta-prerelease-stage [debug-skip-tests]` and the `check` job's first step should print:

```
event=workflow_dispatch schedule=''
debug-skip-tests=true
fail-fast=false
```

Same shape for the per-flavor pipelines: dispatching `prerelease-pipeline-monolith-dotnet.yml` with `debug-skip-tests=true` and `commit-sha=<sha>` should produce the run title `prerelease-pipeline-monolith-dotnet [debug-skip-tests] [pinned-sha]`.

No CI cost — `run-name` is evaluated by GitHub before any runner is requested, and the single `Print Inputs` step adds <1s to `meta-prerelease-stage`.

---

## Out of scope

- Adding similar treatments to non-prerelease workflows (`meta-release-stage.yml`, `meta-bump-all.yml`, etc.) — apply the same pattern only when needed, on demand.
- Re-rendering `commit-sha` as a 7-char short SHA in the run-name — would require a tiny bash step in a setup job to compute and re-export the short form, which defeats the "cheap, no runner" property of `run-name`. Keep the boolean `[pinned-sha]` marker.
- Standardising the run-name suffix vocabulary across flavours of meta vs. flavour pipelines — the suffix tokens already differ (e.g. `[skip-local]` in dry-run vs. `[skip-local-stage]` ... actually unified to `[skip-local]` here) but cross-workflow naming consistency is not the goal of this plan.
