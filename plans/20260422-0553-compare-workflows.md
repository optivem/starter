# 20260422-0553 — Workflow Diff Plan

> 🤖 **Picked up by agent** — `ValentinaLaptop` at `2026-04-22T06:14:59Z`

Architecture: both
Stage: all

## Remaining items (awaiting author decision)

### DIFF-4: commit-stage — `Read Target Component Version` placed before setup/build in multitier, but after Sonar in monolith

**Stage:** commit-stage
**Scope:** monolith vs multitier — all languages

**Files:**
- `.github/workflows/monolith-java-commit-stage.yml:106` — `Read Target Component Version` is step #11 (after `Run Code Analysis`, before `Build Artifact`)
- `.github/workflows/monolith-dotnet-commit-stage.yml:109` — same late position
- `.github/workflows/monolith-typescript-commit-stage.yml:112` — same late position
- `.github/workflows/multitier-backend-java-commit-stage.yml:72` — `Read Target Component Version` is step #3 (right after `Verify Built SHA Is On Main`, before `Setup Java and Gradle`)
- `.github/workflows/multitier-backend-dotnet-commit-stage.yml:72` — same early position
- `.github/workflows/multitier-backend-typescript-commit-stage.yml:72` — same early position
- `.github/workflows/multitier-frontend-react-commit-stage.yml:72` — same early position

**Details:**
Each architecture is internally consistent across its language triplet, but the two architectures disagree: multitier reads the target version at the very top of the `run` job so it can be exposed via `component-version` as a job output; monolith reads it only just before packaging. Both architectures also differ in job outputs:
- monolith `run` → `image-digest-url`
- multitier `run` → `component-version`, `image-version-url`

**Recommendation:**
Align monolith to the multitier shape: move the monolith `Read Target Component Version` step up to immediately after `Verify Built SHA Is On Main`, and add matching `component-version` / `image-version-url` outputs on the `run` job so downstream `pipeline-*` drivers can consume a uniform contract.

VJ: where is the version used? what's the proposal for standardization?

**🤖 Answer (from grep across `.github/workflows/`):**

- **Where `run` outputs are consumed today:**
  - `monolith-*-commit-stage.yml` → `needs.run.outputs.image-digest-url` → **only** used by the `summary` step in the same workflow (e.g. [monolith-dotnet-commit-stage.yml:168](../.github/workflows/monolith-dotnet-commit-stage.yml#L168)).
  - `multitier-*-commit-stage.yml` → `needs.run.outputs.image-version-url` → **only** used by the `summary` step in the same workflow.
  - `component-version` (multitier only) is **not consumed anywhere** — no `prerelease-pipeline-*.yml` reads it, no other stage reads it. Declared but unused.
- **Nothing downstream** (prerelease-pipeline drivers, qa, prod) consumes these commit-stage `run` outputs across workflows. They're purely cosmetic for the summary.
- **Both architectures already compute `image-version-url` the same way** internally: `fromJSON(steps.tag.outputs.image-urls)[2]` (see `monolith-dotnet-commit-stage.yml:154` vs `multitier-backend-dotnet-commit-stage.yml:154`). Monolith just doesn't expose it as a job output.

**Proposal (3 options):**

| | Change | Consumer impact | Effort |
|---|---|---|---|
| **A (recommended)** | Swap monolith's `run` output from `image-digest-url` → `image-version-url` (already computed at `steps.tag.outputs.image-urls[2]`, just re-wire the output + update the summary string). Leave step-ordering as-is. | None (only summary text changes) | ~6 lines × 3 files |
| **B** | Option A + also move `Read Target Component Version` to the top and expose `component-version` on monolith's `run`, matching multitier shape exactly. | None (no consumers yet) | ~15 lines × 3 files, + reorder |
| **C** | Do nothing — leave them different. | None | 0 |

**Recommended: A.** It fixes the user-visible inconsistency (the summary URL) with almost no code change and no downstream blast radius. B aligns the shape for future consumers but adds cost today with no consumer to justify it — revisit if/when a cross-workflow consumer appears. C is fine if you don't care about summary consistency.

**VJ decision:** ⬜ A   ⬜ B   ⬜ C

---

### DIFF-5: commit-stage — summary uses `image-digest-url` in monolith but `image-version-url` in multitier

**Stage:** commit-stage
**Scope:** monolith vs multitier — all languages

**Files:**
- `.github/workflows/monolith-java-commit-stage.yml:165` — `` `${{ needs.run.outputs.image-digest-url }}` ``
- `.github/workflows/monolith-dotnet-commit-stage.yml:168` — same
- `.github/workflows/monolith-typescript-commit-stage.yml:171` — same
- `.github/workflows/multitier-backend-java-commit-stage.yml:165` — `` `${{ needs.run.outputs.image-version-url }}` ``
- `.github/workflows/multitier-backend-dotnet-commit-stage.yml:168` — same
- `.github/workflows/multitier-backend-typescript-commit-stage.yml:171` — same
- `.github/workflows/multitier-frontend-react-commit-stage.yml:171` — same

**Details:**
The summary step renders a different artifact URL depending on architecture. Within each architecture all three languages agree, so this is not a triplet issue — but it is a cross-architecture inconsistency in what the stage summary reports.

**Recommendation:**
Pair with DIFF-4. Once monolith exposes `image-version-url` as a job output, standardize the summary to use `image-version-url` in both architectures (the versioned URL is more human-useful than the digest URL for a commit-stage artifact).

VJ: Are they using the same actions? why this difference?

**🤖 Answer:**

- **Yes, they use the same actions.** Both monolith and multitier commit-stages use `optivem/actions/tag-docker-image@v1` (or equivalent), producing a `steps.tag.outputs.image-urls` JSON array with three entries: `[sha-url, latest-url, version-url]`. Both compute `fromJSON(steps.tag.outputs.image-urls)[2]` = the version URL.
- **Why the difference then?** Accidental divergence, not by design. The summary-URL choice was made independently when each architecture's commit-stage was first written. Monolith's `run` job exposes `image-digest-url` as an output (computed from `steps.push.outputs.image-digest-url`), while multitier's exposes `image-version-url` (computed from the tag step). Both are valid URLs to the same image, just different tag formats.
- **Resolution:** Folded into DIFF-4 option A — swap monolith's summary to use `image-version-url` and this goes away.

**VJ decision:** ⬜ Folded into DIFF-4 (resolves automatically with A or B)   ⬜ Keep as-is

---

### DIFF-7: acceptance-stage — per-language test-project compile step is Java-only and .NET-only (TypeScript lacks it)

**Stage:** acceptance-stage
**Scope:** monolith and multitier — Java vs .NET vs TypeScript

**Files (monolith):**
- `.github/workflows/monolith-java-acceptance-stage.yml:179-181` — `Compile System Tests` → `./gradlew clean compileJava compileTestJava`
- `.github/workflows/monolith-dotnet-acceptance-stage.yml:169-171` — `Build Test Project` → `dotnet build`
- `.github/workflows/monolith-typescript-acceptance-stage.yml` — no equivalent step (Playwright compiles on demand)

**Files (multitier):**
- `.github/workflows/multitier-java-acceptance-stage.yml:180-182` — `Compile System Tests` → `./gradlew clean compileJava compileTestJava`
- `.github/workflows/multitier-dotnet-acceptance-stage.yml:170-172` — `Build Test Project` → `dotnet build`
- `.github/workflows/multitier-typescript-acceptance-stage.yml` — no equivalent step

**Details:**
Java has a "Compile System Tests" step, .NET has a "Build Test Project" step, TypeScript has no up-front compile step. Mix of step-name mismatch (Java vs .NET) and missing-step (TypeScript).

**Recommendation:**
- Rename .NET step to `Compile System Tests` in `monolith-dotnet-acceptance-stage.yml:169`, `multitier-dotnet-acceptance-stage.yml:170`, and all instances in the `*-dotnet-acceptance-stage-cloud.yml` variants (see DIFF-9).
- Add a TypeScript `Compile System Tests` step in `monolith-typescript-acceptance-stage.yml` and `multitier-typescript-acceptance-stage.yml` (insert right after `Install Playwright System Dependencies`) running `npx tsc --noEmit` — fails fast without producing output.

VJ: Can we have separate compilation for typescript, I think something with word emit
(→ confirms `tsc --noEmit` approach)

**🤖 Answer:**

Yes — `npx tsc --noEmit` type-checks the TypeScript sources without producing `.js` output. That's the word you were thinking of. It catches compile errors up-front, before Playwright tries to run tests, and mirrors what the Java `compileTestJava` and .NET `dotnet build` steps do for their siblings.

**Concrete proposal for DIFF-7 & DIFF-8 combined:**

1. **Rename .NET step** `Build Test Project` → `Compile System Tests` in these 6 files:
   - `.github/workflows/monolith-dotnet-acceptance-stage.yml:169`
   - `.github/workflows/multitier-dotnet-acceptance-stage.yml:170`
   - `.github/workflows/monolith-dotnet-acceptance-stage-legacy.yml:153`
   - `.github/workflows/multitier-dotnet-acceptance-stage-legacy.yml:154`
   - `.github/workflows/monolith-dotnet-acceptance-stage-cloud.yml` (all 11 occurrences)
   - `.github/workflows/multitier-dotnet-acceptance-stage-cloud.yml` (all 11 occurrences)

2. **Add TypeScript `Compile System Tests` step** running `npx tsc --noEmit` in `system-test/typescript`, inserted right after `Install Playwright System Dependencies`. Apply to:
   - `.github/workflows/monolith-typescript-acceptance-stage.yml`
   - `.github/workflows/multitier-typescript-acceptance-stage.yml`
   - `.github/workflows/monolith-typescript-acceptance-stage-legacy.yml`
   - `.github/workflows/multitier-typescript-acceptance-stage-legacy.yml`
   - Cloud variants only if they follow the same compile pattern (verify first — see DIFF-9 concern about whether .NET cloud compile-in-every-job is structural).

**VJ decision:** ⬜ Do it (covers DIFF-7 + DIFF-8)   ⬜ Rename .NET only (skip TS compile)   ⬜ Add TS compile only (skip rename)   ⬜ Skip both

---

### DIFF-8: acceptance-stage-legacy — same `Compile System Tests` / `Build Test Project` / missing pattern as DIFF-7

**Stage:** acceptance-stage-legacy
**Scope:** monolith and multitier — Java vs .NET vs TypeScript

**Files (monolith):**
- `.github/workflows/monolith-java-acceptance-stage-legacy.yml:163-165` — `Compile System Tests` present
- `.github/workflows/monolith-dotnet-acceptance-stage-legacy.yml:153-155` — `Build Test Project` present
- `.github/workflows/monolith-typescript-acceptance-stage-legacy.yml` — no equivalent step

**Files (multitier):**
- `.github/workflows/multitier-java-acceptance-stage-legacy.yml:163-165` — `Compile System Tests` present
- `.github/workflows/multitier-dotnet-acceptance-stage-legacy.yml:153-155` — `Build Test Project` present
- `.github/workflows/multitier-typescript-acceptance-stage-legacy.yml` — no equivalent step

**Details:**
Identical shape to DIFF-7 but in the legacy variant.

**Recommendation:**
Apply the same fixes as DIFF-7 to the four legacy workflow files.

VJ: Let's call it Compile System Tests... also in TypeScript, can we add compilation? should we?
(→ VJ already approved the `Compile System Tests` rename; TypeScript compile question resolved via DIFF-7 → `tsc --noEmit`)

---

### DIFF-13: acceptance-stage — `CHANNEL` env value case differs between Java/.NET (uppercase) and TypeScript (lowercase)

**Stage:** acceptance-stage, acceptance-stage-legacy, acceptance-stage-cloud
**Scope:** monolith and multitier — Java/.NET vs TypeScript

**Files:**
- `.github/workflows/monolith-java-acceptance-stage.yml` — uses `-Dchannel=API` and `-Dchannel=UI` (uppercase)
- `.github/workflows/monolith-dotnet-acceptance-stage.yml` — uses `CHANNEL: API` and `CHANNEL: UI` (uppercase)
- `.github/workflows/monolith-typescript-acceptance-stage.yml` — uses `CHANNEL: api` and `CHANNEL: ui` (lowercase)
- Same pattern across `multitier-*-acceptance-stage.yml`, `*-acceptance-stage-legacy.yml`, `*-acceptance-stage-cloud.yml`.

**Details:**
`CHANNEL` is `API`/`UI` in Java and .NET but `api`/`ui` in TypeScript. If any shared tooling or log grep relies on case, it behaves differently per language.

**Recommendation:**
Standardize to uppercase `API`/`UI` across TypeScript workflows (consistent with Java/.NET and with common HTTP convention). Alternate: normalize case inside the TypeScript test harness.

---

### DIFF-14: acceptance-stage-cloud — Java monolith lacks explicit `externalSystemMode` where .NET/TS set `EXTERNAL_SYSTEM_MODE: stub`

**Stage:** acceptance-stage-cloud
**Scope:** monolith — Java vs .NET vs TypeScript

**Files:**
- `.github/workflows/monolith-java-acceptance-stage-cloud.yml:250` — `Run Acceptance Tests - API Channel` → includes `-Dchannel=API`, no `EXTERNAL_SYSTEM_MODE`
- `.github/workflows/monolith-dotnet-acceptance-stage-cloud.yml:293-297` — env block includes `EXTERNAL_SYSTEM_MODE: stub` and `CHANNEL: API`
- `.github/workflows/monolith-typescript-acceptance-stage-cloud.yml:294-299` — env block includes `EXTERNAL_SYSTEM_MODE: stub` and `CHANNEL: api`

**Details:**
Java's test harness may be inferring stub-mode from another signal or defaulting; .NET/TS set it explicitly.

**Recommendation:**
Add explicit `-DexternalSystemMode=stub` in `monolith-java-acceptance-stage-cloud.yml` for the acceptance-* jobs so intent is visible in the workflow, matching .NET/TS.
