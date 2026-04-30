# 20260430-0630 — Workflow Best-Practices Audit

**Date:** 2026-04-30
**Surface A:** 83 shop workflows under `.github/workflows/`
**Surface B:** 42 composite actions under `../actions/` (sibling checkout)
**Tooling:** actionlint not installed — schema lint skipped (see "Tooling" note below)

## Out of scope

- Runtime warnings (deprecation messages, set-output, etc.) → see `workflow-warnings-auditor`
- Cross-language consistency between dotnet/java/typescript siblings → see `workflow-comparator`
- Action behavior / functional bugs (only structure & configuration audited here)

## Tooling

`actionlint` was not on PATH in this audit environment. Recommend installing it (`go install github.com/rhysd/actionlint/cmd/actionlint@latest` or download release binary) and re-running — it catches schema and expression issues this static audit cannot, and the shop workflow set is large enough (83 files, 298 jobs) that schema-level lint coverage is worth the install. No L1 section is included because the tool did not run.

---

## B1 — Action pinning

**Symptom**

```yaml
# .github/workflows/_prerelease-pipeline.yml
- uses: docker/build-push-action@v7        # third-party, version tag, no SHA
- uses: optivem/actions/bump-patch-versions@v1
- uses: SonarSource/sonarqube-scan-action@v7
- uses: nick-fields/retry@v4
```

**Affected references (50 unique third-party `uses:` references, ALL pinned to a moving `@vN` tag, ZERO pinned to a 40-char SHA):**

Distinct refs needing SHA pin (from `/tmp/shop-bp/B1-pinning.txt`, all classified `TAG_NO_SHA`):

- 7 external publishers — `docker/build-push-action@v7`, `docker/metadata-action@v6`, `docker/setup-buildx-action@v4`, `google-github-actions/auth@v3`, `google-github-actions/deploy-cloudrun@v3`, `google-github-actions/setup-gcloud@v3`, `gradle/actions/setup-gradle@v6`, `nick-fields/retry@v4`, `softprops/action-gh-release@v3`, `SonarSource/sonarqube-scan-action@v7`, `Wandalen/wretry.action@v3`
- 39 distinct `optivem/actions/*@v1` composite refs across 83 shop workflows (`../actions/` is owned by the same org, but it is still a separate repository — supply-chain considerations apply)

First-party tags (allowed by spec): `actions/checkout@v6`, `actions/setup-{node,java,dotnet}@v5`, `actions/cache@v5`, `actions/setup-node@v6`. No partial SHAs and no `@main`/`@master` floating-branch references found.

**Why this matters:** A moving major tag (`@v3`, `@v6`, `@v1`) can be force-pushed by the publisher; whoever controls the tag controls what runs in CI. A 40-char SHA pin is immutable. For the `optivem/actions` repo specifically, every shop workflow trusts whatever the `v1` tag currently points to — a single accidental or compromised retag affects all 83 workflows in lockstep.

**Proposed fix:**

1. For each external third-party action, replace `@vN` with the current commit SHA followed by a trailing comment:
   ```yaml
   - uses: docker/build-push-action@2634353e5390cf3a09ade9c52b1c66427b1f5d5d  # v6.13.0
   ```
2. For `optivem/actions/<name>@v1` references, decide between two consistent strategies (pick one for the whole repo — do not mix):
   - **Strategy A (SHA pin):** replace each `@v1` with the current commit SHA touching that composite path. The composite SHAs as of this audit (from `git -C ../actions log -1 --format=%h -- <name>/`):
     - `bulk-update-project-item-status` → `13b3714`
     - `bump-patch-versions` → `941dd89`
     - `check-changes-since-tag` → `8a55d26`
     - `commit-files` → `8a55d26`
     - `compose-tags` → `137ee24`
     - `create-deployment` → `e354f3f`
     - `deploy-docker-compose` → `16b1b9a`
     - `evaluate-run-gate` → `3c05113`
     - `publish-tag` → `efc264b`
     - `tag-docker-images` → `137ee24`
     - `trigger-and-wait-for-workflow` → `8649f90`
     - `wait-for-endpoints` → `09f5cd5`

     (The remaining ~30 composite paths used by shop need the same lookup before applying.)
   - **Strategy B (keep `@v1`, accept the trade):** document explicitly in this repo's README that `optivem/actions` is treated as a trusted internal monorepo and `v1` is the supported moving major. This is a defensible pedagogical choice; flag in CLAUDE.md that the shop course material accepts this.
3. Use a tool like `pinact`, `ratchet`, or Dependabot's auto-updater to keep SHA pins up to date if Strategy A is chosen.

**Risk:** **Medium.** Supply-chain exposure for external actions; lower for internal `optivem/actions` but still real. No imminent deadline (no `upload-artifact@v3` deprecation found), so this is "harden when convenient" rather than "drop everything."

**Surface-B note:** `../actions/render-system-stage-summary/action.yml` itself uses `optivem/actions/format-artifact-list@v1` and `optivem/actions/render-stage-summary@v1` — composite-to-composite pin discipline (C4) reflects the same gap. Composite-action changes affect every consumer beyond shop; coordinate with the `optivem/actions` repo owner.

---

## B2 — Action version freshness

**Symptom:** None of the action references in shop are >2 majors behind the spec's known-latest table, and no deprecated `actions/upload-artifact@v3` / `actions/download-artifact@v3` usages exist anywhere in `.github/workflows/`.

A few references are AHEAD of the spec table — `actions/checkout@v6`, `actions/setup-node@v6`, `actions/cache@v5`, `actions/setup-dotnet@v5`, `actions/setup-java@v5`. The spec lists v5 / v4 as latest. This is fine — being ahead of the conservative table is not a finding. (If `setup-node@v6` does not exist yet in the public registry, that is a different kind of bug — outside the scope of B2 freshness.)

**No findings.** Zero entries written to `/tmp/shop-bp/B2-freshness.txt` after the deprecation grep. Skipping this section in the priority table.

---

## B4 — Least-privilege `permissions:`

**Symptom**

```yaml
# .github/workflows/monolith-dotnet-commit-stage.yml — top-level
name: Monolith Dotnet Commit Stage
on:
  push:
    paths: [...]
# (no top-level permissions: block — falls back to repo default,
#  which is contents: write for repos with default settings)
jobs:
  build: ...
```

**Affected files (81 of 83 workflows, only `meta-bump-all.yml:22` and `move-tickets-to-qa.yml:26` declare a top-level `permissions:` block):**

See `/tmp/shop-bp/B4-permissions.txt` for the full list. The 81-file gap covers every `*-stage.yml`, every `*-pipeline*.yml`, every `bump-patch-version-*.yml`, `cleanup.yml`, `cross-lang-system-verification.yml`, and the meta-* signoff workflows.

`cleanup.yml` does declare a job-level `permissions:` at line 30, so its actual blast radius is constrained even without a top-level block. Other workflows mix in job-level `permissions:` too, but the absence of a top-level default-deny is uniform.

No `permissions: write-all` and no `permissions: read-all` were found anywhere — so the gap is "absent block," not "broad block."

**Why this matters:** Without a top-level `permissions:`, jobs inherit the repo-level default. For repos created in the last few years that default is restricted, but it is settable per-org and per-repo and can drift. A top-level `permissions: {}` followed by per-job grants makes the contract explicit and survives org-policy changes.

**Proposed fix:** Add a top-level `permissions:` block to every workflow. For pipelines that only read code:
```yaml
permissions:
  contents: read
```
For workflows that push commits / tags / releases (the bump-patch-version-* family and the meta-release-stage):
```yaml
permissions: {}
jobs:
  bump:
    permissions:
      contents: write
      pull-requests: write
```
The default-deny + per-job grant pattern lets each job advertise exactly what it needs.

**Risk:** **Low/Medium.** Cosmetic when org defaults are already locked down; medium when they are not. No active breakage.

**Pedagogical note:** This is exactly the kind of "deliberately simple" omission that may belong in early lesson material. Bulk-applying `permissions:` to every workflow would erase a possible teaching moment — review with the course author before mass-edit. See "Cross-cutting risk" below.

---

## B5 — Concurrency groups

**Symptom**

```yaml
# .github/workflows/_prerelease-pipeline.yml — pipeline driver, no concurrency
on:
  workflow_call:
    inputs: ...
# (no concurrency: block)
jobs: ...
```

**Affected files (15 unique stage/pipeline/meta workflows missing a top-level `concurrency:` block):**

- `.github/workflows/_meta-prerelease-pipeline.yml`
- `.github/workflows/_prerelease-pipeline.yml`
- `.github/workflows/monolith-dotnet-commit-stage.yml`
- `.github/workflows/monolith-java-commit-stage.yml`
- `.github/workflows/monolith-typescript-commit-stage.yml`
- `.github/workflows/multitier-backend-dotnet-commit-stage.yml`
- `.github/workflows/multitier-backend-java-commit-stage.yml`
- `.github/workflows/multitier-backend-typescript-commit-stage.yml`
- `.github/workflows/multitier-frontend-react-commit-stage.yml`
- `.github/workflows/prerelease-pipeline-monolith-dotnet.yml`
- `.github/workflows/prerelease-pipeline-monolith-java.yml`
- `.github/workflows/prerelease-pipeline-monolith-typescript.yml`
- `.github/workflows/prerelease-pipeline-multitier-dotnet.yml`
- `.github/workflows/prerelease-pipeline-multitier-java.yml`
- `.github/workflows/prerelease-pipeline-multitier-typescript.yml`

**Why this matters:** Commit-stage and pipeline workflows hold external resources (Docker registries, GHCR, Sonar, deployments). Concurrent runs from rapid pushes can race to publish images with the same tag, double-deploy to the same environment, or trip Sonar's analysis-in-progress lock. A `concurrency:` group with `cancel-in-progress` (or `cancel-in-progress: false` if work-in-progress should serialize, not cancel) prevents this.

**Proposed fix:** Add to each affected workflow:
```yaml
concurrency:
  group: ${{ github.workflow }}-${{ github.ref }}
  cancel-in-progress: false
```
For commit-stage workflows on `main`, `cancel-in-progress: true` is also reasonable (only the latest push matters). For pipelines that publish artifacts, `cancel-in-progress: false` is safer.

**Risk:** **Medium.** Real reliability win; the tag-collision and double-deploy classes of bug are exactly what concurrency exists to prevent.

---

## B6 — `timeout-minutes` on jobs

**Symptom**

```yaml
# .github/workflows/monolith-dotnet-commit-stage.yml
jobs:
  build:
    runs-on: ubuntu-latest
    # no timeout-minutes — falls back to the 360-minute (6h) default
    steps: ...
```

**Affected files (76 of 83 workflows have at least one job missing `timeout-minutes:`; 298 individual jobs across the repo):**

Top offenders by missing-job count (full list in `/tmp/shop-bp/B6-timeouts.txt`):

| File | runs-on | timeouts | missing |
|---|---|---|---|
| `monolith-typescript-acceptance-stage-cloud.yml` | 19 | 10 | 9 |
| `monolith-dotnet-acceptance-stage-cloud.yml` | 19 | 6 | 13 |
| `monolith-java-acceptance-stage-cloud.yml` | 19 | 6 | 13 |
| `multitier-typescript-acceptance-stage-cloud.yml` | 20 | 10 | 10 |
| `multitier-dotnet-acceptance-stage-cloud.yml` | 20 | 6 | 14 |
| `multitier-java-acceptance-stage-cloud.yml` | 20 | 6 | 14 |
| `meta-release-stage.yml` | 9 | 0 | 9 |
| `_prerelease-pipeline.yml` | 7 | 0 | 7 |

Some cloud variants already set timeouts on a subset of jobs (typescript flavors are most consistent at 10 — partial coverage suggests there's an ongoing effort).

**Why this matters:** Without `timeout-minutes:`, a hung process (network deadlock, infinite retry loop, GHCR API stall) consumes a runner for up to 6 hours before being killed. For a CI pipeline that fans out 20+ jobs per push, that is hours of wasted runner time and delayed feedback.

**Proposed fix:** Set `timeout-minutes:` on every job. Reasonable defaults by job kind:
- Quick gates / setup / signoff jobs: `timeout-minutes: 5`
- Build / unit-test jobs: `timeout-minutes: 15`
- System-test / deployment / cloud jobs: `timeout-minutes: 30`
- Long-running e2e suites: explicit per-job override, e.g. `60`

A repo-wide policy works well: pick a default like 20 and override only when justified.

**Risk:** **Medium.** Reliability and runner-cost win; no behavior change on the happy path.

---

## B8 — Default shell hardening

**Symptom**

```yaml
# .github/workflows/monolith-typescript-acceptance-stage-cloud.yml
# 32 run: steps in this file, no defaults: { run: { shell: bash } }
jobs:
  build:
    steps:
    - run: echo hi   # implicit shell — bash on linux, pwsh on windows
    - run: ./build.sh
    ...
```

**Affected files (36 workflows with >=3 `run:` steps and no top-level `defaults:` block):**

The shop runs only on `ubuntu-latest`, so the implicit shell is bash and there is no portability bug today. Still, an explicit `defaults: { run: { shell: bash } }` makes the contract clear and prevents accidental breakage if a future job adds `runs-on: windows-latest`. Top of the list (full list in `/tmp/shop-bp/B8-shell.txt`):

- `monolith-typescript-acceptance-stage-cloud.yml` (32 run steps)
- `monolith-dotnet-acceptance-stage-cloud.yml` (28)
- `monolith-dotnet-acceptance-stage-legacy.yml` (30)
- `monolith-java-acceptance-stage-legacy.yml` (29)
- `monolith-typescript-acceptance-stage-legacy.yml` (29)
- and 31 more

Many individual steps DO declare `shell: bash` explicitly — this finding is for jobs that mix explicit and implicit and would benefit from a single top-level default.

**Why this matters:** Cosmetic / future-proofing on a Linux-only repo. Real risk only emerges if a windows-latest job is added.

**Proposed fix:** Add to each affected workflow once at the top:
```yaml
defaults:
  run:
    shell: bash
```

**Risk:** **Low.** Pure consistency / future-proofing.

---

## B11 — Trigger hygiene

### B11a — `on: push` without `branches:` filter

**Affected files (8 workflows):**

- `.github/workflows/meta-release-stage.yml`
- `.github/workflows/monolith-dotnet-commit-stage.yml`
- `.github/workflows/monolith-java-commit-stage.yml`
- `.github/workflows/monolith-typescript-commit-stage.yml`
- `.github/workflows/multitier-backend-dotnet-commit-stage.yml`
- `.github/workflows/multitier-backend-java-commit-stage.yml`
- `.github/workflows/multitier-backend-typescript-commit-stage.yml`
- `.github/workflows/multitier-frontend-react-commit-stage.yml`

These workflows trigger on every push to every branch. Most should be limited to `main` (and the lesson-branch convention). Several already use `paths:` filters to scope to a directory, but combining `paths:` with `branches:` is the canonical pattern.

**Proposed fix:**
```yaml
on:
  push:
    branches: [main]
    paths: [...]
```

**Risk:** **Low.** Saves runner minutes; unlikely to mask a real bug.

### B11b — `workflow_dispatch:` without `inputs:` documentation

**Affected files (14 workflows):** Soft flag only — `workflow_dispatch:` with no inputs is fine for a "run on demand with no parameters" workflow. Listed in `/tmp/shop-bp/B11-dispatch-no-inputs.txt`. Most are stage workflows where defaults are fine. **No fix proposed unless the user wants them added.**

**Risk:** **Low.**

---

## B12 — `if:` correctness

### B12a — `if: always()` on jobs that depend on others

**Symptom**

```yaml
# .github/workflows/monolith-dotnet-commit-stage.yml:201
summary:
  needs: [check, run]
  if: always()        # runs even on cancellation — usually wrong, prefer !cancelled()
  runs-on: ubuntu-latest
  steps:
    - uses: optivem/actions/render-stage-summary@v1
```

**Affected jobs (53 of 57 `if: always()` occurrences are on summary/notify/cleanup jobs that have a `needs:` block; full list in `/tmp/shop-bp/B12-always.txt`):**

This pattern repeats across nearly every stage workflow (every `*-commit-stage.yml`, `*-acceptance-stage*.yml`, `*-qa-stage*.yml`, `*-prod-stage*.yml`, `*-qa-signoff.yml` — both monolith and multitier, all three languages). It's clearly a copy-pasted convention.

The remaining 4 `if: always()` occurrences are inside steps (not jobs) — typically inside matrix entries that genuinely should run regardless of upstream cancellation. Those are fine.

**Why this matters:** `always()` runs the summary job even when the workflow is cancelled by the user. `${{ !cancelled() }}` is the documented modern equivalent: it runs on success and failure but stops on user-initiated cancellation. For summary / notification jobs, that is what is actually wanted.

**Proposed fix:** Replace job-level `if: always()` with `if: ${{ !cancelled() }}` in every summary job. Keep step-level `always()` only when a teardown step truly must run on cancellation.

**Risk:** **Low.** Pedagogical / behavior is identical except in the cancellation edge case. Worth surfacing to the course author — the explicit `always()` may be intentional for the lesson.

### B12b — `if: success()` (redundant)

**Symptom**

```yaml
# .github/workflows/meta-prerelease-dry-run.yml:87
trigger-stage:
  if: success() && inputs.auto-trigger-stage  # success() is the implicit default
```

**Affected files (1):**
- `.github/workflows/meta-prerelease-dry-run.yml:87` — could be simplified to `if: ${{ inputs.auto-trigger-stage }}`

**Risk:** **Low.** Cosmetic.

---

## B14 — Cache opportunities

**Symptom**

```yaml
# .github/workflows/monolith-dotnet-commit-stage.yml:79
- uses: actions/setup-dotnet@v5
  with:
    dotnet-version: 8.0.x
    # no cache: true / cache-dependency-path
- run: dotnet restore
- run: dotnet build
```

**Affected steps (64 individual `setup-*` steps across 18 distinct workflows; full list in `/tmp/shop-bp/B14-cache.txt`):**

Top affected workflows by step count:

- `monolith-dotnet-acceptance-stage-cloud.yml` (12 setup-dotnet steps, 0 with cache)
- `multitier-dotnet-acceptance-stage-cloud.yml` (11 setup-dotnet)
- `monolith-typescript-acceptance-stage-cloud.yml` (~10 setup-node)
- `monolith-java-acceptance-stage-cloud.yml` (~12 setup-java)
- `_prerelease-pipeline.yml` (one each of node, java, dotnet)

**Why this matters:** Each cloud-stage job pulls a Docker image and runs `dotnet restore` / `npm ci` / `./gradlew build` from a cold cache. Adding `cache: true` (or `cache: 'gradle'` / `cache: 'npm'` / `cache-dependency-path: ...`) to the setup action cuts 30-90 seconds per job. With 12 such jobs in a single workflow, that's 6-18 minutes saved per run.

**Proposed fix:**
- `actions/setup-node`: add `cache: 'npm'` (or `'pnpm'`) and `cache-dependency-path:` pointing at the lockfile
- `actions/setup-java`: add `cache: 'gradle'` and let the toolchain handle it (or use `gradle/actions/setup-gradle@v6` which is already used in places — be consistent)
- `actions/setup-dotnet`: add `cache: true` and `cache-dependency-path: '**/packages.lock.json'` (requires `<RestorePackagesWithLockFile>true` in csproj — verify before bulk-applying)

**Risk:** **Low.** Performance optimization; no behavior change. Some setup-dotnet caching requires lockfiles which may not exist yet — verify per project before applying.

---

## C4 — Composite internal pin discipline

**Symptom**

```yaml
# ../actions/render-system-stage-summary/action.yml:64-75
- uses: optivem/actions/format-artifact-list@v1   # internal composite-to-composite, @v1 tag
- uses: optivem/actions/format-artifact-list@v1
- uses: optivem/actions/render-stage-summary@v1
```

**Affected files (1 composite, 3 internal references):**
- `../actions/render-system-stage-summary/action.yml:64`
- `../actions/render-system-stage-summary/action.yml:70`
- `../actions/render-system-stage-summary/action.yml:75`

Same pattern as B1 — internal references use the `@v1` floating tag rather than a SHA. Whatever decision is made for B1 should apply consistently here.

**Why this matters:** Composite actions are shared infrastructure with consumers beyond shop. A change to `format-artifact-list@v1` affects every workflow that calls `render-system-stage-summary`. Pin discipline matters more here, not less.

**Proposed fix:** Apply the same B1 strategy (SHA pin or document `@v1` as the supported moving major). If SHA-pinning, coordinate with the `optivem/actions` repo owner — internal pin updates are a release concern for that repo.

**Risk:** **Medium.** Surface-B change with multi-consumer impact. Do not apply without owner sign-off.

---

## Priority order

| Priority | Check | Why |
|---|---|---|
| **P1** | B5 — concurrency on commit-stage and pipeline workflows | reliability win, prevents deploy/tag races |
| **P1** | B6 — `timeout-minutes` on jobs (298 jobs across 76 workflows) | runner-cost & feedback-time win, no behavior change |
| **P2** | B1 — third-party action pinning (50 unique refs) | supply-chain hardening; no imminent deadline |
| **P2** | C4 — composite-to-composite pinning (mirror B1 decision) | coordinate with `../actions` repo owner |
| **P3** | B4 — top-level `permissions:` block (81 workflows) | hardening; review with course author for pedagogical intent |
| **P3** | B14 — cache opt-in (64 setup-* steps) | performance; verify lockfiles exist before applying |
| **P3** | B12a — `if: always()` → `if: ${{ !cancelled() }}` (53 jobs) | semantic correctness; could be intentional teaching example |
| **P3** | B11a — `on: push` branches filter (8 workflows) | runner-minute savings |
| **P3** | B8 — `defaults: { run: { shell: bash } }` (36 workflows) | future-proofing for windows-latest; no current bug |
| **P4** | B11b — workflow_dispatch inputs documentation (14) | documentation only |
| **P4** | B12b — `if: success()` redundancy (1) | cosmetic |

No P0 findings — no `pull_request_target` exposure, no deprecated `upload-artifact@v3`, no `permissions: write-all`, no secrets interpolated into shell strings.

## Cross-cutting risk

- **Pinning gaps span both surfaces.** B1 and C4 are the same problem from two angles: every external action and every `optivem/actions` composite is pinned to a moving tag. If you decide to SHA-pin, do it across both surfaces in one coordinated pass — half-pinning is worse than not pinning at all because it gives a false sense of audit completeness.
- **Most B4/B5/B6/B12 findings are systematic, not per-file.** The 81-workflow B4 gap and the 76-workflow B6 gap reflect a missing template, not 81 independent oversights. The fix is a workflow-template policy (or a shared "prelude" composite) rather than 81 PRs.
- **Pedagogical caveat.** The repo is course material. Several findings (B4 missing top-level permissions, B12 explicit `always()`, B6 missing timeouts, B11 catch-all `on: push`) may be deliberate simplifications used to teach defaults before introducing hardening. Review B4, B6, B12, and B11 with the course author before bulk-applying — they are the most likely candidates for "leave it, it's a teaching example."
- **Surface-B impact.** Anything proposed for `../actions/*/action.yml` (here, just C4) affects every consumer of that repo, not just shop. The shop course material should not be the driving force for breaking changes to shared infrastructure.
- **No security blocker.** The two security-axis checks (B7 PR-target, B9 secrets in run-script bodies) returned zero findings. This is an audit of hardening polish, not an incident response.
