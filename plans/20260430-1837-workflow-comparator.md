# 20260430-1837 — Workflow Diff Plan

Architecture: both
Stage: all

## Differences

### DIFF-1: monolith commit-stage path filters do not exclude the VERSION file

**Stage:** commit-stage
**Scope:** monolith — Java, .NET, TypeScript (all three drift the same way relative to multitier)

**Files:**
- `.github/workflows/monolith-java-commit-stage.yml:6-13` — `paths:` filters include `system/monolith/java/**` with no exclusion.
- `.github/workflows/monolith-dotnet-commit-stage.yml:6-13` — `paths:` filters include `system/monolith/dotnet/**` with no exclusion.
- `.github/workflows/monolith-typescript-commit-stage.yml:6-13` — `paths:` filters include `system/monolith/typescript/**` with no exclusion.
- `.github/workflows/multitier-backend-java-commit-stage.yml:6-15` — `paths:` filters include `system/multitier/backend-java/**` and `!system/multitier/backend-java/VERSION`.
- `.github/workflows/multitier-backend-dotnet-commit-stage.yml:6-15` — same exclusion.
- `.github/workflows/multitier-backend-typescript-commit-stage.yml:6-15` — same exclusion.
- `.github/workflows/multitier-frontend-react-commit-stage.yml:6-15` — same exclusion.

**Details:**
The multitier commit-stage workflows explicitly exclude their own `VERSION` file from the path filter (e.g. `'!system/multitier/backend-java/VERSION'`). The monolith commit-stage workflows do not. Because the post-release `bump-patch-version-monolith-*` workflows commit a new VERSION value to `system/monolith/<lang>/VERSION`, that commit will retrigger the corresponding monolith commit stage, building & pushing a needless `dev` tag. The multitier side has already addressed this; the monolith side has not.

**Recommendation:**
Add `!system/monolith/<lang>/VERSION` to both the `push.paths` and `pull_request.paths` blocks in each of the three monolith commit-stage YAMLs. Use the multitier files as the template.

---

### DIFF-2: monolith commit stages do not expose `component-version` as a job output

**Stage:** commit-stage
**Scope:** monolith — Java, .NET, TypeScript

**Files:**
- `.github/workflows/monolith-java-commit-stage.yml:69-70` — `run.outputs:` only declares `image-digest-url`.
- `.github/workflows/monolith-dotnet-commit-stage.yml:69-70` — same.
- `.github/workflows/monolith-typescript-commit-stage.yml:69-70` — same.
- `.github/workflows/multitier-backend-java-commit-stage.yml:71-73` — `run.outputs:` declares both `component-version` and `image-digest-url`.
- `.github/workflows/multitier-backend-dotnet-commit-stage.yml:71-73` — same.
- `.github/workflows/multitier-backend-typescript-commit-stage.yml:71-73` — same.
- `.github/workflows/multitier-frontend-react-commit-stage.yml:71-73` — same.

**Details:**
All four multitier commit-stage workflows expose `component-version: ${{ steps.read-version.outputs.base-version }}` as a job output, with `Read Base Component Version` placed early in the `run` job (line 87, before language setup). The three monolith commit-stage workflows place the same step late (after lint and Sonar at lines 122-130) and do not expose it as an output — and gate it behind `if: steps.verify-main.outputs.on-branch == 'true'`, which means PR runs cannot read the base version even if a downstream step needed it.

**Recommendation:**
Standardize on the multitier layout for both architectures: move `Read Base Component Version` to immediately after `Verify Built SHA Is On Main` (drop the `on-branch` guard so it runs unconditionally), and add `component-version: ${{ steps.read-version.outputs.base-version }}` to the `run.outputs:` block in all three monolith commit-stage files. This keeps cross-architecture parity and makes the monolith commit stage's component version available to downstream meta workflows the same way the multitier stage does.

---

### DIFF-3: TODO placeholders inconsistent between languages in commit stages

**Stage:** commit-stage
**Scope:** monolith and multitier — varies per language

**Files:**
- `.github/workflows/monolith-java-commit-stage.yml:104-118` — `Run Unit Tests` implemented (`./gradlew test`); `Run Narrow Integration Tests`, `Run Component Tests`, `Run Contract Tests` are TODO.
- `.github/workflows/monolith-dotnet-commit-stage.yml:93-107` — `Run Unit Tests` is TODO; the other three are TODO.
- `.github/workflows/monolith-typescript-commit-stage.yml:99-113` — `Run Unit Tests` is TODO; the other three are TODO.
- `.github/workflows/multitier-backend-java-commit-stage.yml:113-127` — `Run Unit Tests` implemented; the other three are TODO.
- `.github/workflows/multitier-backend-dotnet-commit-stage.yml:102-116` — `Run Unit Tests` implemented (`dotnet test`); the other three are TODO.
- `.github/workflows/multitier-backend-typescript-commit-stage.yml:108-122` — `Run Unit Tests` implemented (`npm test`); the other three are TODO.
- `.github/workflows/multitier-frontend-react-commit-stage.yml:108-122` — `Run Unit Tests` is TODO; the other three are TODO.

**Details:**
For the unit-test step the implementation status is split:
- Implemented in: monolith-java, multitier-backend-java, multitier-backend-dotnet, multitier-backend-typescript.
- TODO in: monolith-dotnet, monolith-typescript, multitier-frontend-react.

In particular monolith .NET and monolith TypeScript still echo `"TODO - not yet implemented"` while their multitier-backend siblings invoke `dotnet test` / `npm test`. Narrow Integration / Component / Contract tests are TODO across all seven workflows, so those are consistent in being unimplemented.

**Recommendation:**
1. Implement `Run Unit Tests` in `monolith-dotnet-commit-stage.yml` (`run: dotnet test`, `working-directory: system/monolith/dotnet`) and `monolith-typescript-commit-stage.yml` (`run: npm test`, `working-directory: system/monolith/typescript`). Use the multitier-backend equivalents as the template.
2. For `multitier-frontend-react-commit-stage.yml`: implement `Run Unit Tests` once a frontend test suite is wired up (likely `npm test`); until then, document explicitly why this step is still TODO so it is clearly distinguishable from drift.
3. Leave Narrow Integration / Component / Contract Tests as TODO across all seven (they are consistently unimplemented), but track them as a single open item rather than seven.

---

### DIFF-4: monolith acceptance-stage missing the `sonar` job that exists in multitier

**Stage:** acceptance-stage
**Scope:** monolith vs multitier — Java, .NET, TypeScript

**Files:**
- `.github/workflows/monolith-java-acceptance-stage.yml:378-396` — `summary` job depends on `[preflight, check, run, publish-tag]`. No `sonar` job.
- `.github/workflows/monolith-dotnet-acceptance-stage.yml:380-398` — same shape, no `sonar` job.
- `.github/workflows/monolith-typescript-acceptance-stage.yml:367-385` — same shape, no `sonar` job.
- `.github/workflows/multitier-java-acceptance-stage.yml:369-401` — has dedicated `sonar` job (`needs: run`) running `pwsh ./Run-Sonar.ps1` from `system-test/java`.
- `.github/workflows/multitier-dotnet-acceptance-stage.yml:369-390` — has dedicated `sonar` job running `pwsh ./Run-Sonar.ps1` from `system-test/dotnet`.
- `.github/workflows/multitier-typescript-acceptance-stage.yml:359-382` — has dedicated `sonar` job running `pwsh ./Run-Sonar.ps1` from `system-test/typescript`.

**Details:**
Multitier acceptance-stage workflows include a separate `sonar` job that runs Sonar analysis of the system-test project after the main `run` job. The monolith acceptance-stage workflows have no equivalent — Sonar coverage of the system-test code is therefore measured for multitier but not for monolith, even though the system-test code is the same across both architectures (one Java/.NET/TypeScript test project per language drives both monolith and multitier SUTs).

**Recommendation:**
Add an equivalent `sonar` job to each of the three monolith acceptance-stage workflows, mirroring the multitier shape exactly (Setup language → Run-Sonar.ps1). Update the `summary` job's `needs:` list to include `sonar`. This gives monolith parity with multitier and keeps Sonar feedback aligned with where the system-test code actually lives.

Alternatively (recommended only if Sonar runs are intentionally not duplicated): leave monolith without the job and document in `docs/operations/...` that monolith acceptance-stage explicitly relies on multitier acceptance-stage to publish the system-test Sonar analysis. Pick one — current state is undocumented drift.

---

### DIFF-5: monolith prod-stage does not pass `REPO_TOKEN` to `read-base-versions`

**Stage:** prod-stage
**Scope:** monolith vs multitier — Java, .NET, TypeScript

**Files:**
- `.github/workflows/monolith-java-prod-stage.yml:189-197` — `Read Component Base Versions` step uses `optivem/actions/read-base-versions@v1` with no `token:` input.
- `.github/workflows/monolith-dotnet-prod-stage.yml:189-197` — same, no `token:`.
- `.github/workflows/monolith-typescript-prod-stage.yml:189-197` — same, no `token:`.
- `.github/workflows/multitier-java-prod-stage.yml:192-203` — same step passes `token: ${{ secrets.REPO_TOKEN }}`.
- `.github/workflows/multitier-dotnet-prod-stage.yml:192-203` — same, passes `REPO_TOKEN`.
- `.github/workflows/multitier-typescript-prod-stage.yml:192-203` — same, passes `REPO_TOKEN`.

**Details:**
The multitier prod stages need `REPO_TOKEN` because they read two VERSION files (backend + frontend) keyed against image URLs containing the just-tagged RC. The monolith prod stages read one VERSION file keyed against one image URL and pass no token. If the underlying action `optivem/actions/read-base-versions@v1` requires the token only when it has to resolve refs across components, this may be intentional. If the action always benefits from it (e.g., to avoid rate limits), monolith is silently degraded.

**Recommendation:**
Verify whether `read-base-versions@v1` uses the `token:` input only for cross-component resolution or also for general rate-limit / authenticated read paths. If the latter, add `token: ${{ secrets.REPO_TOKEN }}` to the `Read Component Base Versions` step in all three monolith prod-stage files. If the former, leave as-is and note in a comment that monolith doesn't need it. This is the only architectural cross-cut that's currently undocumented.

---

### DIFF-6: monolith-dotnet-acceptance-stage-legacy reorders `Compile System Tests` relative to non-legacy

**Stage:** acceptance-stage-legacy
**Scope:** monolith — .NET only (drift within the same language between legacy and non-legacy variants)

**Files:**
- `.github/workflows/monolith-dotnet-acceptance-stage.yml:248-278` — order is: Setup .NET → Cache NuGet → Cache Playwright → Compile System Tests → Install Playwright System Dependencies.
- `.github/workflows/monolith-dotnet-acceptance-stage-legacy.yml:207-232` — order is: Setup .NET → Cache NuGet → **Compile System Tests** → Cache Playwright → Install Playwright System Dependencies.

**Details:**
The non-legacy variant places `Compile System Tests` after `Cache Playwright Browsers`, while the legacy variant places `Compile System Tests` immediately after `Cache NuGet Packages`. Functionally either order works, but the divergence is a code-smell — when one variant is updated for a real reason (e.g. caching strategy change) the other is likely to drift further.

**Recommendation:**
Standardize on the non-legacy order (Setup .NET → Cache NuGet → Cache Playwright → Compile System Tests → Install Playwright System Dependencies) in the legacy variant, since the non-legacy is the canonical pipeline. Apply the same edit to `monolith-dotnet-acceptance-stage-legacy.yml` and `multitier-dotnet-acceptance-stage-legacy.yml` if the same drift exists there (verify before committing).

---

### DIFF-7: cloud acceptance-stage uses `test-contract-isolated-stub` job name; non-cloud uses `contract-stub-isolated`

**Stage:** acceptance-stage / acceptance-stage-cloud
**Scope:** monolith — Java, .NET, TypeScript (consistent within each variant; inconsistent across variants)

**Files:**
- `.github/workflows/monolith-java-acceptance-stage.yml:321-323` — step name `contract-stub-isolated`, suite `--suite contract-stub-isolated`.
- `.github/workflows/monolith-dotnet-acceptance-stage.yml:323-325` — same `contract-stub-isolated`.
- `.github/workflows/monolith-typescript-acceptance-stage.yml:310-312` — same `contract-stub-isolated`.
- `.github/workflows/monolith-java-acceptance-stage-cloud.yml` (and the .NET / TypeScript siblings) — job name is `test-contract-isolated-stub` (note: word order swapped).
- `system-test/java/tests-latest.json` — actual suite id is `contract-stub-isolated`.

**Details:**
The non-cloud acceptance stages use the suite-id ordering `contract-stub-isolated` (matching `tests-latest.json`). The cloud variants name the corresponding job `test-contract-isolated-stub` — `isolated` and `stub` are swapped. While the cloud variant invokes its own gradle/dotnet/npm command (not `gh optivem test --suite ...`), the job name is meant to mirror the suite. The drift makes ad-hoc grep across the two variants confusing and risks divergence if someone renames the suite later.

**Recommendation:**
Rename the cloud-variant job from `test-contract-isolated-stub` to `test-contract-stub-isolated` in all three `monolith-<lang>-acceptance-stage-cloud.yml` files (and any `needs:` references in downstream jobs within the same file). Match the suite-id ordering used in `tests-latest.json` and the non-cloud workflows.

---

### DIFF-8: `_prerelease-pipeline.yml` uses `node-version: 22` while every other workflow uses `22.x`

**Stage:** infrastructure (reusable workflow used by prerelease-pipeline drivers)
**Scope:** cross-architecture, infrastructure

**Files:**
- `.github/workflows/_prerelease-pipeline.yml` — `node-version: 22`.
- All other workflows that set up Node — `node-version: 22.x` (monolith-typescript-commit-stage, monolith-typescript-acceptance-stage, monolith-typescript-acceptance-stage-cloud, monolith-typescript-acceptance-stage-legacy, multitier-backend-typescript-commit-stage, multitier-frontend-react-commit-stage, multitier-typescript-acceptance-stage, multitier-typescript-acceptance-stage-cloud, multitier-typescript-acceptance-stage-legacy, cross-lang-system-verification).

**Details:**
`actions/setup-node` resolves both `22` and `22.x` to the latest 22.x release, so functionally these are equivalent today. But the inconsistency (`22` vs `22.x`) is pure formatting drift that will surface in greps and audits. Java and .NET versions (`21`, `8.0.x`) match across all workflows, so this is the only outlier.

**Recommendation:**
Change `_prerelease-pipeline.yml`'s `node-version: 22` to `node-version: 22.x` to match every other workflow in the repo.

---

### DIFF-9: monolith-dotnet-commit-stage Run Code Analysis uses inline scanner; multitier-backend-dotnet uses the same inline scanner but with fewer ignore rules

**Stage:** commit-stage
**Scope:** monolith vs multitier — .NET

**Files:**
- `.github/workflows/monolith-dotnet-commit-stage.yml:113-120` — sonarscanner ignore-multicriteria covers e1..e7 (S2699, S1118, S2068, S4654 css, S1186, S1075, S3267).
- `.github/workflows/multitier-backend-dotnet-commit-stage.yml:122-129` — covers e1..e5 (S2699, S1118, S2068, S1075, S3267) — drops the `S4654 css/wwwroot` and `S1186 Pages/**` rules.

**Details:**
The two scanners ignore different sets of rules. The wwwroot/css and Pages/** ignores were monolith-specific (Razor Pages app), so dropping them in the API-only backend is reasonable. But this is silent drift — there is no comment explaining why each list is what it is, and any future tweak to the monolith list will not propagate. The TypeScript and Java equivalents use action-based scanner calls that ignore nothing inline (rules live in build.gradle / sonar-project.properties), so the .NET side is the only one with inline rule lists at all.

**Recommendation:**
Move the inline ignore-multicriteria settings out of the workflow YAML and into `SonarAnalyzer.json` (or a `sonar.properties` adjacent to the .csproj). The workflow then reduces to a `sonarscanner begin` with project key + token + organization, mirroring the much shorter Java and TypeScript invocations. Both monolith and multitier .NET commit stages converge on the same shape and any future Sonar tweak is a code edit, not a workflow edit.

---

### DIFF-10: `bump-patch-version-*.yml` file names invert the convention used for stage workflows

**Stage:** post-release tooling
**Scope:** all flavors

**Files:**
- `.github/workflows/bump-patch-version-monolith-java.yml` — internal `name:` is `monolith-java-bump-patch-version`.
- `.github/workflows/bump-patch-version-monolith-dotnet.yml` — internal `name:` is `monolith-dotnet-bump-patch-version`.
- `.github/workflows/bump-patch-version-monolith-typescript.yml` — internal `name:` is `monolith-typescript-bump-patch-version`.
- ... (same pattern for the multitier variants and `bump-patch-version-multitier-frontend-react.yml`).
- All stage workflows follow `<flavor>-<stage>.yml`, e.g. `monolith-java-commit-stage.yml`.

**Details:**
The bump-patch-version files are named `bump-patch-version-<flavor>.yml` while every other per-flavor workflow uses `<flavor>-<stage>.yml`. The internal `name:` field of the bump files matches the `<flavor>-<stage>` convention, so the file name is the only mismatch. This makes glob patterns like `monolith-java-*.yml` miss the bump file and forces a separate `bump-patch-version-*.yml` glob.

**Recommendation:**
Rename the seven `bump-patch-version-*.yml` files to `<flavor>-bump-patch-version.yml` (e.g. `monolith-java-bump-patch-version.yml`). Update all `uses: ./.github/workflows/bump-patch-version-<flavor>.yml` references in the prod-stage workflows accordingly. This is a non-functional rename but it makes the per-flavor file glob pattern uniform.

---

### DIFF-11: monolith Java acceptance-stage `Cache Playwright Browsers` uses `id: cache-playwright`; .NET and TypeScript monolith equivalents do not set an id

**Stage:** acceptance-stage
**Scope:** monolith — Java vs .NET / TypeScript (cosmetic drift)

**Files:**
- `.github/workflows/monolith-java-acceptance-stage.yml:261-267` — `Cache Playwright Browsers` has `id: cache-playwright`.
- `.github/workflows/monolith-dotnet-acceptance-stage.yml:256-261` — same step, no `id:`.
- `.github/workflows/monolith-typescript-acceptance-stage.yml:250-255` — same step, no `id:`.
- `.github/workflows/multitier-java-acceptance-stage.yml:264-270` — same as monolith Java, has `id: cache-playwright`.
- `.github/workflows/multitier-dotnet-acceptance-stage.yml:259-264` — no `id:`.
- `.github/workflows/multitier-typescript-acceptance-stage.yml:253-258` — no `id:`.

**Details:**
The step `id` is unused (no later step references `steps.cache-playwright.outputs.cache-hit`) so functionally this has no effect. But it is unexplained drift between languages and is the kind of thing that tempts someone to add a conditional later that works in Java and silently no-ops in .NET / TypeScript.

**Recommendation:**
Either (a) add `id: cache-playwright` to all six (monolith + multitier) `Cache Playwright Browsers` steps in .NET and TypeScript, in case a future step wants to gate on cache-hit, or (b) remove the unused `id: cache-playwright` from the two Java workflows. Option (a) is preferred because it keeps the door open for a `Skip Playwright install if cached` optimization without re-introducing drift.

---

### DIFF-12: monolith-dotnet acceptance-stage(non-legacy) and acceptance-stage-legacy use different orderings for `Compile System Tests` (cross-reference of DIFF-6) — also `Compile System Tests` step is missing in Java / TypeScript

**Stage:** acceptance-stage / acceptance-stage-legacy
**Scope:** monolith and multitier — .NET only has the step; Java and TypeScript do not

**Files:**
- `.github/workflows/monolith-dotnet-acceptance-stage.yml:263-269` — has `Compile System Tests` (`dotnet build` in `system-test/dotnet`).
- `.github/workflows/monolith-java-acceptance-stage.yml` — no equivalent step. Compile happens implicitly via `setupCommands` when the first `gh optivem test system` runs (without `--no-setup`).
- `.github/workflows/monolith-typescript-acceptance-stage.yml` — no equivalent step. `npm ci` is implicit via `setupCommands`.

**Details:**
.NET pre-builds the system-test project explicitly because `playwright.ps1` lives in the build output and the next step (`Install Playwright System Dependencies`) needs the script to exist on disk. The comment at lines 264-267 of `monolith-dotnet-acceptance-stage.yml` explains this. Java and TypeScript don't have that need (Java's `installPlaywright` Gradle task pulls browsers as part of the test task; TypeScript runs `npx playwright install-deps chromium` directly without needing pre-build).

This is **expected drift driven by a Playwright tooling difference**. No action required, but worth annotating.

**Recommendation:**
No code change. Add a one-line comment to the .NET workflows pointing at the Java / TypeScript equivalents to make the asymmetry intentional and visible: e.g. `# .NET-only pre-build: needed because playwright.ps1 lives in the build output.` (the existing comment at line 264 is correct but doesn't explicitly call out the cross-language asymmetry).

---

## Summary

| #       | Stage                                       | Issue                                                                                                  | Recommendation                                                                                |
|---------|---------------------------------------------|--------------------------------------------------------------------------------------------------------|-----------------------------------------------------------------------------------------------|
| DIFF-1  | commit-stage                                | Monolith path filters miss `!system/monolith/<lang>/VERSION` exclusion                                 | Add VERSION exclusion to all three monolith commit-stage `paths:` blocks                      |
| DIFF-2  | commit-stage                                | Monolith commit stages don't expose `component-version` output (multitier does)                        | Move `Read Base Component Version` early and add `component-version` output                   |
| DIFF-3  | commit-stage                                | `Run Unit Tests` is TODO in monolith-dotnet, monolith-typescript, multitier-frontend-react             | Implement `dotnet test` / `npm test` in monolith .NET and TS; document frontend-react TODO    |
| DIFF-4  | acceptance-stage                            | Monolith acceptance-stage missing `sonar` job that all multitier variants have                         | Add a `sonar` job to all three monolith acceptance-stage workflows mirroring multitier        |
| DIFF-5  | prod-stage                                  | Monolith prod-stage `Read Component Base Versions` lacks `token: ${{ secrets.REPO_TOKEN }}`            | Verify intent; if needed, add `token: REPO_TOKEN` for parity with multitier                   |
| DIFF-6  | acceptance-stage-legacy                     | `monolith-dotnet-acceptance-stage-legacy` reorders `Compile System Tests` relative to non-legacy       | Match non-legacy order (Cache Playwright before Compile System Tests)                         |
| DIFF-7  | acceptance-stage / acceptance-stage-cloud   | Cloud variants name the job `test-contract-isolated-stub`; non-cloud uses `contract-stub-isolated`     | Rename the cloud-variant job to `test-contract-stub-isolated` to match the suite id           |
| DIFF-8  | infrastructure                              | `_prerelease-pipeline.yml` uses `node-version: 22`; everything else uses `22.x`                        | Change to `22.x` for consistency                                                              |
| DIFF-9  | commit-stage                                | .NET inline Sonar ignore lists drift between monolith (e1..e7) and multitier-backend (e1..e5)          | Move ignore rules out of YAML into Sonar config files; converge workflow shape on Java/TS     |
| DIFF-10 | post-release tooling                        | `bump-patch-version-<flavor>.yml` file names invert the `<flavor>-<stage>.yml` convention              | Rename to `<flavor>-bump-patch-version.yml` and update prod-stage `uses:` references          |
| DIFF-11 | acceptance-stage                            | `Cache Playwright Browsers` has `id: cache-playwright` only in Java workflows                          | Add the id to .NET and TypeScript workflows so future cache-hit gates work uniformly          |
| DIFF-12 | acceptance-stage / acceptance-stage-legacy  | `Compile System Tests` step exists only in .NET (Java / TS pre-build via setupCommands)                | No code change; expand the existing comment to make the cross-language asymmetry explicit     |

**Total: 12 inconsistencies found**

By architecture:
  - Monolith (only): 5 (DIFF-1, DIFF-3 partial, DIFF-4, DIFF-6, DIFF-7)
  - Multitier (only): 0
  - Cross-architecture (monolith vs multitier drift): 4 (DIFF-2, DIFF-5, DIFF-9, DIFF-12)
  - Both (per-language drift inside both architectures): 3 (DIFF-3 cross-arch coverage, DIFF-8, DIFF-10, DIFF-11)

By severity:
  - Missing steps/jobs: 2 (DIFF-2 component-version output, DIFF-4 sonar job)
  - TODO placeholders: 1 (DIFF-3)
  - Configuration mismatches: 5 (DIFF-1 path filter, DIFF-5 token, DIFF-6 step order, DIFF-9 sonar ignore lists, DIFF-11 step id)
  - Naming / cosmetic: 3 (DIFF-7 suite name swap, DIFF-8 node-version, DIFF-10 file name convention)
  - Documentation only: 1 (DIFF-12 — drift exists but is justified, needs a comment)
  - Action version mismatches: 0 (all uses-pinned actions are at the same version across languages)

---

> **Note (2026-05-11):** references to `Run-Sonar.ps1` above are historical.
> The PS1 scripts were replaced by `run-sonar.sh` (`./run-sonar.sh` invocation
> in workflows) per `gh-optivem/plans/20260511-1830-bash-sonar-invocation.md`.
