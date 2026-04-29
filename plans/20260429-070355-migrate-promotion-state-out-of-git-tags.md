# Plan — Migrate promotion / verification state out of git tags

**Date:** 2026-04-29
**Trigger:** Recurring class of bugs caused by storing mutable per-environment lifecycle state in git tag suffixes. Most recent instance: rc-tag glob collision — see [`20260428-210716-resolve-latest-tag-from-sha-status-tag-collision.md`](20260428-210716-resolve-latest-tag-from-sha-status-tag-collision.md).
**Scope:** All stage workflows (acceptance, qa, staging, prod) × all flavors (monolith/multitier × dotnet/java/typescript) × all consumers of status-suffix tag reads.
**Status:** Architecture plan. Stop-gap for the immediate rc-tag bug ships separately.

---

## Motivation

The shop pipeline encodes promotion / verification state as git tag suffixes:

- `<prefix>-v<version>-rc.<n>-acceptance-tested`
- `<prefix>-v<version>-rc.<n>-qa-deployed`
- `<prefix>-v<version>-rc.<n>-qa-approved`

This is a category error. **Git tags are designed for immutable artifact identity** (`rc.712`, `v1.0.56`); promotion state is per-environment lifecycle data that needs append-only history, queryable status, and a clear distinction between machine and human verdicts. GitHub provides three native APIs for exactly this — Deployments, Check Runs, and Environment protection rules — but the shop pipeline is not using them for this purpose.

Concrete consequences of state-in-tags:

1. **Glob collisions.** Bash globs cannot express "ends with `rc.<digits>` and no further suffix"; status-suffix tags silently widen `-rc.*` matches and break consumers (the rc-tag bug).
2. **Asymmetric tagging.** rc tags exist on both git and Docker (immutable identity, in lockstep); status-suffix tags exist only on git. Any tooling that assumes "tags-on-this-SHA = rc tags" breaks the moment a status suffix lands.
3. **No structured audit.** "What happened to rc.708?" is unanswerable without scraping tag names. No actor, no timestamp, no reason captured.
4. **Conflated concepts.** A deploy event, an automated test verdict, and a human signoff are three different categories of fact. Squashing them all into "string suffixes on a git tag" loses the distinction and pushes downstream tooling to re-derive it from the suffix vocabulary.

Moving state to native APIs fixes all four at once. Git tags revert to immutable identity, the glob-collision bug becomes structurally impossible, and audit becomes a first-class capability instead of a tag-name-archaeology exercise.

---

## Durable design

### Naming convention for state records

- **Format:** `<env>/<verb>`
- **Separator:** `/` — matches GitHub-ecosystem norms (`ci/build`, `codecov/project`, `deploy/staging`).
- **Order:** environment first — sorts the GitHub Checks UI in pipeline progression order; same env's records cluster together.
- **Verbs describe the gate or activity, not the outcome.** Use `tested`, not `passed`. Use `signoff`, not `approved`. The conclusion / state field carries the verdict; the name must not lie when the verdict is failure.

### Mechanism per kind of event

| Mechanism | What goes here | Why |
|---|---|---|
| **Deployment Record** (Deployments API) | Deploy events — "this SHA was pushed to env Y." `state` field carries success/failure of the **deploy itself** (did it come up). | Environment-indexed timeline. Native UI under *Environments* tab. "Currently deployed" semantics. Append-only history per environment. |
| **Check Run** (Check Runs API) | Verdicts on a SHA — "verification gate Y for env Z came back with conclusion C." `conclusion` field carries the verdict. | SHA-indexed, append-only, parallel-friendly, integrates with branch protection. Re-runs produce new records; history preserved. |
| **Environment Approval** (Environment protection rules) | Forward gates — "human approves advancement to env Y." Recorded against the next deployment. | Synchronous required-reviewer enforcement, paused-job UX, ref restrictions. Both approve and reject are recorded automatically. |

### Verb list

| Verb | Mechanism | Meaning |
|---|---|---|
| `deployed` | Deployment Record | Something physically reached this environment |
| `tested` | Check Run | Automated test gate ran (machine verdict) |
| `signoff` | Check Run | Human reviewed and gave a verdict (manual tester, security, product) |
| `approval` | Environment Approval | Human gate to advance to this environment (forward gate, recorded on the deployment) |

### Per-stage assignment

| Stage | Deployment Record | Check Run | Environment Approval |
|---|---|---|---|
| **acceptance** | `acceptance/deployed` (when docker-compose comes up) | `acceptance/tested` (automated) | — |
| **qa** | `qa/deployed` | `qa/signoff` (manual tester verdict) | — |
| **staging** | `staging/deployed` | `staging/signoff` *(if a separate verdict exists)* | `staging/approval` (required reviewer = QA tester, gates the staging deploy) |
| **prod** | `prod/deployed` | — | `prod/approval` (required reviewer, gates prod deploy) |

### Verdict-tracking policy: track both, or positives only?

Split by event kind, not blanket:

| Event kind | Track failures? | Reason |
|---|---|---|
| `*/tested` (automated Check Run) | **No — positives-only is fine.** | The failed workflow run itself is already the failure record (`gh run list --workflow=...`). A redundant failure Check Run adds noise without adding evidence. |
| `*/signoff` (human Check Run) | **Yes — track both.** | No other record exists. If a failure Check Run is not written when the tester rejects, the rejection vanishes entirely; the signoff workflow run only records "the signoff workflow ran," not "the human said no." |
| `*/deployed` (Deployment Record) | **Yes — automatic.** | Native: Deployment status timeline records `state=failure` when the deploy step fails. No extra code path. |
| `*/approval` (Environment Approval) | **Yes — automatic.** | GitHub records reject decisions on Environment approvals natively; not opt-in. |

### Replacement map (status tag → native record)

| Today (status-suffix git tag) | After migration |
|---|---|
| `<prefix>-v<base>-rc.<n>-acceptance-tested` | `acceptance/tested` Check Run on the rc's SHA + `acceptance/deployed` Deployment Record on the `acceptance` environment for that SHA |
| `<prefix>-v<base>-rc.<n>-qa-deployed` | `qa/deployed` Deployment Record on the `qa` environment for that SHA |
| `<prefix>-v<base>-rc.<n>-qa-approved` | `qa/signoff` Check Run on the rc's SHA *(and/or `staging/approval` Environment approval if the verdict is coupled to staging advancement)* |

After migration, git tags carry **immutable artifact identity only** — `<prefix>-v<base>-rc.<n>`, `<prefix>-v<base>`. No state. The `resolve-latest-tag-from-sha` glob-collision bug becomes structurally impossible: no SHA carries a status-suffixed tag, so no `-rc.*` glob can ever pick one up.

Floating Docker tags (`:qa`, `:staging`, `:prod`) optional as ergonomic pull handles; the Deployments API is the audit truth for "which rc is in which env."

---

## Sequencing

Multi-step rollout. Steps 1–4 below; step 0 (the immediate stop-gap) is in the sister bug-fix plan.

### Step 1 — Stop cutting new status tags

In each acceptance / qa / staging / prod stage workflow, replace `publish-tag` (or equivalent) calls that emit status-suffixed tags with the corresponding API call:

- Replace writing `*-acceptance-tested` git tag with:
  - `gh api .../deployments` → Deployment Record on `acceptance` for the SHA, with `state=success`
  - `gh api .../check-runs` → Check Run named `acceptance/tested`, `conclusion=success`, on the SHA, with workflow-run link in `output.summary`
- Replace writing `*-qa-deployed` git tag with:
  - `gh api .../deployments` → Deployment Record on `qa` for the SHA
  - Status timeline updates as the deploy progresses (`in_progress` → `success` / `failure`)
- Replace writing `*-qa-approved` git tag with:
  - `gh api .../check-runs` → Check Run named `qa/signoff` written by a `workflow_dispatch` workflow (manual tester triggers it with `verdict ∈ {pass, fail}`), with `conclusion=success` for pass and `conclusion=failure` for reject
  - Optionally: layer an Environment approval on the staging environment so the act of approving the staging deploy *is* the QA signoff (only if QA signoff is always coupled to a forward staging deploy)

After this step, no new status-suffix git tags are created. Existing ones remain readable until step 2 retires consumers.

**Per-flavor rollout:** start with one flavor (suggest `monolith-dotnet` since it's the one that surfaced the bug), validate end-to-end, then fan out across the other five flavors. Don't ship all six at once.

### Step 2 — Migrate consumers off status-tag reads

`grep` `optivem/shop/.github/workflows` for any workflow that reads `*-qa-deployed`, `*-qa-approved`, `*-acceptance-tested` tags (e.g. for "has this rc been QA-approved?" gates). Replace each with the equivalent API query:

- `gh api repos/{repo}/deployments?environment=qa&sha=$SHA` for "deployed?"
- `gh api repos/{repo}/commits/$SHA/check-runs` filtered by name (`acceptance/tested`, `qa/signoff`) for "tested?" / "signed-off?"
- Branch-protection / required-checks for "must be signed-off before staging?"

**Important ordering:** switch readers *before* writers in step 1 are removed for any given flavor — readers should gracefully fall back to the API while the producer side is still dual-writing or being migrated. Otherwise readers fail when the producer stops writing the tag.

For each replaced consumer: include a unit/contract test that the API query returns the expected record shape on a representative SHA.

### Step 3 — Audit & retire stop-gap

Once no consumer reads status-suffix tags and no producer writes them, the post-resolve regex filter from the bug-fix plan becomes redundant safety. Remove it (or leave as defense-in-depth — low cost either way).

### Step 4 — Optional historical cleanup

Bulk-delete existing `*-acceptance-tested` / `*-qa-deployed` / `*-qa-approved` git tags from the repo. Tidies the namespace. Coordinate so no in-flight migration step still depends on reading them. Skip if there's any audit/legal reason to retain historical tags.

---

## Rejected alternatives

For trail-of-reasoning. All considered before landing on the design above.

- **Add `pattern-format: glob|regex` to `resolve-latest-tag-from-sha`.** Fixes the glob weakness for every caller; clean API change. Rejected as the *durable* answer because it treats the symptom (glob can't anchor) not the disease (state-in-tags). Optionally adoptable as a quality-of-life upgrade to the shared action — but not load-bearing once status tags stop being written.

- **Add `exclude-pattern` input to `resolve-latest-tag-from-sha`.** Denylist; couples this caller to the suffix vocabulary owned by `compose-prerelease-status`. Drift-prone — every new promotion stage requires updating the deny pattern.

- **Keep status tags but mirror them onto Docker images** (symmetric tagging on both sides). Doubles the failure surface — now both git and the registry can drift, and the glob-collision exists in both namespaces. Strictly worse than either side alone.

- **Floating Docker tags only** (`:qa`, `:staging`) as the *only* state record. Loses history — re-pointing the float erases the previous mapping. Fine as a *companion* read-side handle next to the Deployments API; not as the source of truth.

- **Append-only JSON log committed to the repo** (`.promotions/log.jsonl`). Workable but reinvents the Deployments API by hand: needs CI write access, race-prone under concurrent promotions, awkward with PR vs. direct-push.

- **OCI attestations (cosign / SLSA / GitHub Artifact Attestations)** for signoffs and approvals. Strongest audit answer; required for SLSA L3+ and some compliance regimes. Out of scope for this migration — adoptable later as a layer on top of the design above without rework.

- **All Check Runs as a universal append-only ledger** (skip Deployments and Environment approvals). Coherent simpler model; some teams do this. Rejected because it loses the native *Environments* tab UI, native required-reviewer enforcement, and "currently deployed" semantics. The cost of using each API for what it's good at (some heterogeneity) is less than the cost of rolling those affordances by hand.

---

## Out of scope

- Cryptographic attestations (cosign / SLSA). Adoptable later without rework.
- Cleanup of historical status-suffix tags already in the repo. Optional step 4 above.
- Whether to also add regex support to `resolve-latest-tag-from-sha`. Independent quality-of-life improvement; not required once this migration completes.
- Unifying `meta-prerelease-stage` re-targeting policy (whether already-promoted SHAs should be re-verifiable). Independent question.

---

## Risk assessment

| Action | Risk | Notes |
|---|---|---|
| **Step 1 — stop cutting new status tags** | Low–medium. | Per-stage workflow change; needs test coverage for each (acceptance, qa, staging, prod) × (monolith, multitier) × (dotnet, java, typescript) = up to 24 cells. Stage-by-stage and flavor-by-flavor rollout possible. Risk: a stage workflow stops writing the tag before its consumers stop reading it — see step 2 ordering. |
| **Step 2 — migrate consumers** | Medium. | Highest-risk step because every reader has to be found and updated. Mitigation: grep audit before starting; switch readers *before* tag-writers in step 1 are removed; keep the bug-fix stop-gap as a backstop while in flight; add per-consumer contract tests. |
| **Step 3 — retire stop-gap** | Trivial. | Defense-in-depth removal. Reversible. |
| **Step 4 — optional historical cleanup** | Low. | Tag deletion is reversible from local refs while branch is fresh; once gone from origin and locals, recreating means re-tagging from the original SHA. Skip if any doubt about retention requirements. |
| **Doing nothing** | High. | State-in-tags continues to grow new failure modes as new promotion stages are added. The rc-tag glob bug recurs every time `meta-prerelease-stage` is re-dispatched against an already-promoted SHA. Audit gap remains; "what happened to rc.708?" stays unanswerable. |
