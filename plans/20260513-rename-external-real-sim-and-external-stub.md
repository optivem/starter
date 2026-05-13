# Rename `external-real-sim` → `simulators`, `external-stub` → `stubs`

**Status:** Steps 1 & 2 executed (gh-optivem + shop code/tests/docs). Step 3 (coordinated merge) pending user action.
**Created:** 2026-05-13.
**Cross-repo change** — touches both `optivem/shop` and `optivem/gh-optivem`.

---

## Step 3 — coordinated merge

1. Land Step 1 (`gh-optivem`) and Step 2 (`shop`) on the **same day**, in two PRs.
2. Bump `gh-optivem` patch version, cut release tag.
3. Run a rehearsal scaffold (`rehearsal-YYYYMMDD-HHMMSS`) from the updated shop using the new `gh-optivem` binary; bring up the scaffolded repo's docker stack; confirm `external-system-simulators` and `external-system-stubs` services come up and health probes pass.
4. Watch CI on shop's acceptance-stage flavors post-merge.

No backwards-compat shim in `copyExternals` (per CLAUDE.md anti-pattern guidance — would mask the silent-failure mode for the next mover).

---

## Out-of-plan scope found during execution

The 18 `.github/workflows/*-{acceptance,qa,prod}-stage-cloud.yml` files reference:

- Job names: `deploy-external-real`, `deploy-external-stub` (plus all the `needs:` references).
- Cloud Run service names: `external-real-acceptance`, `external-stub-acceptance`.
- GHCR image tags: `ghcr.io/.../external-real-sim:latest`, `ghcr.io/.../external-stub:latest`.

These were **not** updated by Step 2. The plan was silent on cloud workflows; renaming Cloud Run services creates new instances and orphans the old ones, and the GHCR image tags depend on whoever pushes them (build step not visible in the workflows themselves). Decide before Step 3 release tag:

- **Option A** — Leave cloud workflows alone (current state). Local docker stack uses new names; cloud deploys keep using old names. Inconsistent but non-breaking.
- **Option B** — Rename job names + image tags, but keep Cloud Run service names (to avoid orphaning). Find/update wherever GHCR images get built.
- **Option C** — Rename everything end-to-end, including Cloud Run services; budget for an orphan-services cleanup pass.

---

## Local validation performed

- gh-optivem: `bash scripts/test.sh --all ./...` — all packages pass.
- shop: `./compile-all.sh` — all 6 variants PASSED.
- shop: Java monolith `gh optivem run system` + `gh optivem test system --sample` — 11/11 sample suites PASSED (Smoke/Acceptance/Contract/E2E across real and stub labels).
- Containers come up under new names `my-shop-real-external-system-simulators-1` and `my-shop-stub-external-system-stubs-1` and pass health probes.

> Per CLAUDE.md, samples should be run for each of {java, dotnet, typescript} before commit; only Java was run during batch execution. Run dotnet + typescript samples before merging Step 2's commit, or rely on the post-commit acceptance-stage workflow.
