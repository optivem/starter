# Plan — gh-optivem `gh-acceptance-stage` Production-Stage Collision

**Date:** 2026-04-28
**Source run:** [optivem/gh-optivem run 25066324975](https://github.com/optivem/gh-optivem/actions/runs/25066324975) (workflow: `gh-acceptance-stage.yml`)
**Failing flavor:** `Run (multitier, monorepo, typescript, dotnet)` — job [73440156200](https://github.com/optivem/gh-optivem/actions/runs/25066324975/job/73440156200)
**Scope:** Diagnose the single failing flavor and propose a durable fix in `gh-optivem`. Other 9 annotations on this run are pre-existing C# Sonar warnings (`S4136`, `S1939`, `S2955`, `CS8604`, `CS8603`) tracked separately in `plans/20260427-082443-workflow-warnings-cleanup.md` — out of scope here.

---

## Symptom

`gh-acceptance-stage` job `Run (multitier, monorepo, typescript, dotnet)` failed at the `Acceptance Test` step. All other 16 flavors (Smoke + Run matrix) passed. The failure cascade:

1. Test scaffolds a fresh sandbox repo: `valentinajemuovic/test-app-a0dc3456-cb69c7b603fe351d`.
2. Phases 1–7 (commit / acceptance / QA stages on the sandbox) all pass.
3. Phase 8/9 — *Verify production stage* — triggers the sandbox's `prod-stage` workflow ([run 25069635242](https://github.com/valentinajemuovic/test-app-a0dc3456-cb69c7b603fe351d/actions/runs/25069635242)).
4. Sandbox `prod-stage` fails at the **`Verify System Release Not Yet Published`** check with:

   > Version `v1.0.56-rc.1-qa-deployed` is already released. Bump the VERSION file and create a new RC.

5. Outer Go test `TestValidMultitierConfigurations/multitier_monorepo_ts_react_dotnet` records the downstream failure → step exits 1.

Phases 1–7 succeeded for this flavor in the same run, so the scaffold itself works; only the prod-stage idempotency check tripped.

---

## Root cause

The sandbox-app `prod-stage` performs a uniqueness check that asks: *"has any image tagged `v<VERSION>-rc.1-qa-deployed` already been published to GHCR?"* If yes, it aborts with the "already released" error.

The `VERSION` baked into every scaffold inherits from the upstream shop template, currently `1.0.56`. The acceptance-stage matrix runs **17 flavors in parallel** (4 Smoke + 13 Run), each spawning its own ephemeral test-app under a fresh GUID-suffixed name. Multiple flavors that exercise the **same backend × frontend combination** (e.g. `typescript+dotnet`) end up pushing to the same per-combo GHCR image path on the **scaffolding user's** namespace (`ghcr.io/valentinajemuovic/...`), tagged with the same `v1.0.56-rc.1-qa-deployed`.

The first flavor to reach Phase 8 publishes the image. Subsequent flavors with a colliding (backend, frontend) tuple then hit the uniqueness gate and fail. In this run, the `multitier-monorepo-ts-dotnet` flavor was the loser — three other `*-dotnet` flavors (`monolith-multirepo-typescript-dotnet`, `monolith-monorepo-typescript-dotnet`, `multitier-multirepo-typescript-dotnet`) had already populated `v1.0.56-rc.1-qa-deployed` for shared image names earlier in the run.

This matches the pattern from the recent shop commit `80c42385` (*Bump VERSION 1.0.55 → 1.0.56 — manual: meta-prerelease blocked because monolith-system-{dotnet,typescript}:v1.0.55 GHCR images already published…*) — same class of bug, observed at the parent-repo prerelease layer instead of the meta-test layer.

**Two contributing factors:**
- **F1.** The GHCR image path used by the sandbox is not isolated per scaffold (does not include the test-app's GUID suffix), so parallel scaffolds from one upstream run share a tag namespace.
- **F2.** The `Verify System Release Not Yet Published` gate has no awareness of "this was published by a prior flavor in the same upstream run" vs. "this is a real duplicate release attempt."

F1 is the real bug; F2 is what amplifies it from "one nightly collision" to "every parallel matrix run."

---

## Proposed fix

Three options, in order of preference.

### Option A (preferred) — Namespace GHCR image paths by scaffold ID

Change the sandbox-app templates so the GHCR image name embeds the scaffold's unique GUID suffix (the one already in the repo name, `test-app-a0dc3456-cb69c7b603fe351d`). The image path becomes something like `ghcr.io/<owner>/<scaffold-id>/monolith-system-dotnet` instead of `ghcr.io/<owner>/monolith-system-dotnet`.

**Pros:**
- Eliminates F1 entirely; the uniqueness gate now compares per-scaffold, which is what it was implicitly assuming.
- Matches the existing per-scaffold isolation already used for the test-app repo name itself.
- No need to weaken or special-case the uniqueness gate — keeps protection against real duplicate-release bugs intact.

**Cons:**
- Requires changes in `gh-optivem`'s scaffold templates (workflow YAML + any `image:` references in compose files).
- Old leftover images under the un-namespaced path remain in GHCR until cleanup; they would not collide further but are noise.

**Risk:** Low. The scaffold tear-down in `gh-acceptance-stage` already deletes the test-app repo; extending the cleanup to also delete the per-scaffold GHCR namespace is a one-liner using `gh api -X DELETE /user/packages/...`.

### Option B — Make the uniqueness gate idempotent within an upstream run

Have the sandbox `prod-stage` `Verify System Release Not Yet Published` step accept the image as "ours" when its OCI annotations reference the current upstream run ID (passed in via `workflow_dispatch` inputs).

**Pros:**
- Smaller blast radius — only touches the verify step.
- Preserves a single shared image-path layout.

**Cons:**
- Conceptually wrong: the gate exists to prevent re-publishing a tag that already exists, and "already exists, but I put it there earlier" is exactly what it should *not* allow on a real release. Loosening it for the test path either drifts the test out of fidelity with prod, or weakens the prod gate itself.
- Requires plumbing the upstream run ID through three layers of workflow dispatch.

**Risk:** Medium. Easy to misconfigure such that the gate is effectively disabled in real prod.

### Option C — Serialize the matrix

Set `max-parallel: 1` on the `Run` matrix in `gh-acceptance-stage.yml`.

**Pros:**
- One-line fix.

**Cons:**
- Trades a 1-hour run for a ~9-hour run. The current run took 1h 6m end-to-end with parallel matrix; serialized would be ~9× that.
- Doesn't fix the underlying isolation bug — only hides it. Two consecutive runs against the same VERSION would still collide.

**Risk:** Low correctness, very high cycle-time cost. Not viable as a permanent solution; acceptable as a temporary workaround if Option A takes more than a day to land.

---

## Recommendation

Adopt **Option A**. Specifically:

1. In `gh-optivem`, locate the scaffold templates that emit the sandbox `prod-stage` workflow and the GHCR image push (likely under `internal/scaffold/...` or template files referenced by `TestValidMultitierConfigurations`).
2. Thread the scaffold's GUID suffix into the image name — both push side (in `prod-stage.yml`) and pull side (in any compose / k8s / helm references on the sandbox).
3. Extend the existing scaffold cleanup to also `gh api -X DELETE` the per-scaffold GHCR packages on success/failure. (Do this in the *outer* `gh-acceptance-stage` finalizer so the sandbox repo deletion and the GHCR cleanup happen together.)
4. Re-run [run 25066324975](https://github.com/optivem/gh-optivem/actions/runs/25066324975) to confirm green.

If Option A blocks on a non-trivial scaffold change, ship **Option C** as an interim and convert the matrix back to parallel once Option A lands.

---

## Verification

- Re-trigger `gh-acceptance-stage` on a fresh commit. Expected: all 17 flavors pass; no `Already released` from the sandbox `prod-stage`.
- Inspect GHCR (`https://github.com/users/<owner>/packages?repo_name=test-app-*`) — there should be one package set per active scaffold, all under namespaced paths, and the leftover entries from old un-namespaced runs should be either gone (after cleanup lands) or static.
- After two consecutive `gh-acceptance-stage` runs against the same shop `VERSION`, both should pass without manual VERSION bumps.

---

## Out of scope

- The 9 C# Sonar / nullable-reference warnings annotated on this run. Tracked in `plans/20260427-082443-workflow-warnings-cleanup.md` (W2, W3).
- The shop-side parallel symptom from commit `80c42385` (manual VERSION bump after meta-prerelease GHCR collision). Same class of bug at a different layer; consider a follow-up plan for the shop's `prerelease-pipeline-monolith-*` workflows once the gh-optivem fix is validated, since the same isolation principle applies.
