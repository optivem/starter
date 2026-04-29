# GitHub Actions Hardening Plan

Remediation plan for findings from the 2026-04-28 Actions audit of the 71 workflows under `.github/workflows/`.

Items are ordered by severity. Each item is independently shippable — no cross-item dependencies unless noted.

---

## High

### H0. Stop using git tags as the promotion-state machine

**Problem.** The pipeline encodes deployment/approval state by appending suffixes onto RC tags:

- `<flavor>-v<version>-rc.<n>` — RC artifact created by prerelease stage
- `<flavor>-v<version>-rc.<n>-qa-deployed` — appended when deployed to QA
- `<flavor>-v<version>-rc.<n>-qa-approved` — appended when QA signs off
- `meta-release-stage` then runs `git tag --list "<flavor>-v<VERSION>-rc.*-qa-approved"` to decide what to promote.

Evidence:
- `monolith-dotnet-qa-signoff.yml:38-44` — resolves "latest RC deployed to QA" by listing `*-qa-deployed` tags
- `monolith-dotnet-qa-signoff.yml:67-73,86-90` — composes a `-qa-approved` status tag and pushes it
- `meta-release-stage.yml:154,162` — promotion decision driven by `git tag --list "${flavor}-v${VERSION}-rc.*-qa-approved"`
- `meta-prerelease-stage.yml:10` — input description explicitly states the workflow depends on those approval tags pre-existing for recovery scenarios
- `cleanup.yml:41` — exists in part to garbage-collect the resulting tag pollution

**Why this is bad.**
1. **Git is not a state store.** Tags are immutable refs, not a transactional state machine. Two concurrent runs can both create approval tags; nothing rejects a `-qa-approved` tag without a prior `-qa-deployed` tag. Order/causality is convention, not enforcement.
2. **No audit trail.** A tag's existence tells you the state but not who set it, when, or why. No structured metadata, no rollback record, no history of transitions.
3. **Non-atomic across jobs.** Tag-then-release in `meta-release-stage.yml:269-272` already needs custom retry handling because the two-phase commit is brittle. The `meta-prerelease-stage.yml` recovery input exists for the same reason.
4. **Tag namespace explosion.** Every RC × every environment × every status creates new tags. `cleanup.yml` is a symptom of this — the pipeline generates garbage that must be swept.
5. **Authorization is implicit.** Anyone with tag-push rights can fake an approval. Tags should be artifact markers, not authorization tokens.
6. **Hard to query.** "What's currently approved in QA?" requires shelling out `git tag --list` and parsing strings. Cannot be answered from the GitHub UI, dashboards, or external tools without bespoke scripting.
7. **Source control bloat.** Tag count grows unbounded; `git fetch --tags` and clone times degrade.

**Fix — target architecture.**

Use the **GitHub Deployments API** as the promotion-state store. It is purpose-built for exactly this: native UI under repo → Deployments, native ACL via environment protection rules, queryable via `gh api`, atomic state transitions, structured payload for metadata.

Mapping:

| Today | Target |
|---|---|
| `-rc.<n>` tag | Keep — it's an artifact marker, that's fine |
| `-qa-deployed` tag | `POST /repos/.../deployments` with `environment: qa-<flavor>`, ref = RC tag |
| `-qa-approved` tag | `POST /repos/.../deployments/<id>/statuses` `{state: success}` after manual approval gate |
| `-qa-rejected` tag | Same endpoint with `{state: failure}` |
| `git tag --list "*-qa-approved"` query | `gh api ...deployments?environment=qa-<flavor>&...` filtered by status |

GitHub **environment protection rules** then provide the manual approval gate natively — no custom signoff workflow needed. Promotion is gated by required reviewers configured per environment.

**Migration path (incremental, no big-bang).**

1. **Add deployments alongside tags.** In `*-acceptance-stage*.yml` and `*-qa-signoff.yml`, additionally call `gh api` to create deployments + statuses. Keep the tag writes for now. Verify deployments appear correctly in the GitHub UI.
2. **Switch the read side.** Change `meta-release-stage.yml` to query deployments instead of `git tag --list`. Tags still exist but become advisory.
3. **Add environment protection rules.** Configure required-reviewers on `qa-<flavor>` and `prod-<flavor>` environments. The `qa-signoff` workflows become deployment-gating jobs (`environment: qa-<flavor>`) instead of separate workflows.
4. **Remove tag writes.** Delete the `-qa-deployed` / `-qa-approved` tag-creation steps once the deployment-API path has been running cleanly for ~2 release cycles.
5. **Retire the cleanup of suffix tags** in `cleanup.yml` (keep RC-tag cleanup).

**Effort.** Significant — ~1–2 weeks of careful work. Not urgent in the sense that the current setup *works*, but it is the highest-leverage architectural improvement in this audit.

**Risk.** Medium during migration (parallel-write phase has to stay consistent), low after. The Deployments API is stable and GitHub-native.

**Out of scope alternative.** If the Deployments API is rejected for any reason, the second-best option is a state branch (`refs/heads/state/promotions`) holding a JSON file written via atomic commit. Still better than tag suffixes — provides history, audit, and atomicity — but lacks UI integration. Do not consider this preferable to deployments.

---

### H0b. Replace `WORKFLOW_TOKEN` PAT with a GitHub App

**Problem.** A long-lived secret named `WORKFLOW_TOKEN` is used in **42 workflows** wherever the workflow needs to push a tag or commit that should *trigger* downstream workflows. `GITHUB_TOKEN`-authored refs deliberately do not trigger downstream workflows (loop prevention), so the team has worked around this with a PAT.

Evidence: `monolith-dotnet-qa-signoff.yml:34-36, 90`; identical pattern in 41 other files.

**Why this is bad.**
1. **Long-lived credential.** PAT lifetimes are typically months/years; rotation is manual and easy to miss. A leaked PAT = full repo write for its scopes.
2. **Tied to a human user.** PATs are owned by a specific GitHub account. If that user leaves the org, every workflow breaks. If their permissions change, workflows silently start failing.
3. **No fine-grained scoping per workflow.** All 42 workflows share the same token with the same scopes — least-privilege is impossible.
4. **No audit attribution.** Actions taken by a PAT show up under the human's name; you can't tell from audit log whether `alice` pushed a tag manually or it was workflow X at 03:00 UTC.

**Fix.** Create a GitHub App scoped to this repo with only the permissions actually needed (contents: write, actions: write). Use `actions/create-github-app-token@v2` (or equivalent) to mint a short-lived installation token at the start of each job. Apps appear in audit logs as themselves and tokens auto-expire in ~1 hour.

**Effort.** ~half a day to set up the App + a sweep to replace `secrets.WORKFLOW_TOKEN` with the App-token step output.

**Verification.** Roll out to one workflow first, confirm downstream triggers still fire, then sweep.

---

### H0c. Cross-workflow orchestration via dispatch-and-poll

**Problem.** `meta-release-stage` and `_meta-prerelease-pipeline` orchestrate the 6 flavor pipelines via `optivem/actions/trigger-and-wait-for-workflow@v1` — 14 invocations across the meta and pipeline workflows. Each call dispatches a separate `workflow_dispatch` and polls the runs API for completion (timeout 3600s).

Evidence: `meta-release-stage.yml:183,194,205,216,227,238`; `_meta-prerelease-pipeline.yml:218,258,279`; `_prerelease-pipeline.yml:204,216,247,259,270`.

**Why this is bad.**
1. **No native dependency graph.** Each polled workflow is a *separate* run on the Actions UI. Debugging means clicking through 7+ runs (parent + 6 children) and correlating timestamps.
2. **Polling is fragile.** API rate limits, transient 5xx, and the 3600s timeout each create distinct failure modes. The custom `gh-retry.sh` exists partly to compensate.
3. **Status propagation is convention, not enforced.** Parent workflow has to interpret child outputs; no built-in fan-out/fan-in.
4. **The `tag-meta-release` job has to use `if: !failure() && !cancelled()`** (`meta-release-stage.yml:249`) precisely because the dispatched children's status doesn't propagate cleanly through `needs:`.

**Fix.** Convert each dispatched workflow to a **reusable workflow** (`on: workflow_call`) and call it via `uses: ./.github/workflows/<flavor>-prod-stage.yml` from the meta-release-stage. GitHub Actions then renders one unified run graph with native `needs:` dependencies, native status propagation, and one timeline. No polling, no PAT (`secrets: inherit` works for reusable workflows), no compensating retry logic.

**Caveat.** Reusable workflows can't be dispatched directly from the UI for ad-hoc runs. Solution: keep a thin `workflow_dispatch` shim that calls the reusable. The shim is one job, no logic.

**Effort.** ~3–5 days. Touches 6 prod-stage workflows + the meta-release. Highest payoff is operability — release-engineering debug time should drop noticeably.

**Verification.** Run the meta-release-stage end-to-end on a non-prod tag; verify the run graph shows all 6 flavor pipelines as nested jobs.

---

### H0d. VERSION-files-in-repo + bump commits as deployment ledger

**Problem.** `bump-patch-version.yml` writes a commit to `main` after each release that bumps `VERSION`, `system/multitier/backend-dotnet/VERSION`, etc. The recent `git log` is dominated by `Bump system/multitier/frontend-react/VERSION 1.3.41 -> 1.3.42` style entries.

**Why this is bad.**
1. **Repo history pollution.** A semantic `git log` of "what changed" is drowned in mechanical bumps. `git blame` on functional code becomes harder.
2. **Ledger lives in the working directory.** Anyone editing a VERSION file by hand can desync with reality. The `signal: ghcr-image` lookup is a workaround — the *real* version lives in the registry, but the file is treated as authoritative.
3. **Each bump is a `main` commit** with `contents: write` permission needed by an automated actor — a permission you'd otherwise prefer not to grant.
4. **Race-prone.** `bump-patch-version.yml` already uses the Contents API with SHA preconditions (good — that detail wouldn't be necessary if VERSION wasn't a shared mutable file).

**Fix — two options:**

- **A. Drop VERSION files entirely; derive version from registry tags or git tags.** The `meta-v<version>` git tag is already authoritative for releases. CI computes "next patch" from the latest matching tag at build time. No commits to `main`, no VERSION files.
- **B. Keep VERSION files but stop committing bumps.** Treat them as the *floor* (manually-bumped on minor/major) and let CI compute the patch suffix from registry/tags. Auto-bump goes away.

A is cleaner; B is a smaller migration.

**Effort.** Option A: ~1 week (touches every build that reads a VERSION file). Option B: ~2 days.

**Note.** This one is lower priority than H0/H0b/H0c because it's about repo hygiene rather than reliability or security. Listed here because it's the same family of anti-pattern: encoding state as git side-effects.

---

### H1. Pin third-party actions to commit SHA

**Problem.** Third-party actions are pinned to floating major tags. A compromised tag re-point would execute attacker code with our `GITHUB_TOKEN` and secrets.

**Affected actions** (run `grep -rE "uses: (Wandalen|nick-fields|SonarSource|softprops)/" .github/workflows/` for the full list):

| Action | Current | Files |
|---|---|---|
| `Wandalen/wretry.action` | `@v3` | ~100 occurrences across `*-acceptance-stage*.yml`, `*-commit-stage.yml` |
| `nick-fields/retry` | `@v4` | `monolith-java-acceptance-stage-cloud.yml:284,313,342` |
| `SonarSource/sonarqube-scan-action` | `@v7` | `monolith-typescript-commit-stage.yml:115` |
| `softprops/action-gh-release` | `@v3` | `meta-release-stage.yml` |

**Fix.** Replace each with the full commit SHA + a comment recording the version:

```yaml
- uses: Wandalen/wretry.action@<40-char-sha>  # v3.8.0
```

**Notes.**
- Keep `actions/*` (first-party) on major tags — that is the GitHub-recommended exception.
- `Wandalen/wretry` dynamically loads other actions via its `action:` input (e.g. `docker/login-action@v4`). SHA-pinning Wandalen does not pin its callees. Either (a) replace Wandalen with our own retry around explicitly-pinned actions, or (b) accept the residual risk and document it.

**Effort.** ~2 hours, mostly mechanical sed-replace + verification run.

**Verification.** Run one acceptance workflow per language post-change.

---

### H2. Add job-level `timeout-minutes`

**Problem.** No workflow sets `timeout-minutes` at job level. Default is 360 min — a hung test run will burn 6 hours of runner time before GitHub kills it.

**Affected.** All 71 workflows — current step-level `timeout-minutes` (64 occurrences) are only on Playwright install steps.

**Fix.** Add a `timeout-minutes:` to every `jobs.<id>:` block, sized per stage:

| Stage | Suggested `timeout-minutes` |
|---|---|
| Commit (build + unit) | 20 |
| Acceptance (in-cluster) | 45 |
| Acceptance cloud (GCP) | 60 |
| QA / prod stages | 30 |
| Release / meta-pipeline | 90 |
| Cleanup, version-bump | 10 |

**Effort.** ~3 hours — touches every workflow but each edit is one line.

**Verification.** Trigger one of each stage type; confirm it still finishes well inside the new limit.

---

### H3. Reduce hourly acceptance-cron fan-out

**Problem.** ~12 acceptance workflows fire `0 * * * *`. 288 runs/day, all on the hour. Most are no-ops behind a timestamp gate but still consume queue/runner minutes and concentrate load.

**Affected.**
- `monolith-{dotnet,java,typescript}-acceptance-stage.yml:5`
- `monolith-{dotnet,java,typescript}-acceptance-stage-legacy.yml:5`
- `multitier-{dotnet,java,typescript}-acceptance-stage.yml:5`
- `multitier-{dotnet,java,typescript}-acceptance-stage-legacy.yml:5`

**Fix — pick one:**

- **Option A (preferred).** Single dispatcher workflow that polls the timestamp gate once per hour and dispatches only the workflows that need to run via `workflow_dispatch`. Reduces baseline cost to one runner-minute/hour.
- **Option B (low effort).** Stagger crons across the hour (`0 * * * *`, `5 * * * *`, …, `55 * * * *`) so they don't queue simultaneously. Doesn't reduce total runs, only smooths load.

**Effort.** Option A: ~1 day. Option B: ~30 min.

**Recommendation.** Option B now (quick win), Option A as a follow-up if cron-runner cost becomes a concern.

---

## Medium

### M1. Add Docker GHA layer caching

**Problem.** `docker/build-push-action@v7` invocations have `provenance: mode=max` and `sbom: true` (good) but no `cache-from`/`cache-to`. Every build re-pulls and re-builds layers from scratch.

**Affected.** All `*-commit-stage.yml` files that publish images (~7 jobs):
- `monolith-{dotnet,java,typescript}-commit-stage.yml`
- `multitier-backend-{dotnet,java,typescript}-commit-stage.yml`
- `multitier-frontend-react-commit-stage.yml`

**Fix.** Add to each `docker/build-push-action` step:

```yaml
cache-from: type=gha
cache-to: type=gha,mode=max
```

**Effort.** ~30 min.

**Verification.** Compare commit-stage duration before/after on a no-op rebuild — expect 30–60% reduction.

---

### M2. Add `concurrency:` with `cancel-in-progress` to commit stages

**Problem.** Commit-stage workflows have no `concurrency:` block. Rapid PR pushes queue up multiple full builds for commits that will be superseded.

**Affected.**
- `monolith-{dotnet,java,typescript}-commit-stage.yml`
- `multitier-backend-{dotnet,java,typescript}-commit-stage.yml`
- `multitier-frontend-react-commit-stage.yml`

**Fix.** Add at workflow top level:

```yaml
concurrency:
  group: ${{ github.workflow }}-${{ github.ref }}
  cancel-in-progress: true
```

**Effort.** ~15 min.

**Note.** Do NOT apply `cancel-in-progress: true` to release/meta-prerelease pipelines — those must complete. Audit confirms they already correctly use `cancel-in-progress: false`.

---

### M3. Acceptance workflows have `concurrency.group` but no `cancel-in-progress`

**Problem.** `monolith-dotnet-acceptance-stage.yml:13` (and siblings) set a group but no cancel policy. Hourly cron + `workflow_dispatch` can overlap.

**Fix.** Add `cancel-in-progress: true` to acceptance workflows. Decide explicitly per workflow whether overlap matters — for cron-triggered nightly-style runs, cancel-on-overlap is usually correct.

**Affected.** All `*-acceptance-stage*.yml` (12 workflows).

**Effort.** ~20 min.

---

### M4. Audit unnecessary `fetch-depth: 0`

**Problem.** Full-history clone used 55 times. Slow on large repos and rarely needed.

**Fix.** For each occurrence, decide:
- Genuinely needs tags / `git describe` / changelog generation → keep
- Doesn't → remove (default fetch-depth: 1 is faster)
- Summary jobs (`monolith-*-qa-signoff.yml:99-101`) re-checkout but probably don't need any source — remove the checkout entirely.

**Effort.** ~2 hours of careful review.

---

### M5. Standardise `actions/setup-node` version

**Problem.** `v6` in commit-stage workflows, `v5` in `cross-lang-system-verification.yml:135`.

**Fix.** Bump to `v6` everywhere.

**Effort.** ~5 min.

---

## Low

### L1. Drop redundant token in `bump-patch-version.yml`
`bump-patch-version.yml:61-63` passes `secrets.GITHUB_TOKEN` both as `with.token` and `env.GITHUB_TOKEN`. Keep one.

### L2. Pin `runs-on: ubuntu-latest` → `ubuntu-24.04`
Optional. Improves reproducibility; downside is manual bumps when GitHub deprecates the image. Skip unless we hit a reproducibility issue.

### L3. SHA-pin `actions/*` first-party actions (defence in depth)
Industry trend. Lower priority because GitHub has stronger controls on first-party actions. Consider a future sweep.

---

## Out of scope

- Migrating to GitHub-hosted larger runners — separate cost/perf decision.
- Adding required-status-checks branch protection — repo-settings change, not a workflow change.
- Replacing `Wandalen/wretry` entirely with `scripts/gh-retry.sh`-style native retry — possible follow-up after H1 if the residual transitive-pinning risk is unacceptable.

---

## Suggested execution order

**Quick wins (1 sprint):**
1. **H2** (timeout-minutes) — broadest safety win, lowest risk.
2. **M2** + **M3** (concurrency) — immediate runner-cost reduction.
3. **M1** (Docker cache) — measurable speedup on every commit.
4. **H1** (SHA-pin third-party) — security win, more careful work.
5. **H3 Option B** (stagger crons) — quick.
6. **M5** (setup-node alignment), **L1** (token cleanup), **M4** (fetch-depth audit) — housekeeping.

**Architectural (multi-sprint):**

7. **H0b** (GitHub App for `WORKFLOW_TOKEN`) — security hygiene, well-bounded, ~half a day.
8. **H0c** (reusable workflows instead of dispatch-and-poll) — biggest operability win; design first, then incremental conversion.
9. **H0** (replace tag-based promotion state with Deployments API) — highest-leverage architectural change. Sequence after H0c so the new run graph is the obvious place to attach deployment records.
10. **H0d** (drop or freeze VERSION-file bump commits) — lowest urgency of the architectural set; do last.
11. **H3 Option A** (cron dispatcher) — if cron cost still matters after the above.

Each step is its own PR. After each, run `gh optivem test system --sample` per CLAUDE.md before merging.

Note on ordering: **H0 is high-severity but deliberately sequenced after the quick wins.** It is the most valuable change but also the most invasive. Do the cheap, isolated fixes first to bank reliability/security wins while H0 is being designed and rolled out incrementally.
