# Plan — Shop Workflow Warnings Cleanup

**Date:** 2026-04-29
**Source audit:** Latest runs of all 77 workflows in optivem/shop. 50 had completed runs (37 success + 13 failure) and were analyzed; 27 cloud / orchestration / bump-patch workflows have never run and were skipped; 0 in progress.
**Scope:** Address warnings (not failures). Failures are listed in "Out of scope" only.

## Out of scope

### Workflows with no runs (27, skipped)

Cloud (16 — never run, missing `GCP_*` config):
`monolith-dotnet-acceptance-stage-cloud`, `monolith-dotnet-prod-stage-cloud`, `monolith-dotnet-qa-stage-cloud`,
`monolith-java-acceptance-stage-cloud`, `monolith-java-prod-stage-cloud`, `monolith-java-qa-stage-cloud`,
`monolith-typescript-prod-stage-cloud`, `monolith-typescript-qa-stage-cloud`,
`multitier-dotnet-acceptance-stage-cloud`, `multitier-dotnet-prod-stage-cloud`, `multitier-dotnet-qa-stage-cloud`,
`multitier-java-acceptance-stage-cloud`, `multitier-java-prod-stage-cloud`, `multitier-java-qa-stage-cloud`,
`multitier-typescript-acceptance-stage-cloud`, `multitier-typescript-prod-stage-cloud`, `multitier-typescript-qa-stage-cloud`.

Reusable / dispatch-only (4): `_prerelease-pipeline`, `_meta-prerelease-pipeline`, `bump-patch-version-meta`, `bump-patch-version`.

Per-language bump-patch (6 — manually triggered): `monolith-dotnet-bump-patch-version`, `monolith-java-bump-patch-version`, `monolith-typescript-bump-patch-version`, `multitier-dotnet-bump-patch-version`, `multitier-java-bump-patch-version`, `multitier-typescript-bump-patch-version`.

### Latest run failed entirely (13 — out of scope for this audit)

These are failures, not warnings. Surfacing only the top error so they aren't lost:

| Workflow | Error |
|---|---|
| `monolith-dotnet-qa-signoff` | `gh: You must authenticate via a GitHub App. (HTTP 403)` in `Record QA Signoff Check Run` |
| `monolith-java-qa-signoff` | same `gh ... HTTP 403` in `Record QA Signoff Check Run` |
| `monolith-typescript-qa-signoff` | same `gh ... HTTP 403` |
| `multitier-java-qa-signoff` | same `gh ... HTTP 403` |
| `multitier-typescript-qa-signoff` | same `gh ... HTTP 403` |
| `monolith-typescript-acceptance-stage-cloud` (last ran 2026-04-20) | `Missing required config: GCP_PROJECT_ID GCP_REGION GCP_WORKLOAD_IDENTITY_PROVIDER GCP_SERVICE_ACCOUNT` |
| `prerelease-pipeline-monolith-{dotnet,java,typescript}` | downstream `qa-signoff` propagation of the HTTP 403 |
| `prerelease-pipeline-multitier-{dotnet,java,typescript}` | same |
| `meta-prerelease-stage` | aggregates the above failures |

The five `qa-signoff` failures and the six `prerelease-pipeline-*` failures share a single root cause (PAT lacks `checks:write` for App-protected check runs) and should be addressed in their own plan, not here.

---

## W1 — Node.js 20 GitHub Actions runner deprecation (Wandalen/wretry.action)

**Symptom**
```
##[warning]Node.js 20 actions are deprecated. The following actions are running on
Node.js 20 and may not work as expected: Wandalen/wretry.action@v3.8.0_js_action.
Actions will be forced to run with Node.js 24 by default starting June 2nd, 2026.
Node.js 20 will be removed from the runner on September 16th, 2026.
[...]
For more information see:
https://github.blog/changelog/2025-09-19-deprecation-of-node-20-on-github-actions-runners/
```

**Affected workflows (~40):** every workflow that calls `Wandalen/wretry.action@v3` — every commit/acceptance/qa/prod stage across all six pipelines, the legacy variants, `cross-lang-system-verification`, `meta-prerelease-stage`, `meta-release-stage`, and the `prerelease-pipeline-*` aggregators. The action is referenced from 51 of the workflow YAMLs in `.github/workflows/`.

**Root cause:** `Wandalen/wretry.action@v3` (the retry wrapper for flaky test steps) ships a `_js_action` shim that pins to Node 20. GitHub will force JS actions to Node 24 on **2 June 2026** and remove Node 20 entirely on **16 September 2026** — both deadlines fall inside the active term of this course. After June 2nd the action may break silently if it isn't Node-24-compatible.

**Proposed fix:**
1. Check upstream `Wandalen/wretry.action` for a Node-24 release (track issue / latest tag).
2. If a Node-24-compatible tag exists, bump every `uses: Wandalen/wretry.action@v3` reference to that tag (51 occurrences). A single repo-wide search/replace works.
3. If no upstream release is forthcoming by mid-May, evaluate replacements: `nick-fields/retry@v3` (Node 20 itself, same problem), inline `for i in 1 2 3; do … done` shell loop, or a thin composite action in `optivem/actions`.
4. As a temporary muffler only (not a fix), `FORCE_JAVASCRIPT_ACTIONS_TO_NODE24=true` can be set on the runner to force-run on Node 24 starting now and surface any breakage early.

**Risk:** **Medium.** Hard external deadline (~5 weeks to the soft cutover, ~20 weeks to removal). Bumping to a newer tag is mechanical, but if upstream doesn't ship Node-24 support in time the fallback (replacement action or inlined retry) is a wider change touching 51 files. Pedagogically inert — students don't read this action.

---

## W2 — npm transitive deprecations (`glob@7`, `glob@10.5.0`, `inflight@1.0.6`)

**Symptom**
```
npm warn deprecated inflight@1.0.6: This module is not supported, and leaks memory.
  Do not use it. Check out lru-cache if you want a good and tested way to coalesce
  async requests by a key value, which is much more comprehensive and powerful.
npm warn deprecated glob@7.2.3: Old versions of glob are not supported, and contain
  widely publicized security vulnerabilities, which have been fixed in the current
  version. Please update.
npm warn deprecated glob@10.5.0: Old versions of glob are not supported, ...
```

**Affected workflows (4):** `monolith-typescript-commit-stage` (6 lines), `multitier-backend-typescript-commit-stage` (13 lines), `monolith-java-acceptance-stage` and `multitier-java-acceptance-stage` (1 line each — `npm warn exec` for Playwright CLI install, benign).

**Root cause:** TypeScript projects in `monolith/typescript/` and `multitier/typescript/backend-typescript/` have transitive deps that pin old `glob` (≤10.5.0 — current is 11.x) and `inflight@1.0.6`. Both warnings are emitted twice — once during host `npm install` and once again inside `Build and Push Docker Image` (so the dev image has the same problem).

**Proposed fix:**
1. Run `npm ls glob inflight` in `monolith/typescript/` and `multitier/typescript/backend-typescript/` to identify the offending direct dependency chain.
2. Add `overrides` in each `package.json` to force `glob@^11` and replace `inflight` with `lru-cache` (the upstream-recommended substitute). Example:
   ```json
   "overrides": {
     "glob": "^11.0.0",
     "inflight": "npm:lru-cache@^11.0.0"
   }
   ```
3. Re-run `npm install`, commit refreshed `package-lock.json`, and rebuild the Docker image to verify the warnings are gone.
4. Verify the same fix in the matching `Dockerfile` build step (warnings appear there too).

**Risk:** **Low–Medium.** `glob` and `inflight` are dev/build-time utilities used by tooling, not runtime app code. `overrides` is a supported npm 8+ feature. Risk is bumping `glob` major could break a tool that reads its API — verify by re-running the existing test suite. Pedagogically inert.

---

## W3 — npm audit: high & moderate vulnerabilities in TypeScript projects

**Symptom**
```
npm warn deprecated glob@10.5.0: ...security vulnerabilities, which have been fixed...
8 vulnerabilities (3 moderate, 5 high)        # multitier-backend-typescript-commit-stage
2 vulnerabilities (1 moderate, 1 high)        # monolith-typescript-commit-stage
15 vulnerabilities (9 moderate, 6 high)       # multitier-backend-typescript-commit-stage Docker build
```

**Affected workflows (2):** `monolith-typescript-commit-stage` (2 vuln), `multitier-backend-typescript-commit-stage` (8 vuln in npm install + 15 in Docker image build = same 15 transitive set).

**Root cause:** `npm install` reports unresolved advisories on transitive deps in the two TS projects. The Docker image build prints a higher count (15) because it does a fresh resolve, suggesting the lockfile in the project also lags. These overlap with W2 — fixing `glob` likely clears most of them.

**Proposed fix:**
1. In `monolith/typescript/` and `multitier/typescript/backend-typescript/`, run `npm audit --omit=dev` to scope to runtime advisories.
2. For each high-severity finding, attempt `npm audit fix` first; for breaking ones, add an `overrides` entry pinning the patched version.
3. Re-run npm install + Docker build and confirm `0 vulnerabilities` (or only `low`).
4. Because this repo is a template students copy, prefer `overrides` over leaving the lockfile dirty — students who run `npm install` from a fresh clone should also see the fix.

**Risk:** **High.** This is a course template — every student that scaffolds from `multitier-backend-typescript` inherits the 15 vulnerabilities (6 high). Even if none is exploitable in the demo, the security smell is exactly what the course warns against. Apply quickly. The fix itself is low-risk because the affected packages are dev/build tooling, not API surface.

---

## W4 — Gradle deprecation: features incompatible with Gradle 9.0

**Symptom**
```
Deprecated Gradle features were used in this build, making it incompatible with Gradle 9.0.

You can use '--warning-mode all' to show the individual deprecation warnings and
determine if they come from your own scripts or plugins.
```

**Affected workflows (4):** `monolith-java-commit-stage` (5 occurrences), `monolith-java-acceptance-stage` (12), `multitier-backend-java-commit-stage` (5), `multitier-java-acceptance-stage` (13). Also amplified inside `cross-lang-system-verification` (156) and `meta-prerelease-stage` (156) which include the Java jobs.

**Root cause:** A Gradle plugin or build script in `monolith/java/` and `multitier/java/backend-java/` uses an API that Gradle 9 will remove. The current message doesn't say which feature — it requires `--warning-mode all` to surface the specific call.

**Proposed fix:**
1. Locally run `./gradlew build --warning-mode all` in `monolith/java/` and `multitier/java/backend-java/` to capture the actual deprecated calls.
2. Update the offending plugin / build-script lines (most likely a Spring Boot plugin or jib version that's a major behind, or a `compile`/`testCompile` config name that's now `implementation`/`testImplementation`).
3. Verify with a clean `./gradlew clean build` — the message should disappear.
4. Optionally add `--warning-mode all` to CI temporarily to make future regressions visible.

**Risk:** **Medium.** Gradle 9 is not yet on the runner — there's no immediate breakage. But the course teaches Gradle, so students copying out-of-date build scripts inherit a known smell. Touching `build.gradle` carries the standard "did this break the build" risk; mitigate by running smoke tests after each plugin bump. Recommend reviewing the eventual plugin/script change with the course author — some patterns may be intentional teaching examples.

---

## W5 — Playwright "Host system is missing dependencies" (Java acceptance)

**Symptom**
```
║ Host system is missing dependencies to run browsers.                         ║
║ Please install them with the following command:                              ║
║                                                                              ║
║   sudo mvn exec:java -e -D exec.mainClass=com.microsoft.playwright.CLI \     ║
║       -D exec.args="install-deps"                                            ║
║                                                                              ║
║ <14 missing system dependencies are listed>                                  ║
```

**Affected workflows (2):** `monolith-java-acceptance-stage` (28 lines = 1 banner per browser launch), `multitier-java-acceptance-stage` (28). Amplified in `cross-lang-system-verification` (488) and `meta-prerelease-stage` (488).

**Root cause:** The Java acceptance jobs install the Playwright NPM CLI (`npm warn exec ... playwright@1.59.1`) to acquire system deps, but the install never actually runs `install-deps`, so each browser launch reprints the banner. Tests still pass (the deps are in fact present on `ubuntu-latest`) so it's pure log noise. The TypeScript and .NET acceptance stages don't have this — they call `npx playwright install-deps` explicitly.

**Proposed fix:**
1. In the Java acceptance composite step (likely `optivem/actions` or inline in the workflow), add an explicit `npx playwright install-deps` (or the Maven equivalent printed in the banner) before the Maven test invocation, mirroring the TS / .NET pipelines.
2. Or, since the deps are present on `ubuntu-latest`, suppress the warning by setting `PLAYWRIGHT_SKIP_HOST_REQUIREMENTS=1` (env var) on the test step.

**Risk:** **Low.** Cosmetic — tests already pass. Fix is one line per pipeline. Verify by re-running both Java acceptance workflows after the change.

---

## W6 — C# nullable reference warnings (CS8603 / CS8604)

**Symptom**
```
##[warning].../system-test/dotnet/Dsl.Core/Scenario/Assume/AssumeStage.cs(22,39):
  warning CS8604: Possible null reference argument for parameter 'channel' in
  'Task<MyShopDsl> UseCaseDsl.MyShop(Channel channel)'.
##[warning].../system-test/dotnet/Dsl.Core/Scenario/When/Steps/Base/BaseWhenStep.cs(38,34):
  warning CS8603: Possible null reference return.
```

**Affected workflows (4):** `monolith-dotnet-acceptance-stage` & legacy (16 each), `multitier-dotnet-acceptance-stage` (32 — twice as many because it builds backend + frontend together), `multitier-dotnet-acceptance-stage-legacy` (16).

**Root cause:** Files under `system-test/dotnet/Dsl.Core/Scenario/**` and `Driver.Adapter/**` have nullable-reference-types enabled but rely on un-asserted parameters. 294 CS8604 + 42 CS8603 occurrences (totalled across all dotnet acceptance runs) all originate in test infrastructure — the production code under `monolith/dotnet/` and `multitier/dotnet/backend-dotnet/` is clean.

**Proposed fix:**
1. In each file, either:
   - Add `ArgumentNullException.ThrowIfNull(channel)` at the top of the offending method, or
   - Mark the parameter `Channel? channel` and adjust callers, or
   - Add `[NotNull]` / `[DisallowNull]` attributes if the call sites are known never to pass null.
2. For the small CS8603 surface (`BaseWhenStep.cs`), change the return type to nullable or assert non-null at the boundary.

**Risk:** **Low.** Test-infra-only, behaviorally inert. Recommend reviewing with the course author first — CS8604 in DSL plumbing may be a deliberate teaching example showing why nullable annotations matter. If so, suppress narrowly with `#pragma warning disable CS8604` plus an explanatory comment, rather than fixing.

---

## W7 — Sonar smells in C# system-test infrastructure

**Symptom**
```
##[warning].../Driver.Adapter/Shared/Client/WireMock/JsonWireMockClient.cs(100,47):
  warning S2955: Use a comparison to 'default(T)' instead or add a constraint to 'T'
  so that it can't be a value type. (https://rules.sonarsource.com/csharp/RSPEC-2955)
##[warning].../Dsl.Core/Scenario/Then/Steps/ThenSuccessAnd.cs(50,70):
  warning S4136: All 'Coupon' method overloads should be adjacent.
##[warning].../system-test/dotnet/...(...): warning S1075: URIs should not be hardcoded
```

**Affected workflows (5):** all dotnet acceptance + commit-stage workflows. Aggregate rule frequency:

| Rule | Count | Meaning |
|---|---|---|
| S4136 | 640 | Overloaded methods should be adjacent |
| S1939 | 160 | Inheritance list should not include redundant interfaces |
| S2955 | 40 | Generic type compared to null without `where T : class` |
| S1075 | 24 | Hardcoded URIs |
| S1186 | 12 | Methods should not be empty |
| S3267 | 4 | Loop should be simplified to LINQ |
| S2699, S2068, S1118 | 4 each | Test without assertion / hardcoded credential / utility class with public ctor |
| S4502 | 2 | CSRF / disable view state |

**Root cause:** All findings live under `system-test/dotnet/` (Dsl.Core, Driver.Adapter, Use.Case, Test.Adapter). The hardcoded URI (`S1075`) and method-overload-adjacency (`S4136`) rules are by far the loudest.

**Proposed fix:**
1. **Coordinate with course author first** — many of these (`S2068` hardcoded credential, `S2699` test-without-assertion, `S1075` hardcoded URI) might be deliberate course examples.
2. For ones safe to suppress in bulk: add a `Directory.Build.props` under `system-test/dotnet/` with `<NoWarn>S4136;S1939;S1118</NoWarn>` (cosmetic-only rules in test code).
3. For S1075 (hardcoded URIs) — pull them into a `TestSettings` class or `appsettings.Test.json` if they're truly configurable, or suppress with an inline `#pragma` if they're documentation-as-test.
4. Do **not** bulk-suppress S2699 (test missing assertion) or S2068 (hardcoded credential) — review each one.

**Risk:** **Low**, but **High pedagogical risk** of removing teaching content. Always review with the course author. The fix is 100% scoped to test infrastructure.

---

## W8 — Sonar smells in .NET `multitier-backend` commit stage

**Symptom**
```
warning S1075: URIs should not be hardcoded         (12 occurrences)
warning S3267: Loop should be simplified to LINQ    (2)
warning S2699: Tests should include assertions      (2)
warning S2068: Hard-coded credentials               (2)
warning S1118: Add 'private' constructor            (2)
```

**Affected workflows (1):** `multitier-backend-dotnet-commit-stage` (20 Sonar warnings, separate from the system-test set above — these are in production / domain code in `multitier/dotnet/backend-dotnet/`).

**Root cause:** Production-code Sonar findings in the multitier backend C# project. Unlike W7 (system-test only), these touch real backend code. Most likely deliberate teaching examples for the course module on Sonar / static analysis.

**Proposed fix:** Same coordinate-with-author flow as W7. If kept, document them in the README so students know they're examples, not regressions.

**Risk:** **Pedagogical** — almost certainly intentional. **Low** technical risk. **Do not silently fix** — require explicit author sign-off.

---

## W9 — Cosmetic GHA warnings inside `cross-lang-system-verification` & `meta-prerelease-stage`

**Symptom**
The two umbrella workflows show warning counts that are the **sum of their child workflows' warnings** (Node 20: 12+19, Sonar S+CS: 464+464, Gradle: 156+156, Playwright: 488+488). Fixing W1–W8 above eliminates these too.

**Affected workflows (2):** `cross-lang-system-verification` (476 GHA warnings, but all inherited), `meta-prerelease-stage` (476 + a higher Node-20 count because it spawns six pipelines).

**Root cause:** Reusable-workflow output aggregation. No new warnings here.

**Proposed fix:** None — fixing W1–W8 cascades. Leave as is.

**Risk:** **Low.** Tracking only.

---

## Priority order (recommended)

| Priority | Warning | Why |
|---|---|---|
| **P0** | W3 (npm vulnerabilities — high severity in template) | Course material; students copy it; 6 high-sev CVEs propagate |
| **P0** | W1 (Wandalen/wretry on Node 20) | Hard runner deadline 2 June 2026 (~5 weeks) |
| **P1** | W2 (npm deprecated transitive deps) | Cheap reliability win; overlaps W3 |
| **P1** | W5 (Playwright `install-deps` on Java) | One-line fix; clears 488 banner lines from meta logs |
| **P2** | W4 (Gradle 9 deprecation) | Future-proofing; needs `--warning-mode all` triage first |
| **P2** | W6 (C# CS8604 nullable in DSL) | Low risk fix; review with author first |
| **P3** | W7 (Sonar smells in `system-test/dotnet`) | Bulk-cosmetic; needs author sign-off |
| **P3** | W8 (Sonar smells in `multitier-backend-dotnet`) | Likely intentional teaching content |
| **P4** | W9 (umbrella aggregation) | Tracking only — clears when W1–W8 close |

---

## Cross-cutting risk

**Source of warnings.** W1 (Node 20) and W4 (Gradle) come from external action / plugin upstreams; the shop repo can only react. W2/W3 (npm) come from transitive deps shop owns via `package-lock.json`. W5 (Playwright) is a missing step in shop's own composite. W6/W7/W8 (C#/Sonar) are in shop's own source tree (`system-test/dotnet/` for W6/W7; `multitier/dotnet/backend-dotnet/` for W8). The shared `optivem/actions` repo is **not** the source of any warning audited here — every category lives in shop.

**Pedagogical impact.** This repo is course material; the dotnet system-test code (W6, W7) and the multitier backend Sonar findings (W8) very likely include intentional teaching smells (hardcoded credentials, test-without-assertion, hardcoded URIs are exactly the patterns the course warns against). **Always review W6/W7/W8 fixes with the course author before bulk-applying** — silently scrubbing these would remove deliberate examples.

**Failure cluster.** All five `qa-signoff` workflows and the six dependent `prerelease-pipeline-*` workflows fail with the same `gh: You must authenticate via a GitHub App. (HTTP 403)` in `Record QA Signoff Check Run`. This is a single PAT/scope problem, not a warning, and is out of scope for this audit — but should be addressed because it currently blocks every prerelease.

**One audit fixes 1500+ warnings.** Closing W1, W2, W3, W5 alone removes ~80 % of the warning volume across the audit. W4 + W6 close the remaining noisy categories. W7/W8 are pedagogical and deferred.
