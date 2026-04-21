# Workflow Diff Report

Cross-language comparison of GitHub Actions workflows (Java vs .NET vs TypeScript).

Generated: 2026-04-08

## Legend

- **OK** — expected language difference (build commands, ports, runtime setup)
- **DIFF** — inconsistency that should be investigated/resolved

---

## 1. Monolith — Commit Stage

Files: `monolith-java-commit-stage.yml`, `monolith-dotnet-commit-stage.yml`, `monolith-typescript-commit-stage.yml`

### Job structure, triggers, permissions, outputs

All consistent.

### Step comparison — `run` job

| Step                          | Java                          | .NET                          | TypeScript                    | Status |
|-------------------------------|-------------------------------|-------------------------------|-------------------------------|--------|
| Checkout Repository           | `actions/checkout@v5`         | `actions/checkout@v5`         | `actions/checkout@v5`         | OK     |
| Setup Runtime                 | `setup-java-gradle` (21)      | `setup-dotnet` (8.0.x)        | `setup-node` (22.x)          | OK     |
| Compile Code                  | `./gradlew build`             | `dotnet build`                | `npm run build`               | OK     |
| Run Unit Tests                | `./gradlew test` (IMPL)       | `echo "TODO"` (TODO)          | `echo "TODO"` (TODO)          | **DIFF** |
| Run Narrow Integration Tests  | (missing)                     | `echo "TODO"` (TODO)          | `echo "TODO"` (TODO)          | **DIFF** |
| Run Component Tests           | (missing)                     | `echo "TODO"` (TODO)          | `echo "TODO"` (TODO)          | **DIFF** |
| Run Contract Tests            | (missing)                     | `echo "TODO"` (TODO)          | `echo "TODO"` (TODO)          | **DIFF** |
| Run Linter                    | `./gradlew checkstyleMain`    | `dotnet format ... --verify`  | `npm run lint`                | OK     |
| Run Code Analysis             | `./gradlew build sonar`       | `dotnet sonarscanner`         | `SonarSource/sonarcloud@v5`   | OK     |
| Read Target Version           | present                       | present                       | present                       | OK     |
| Build Artifact                | present                       | present                       | present                       | OK     |
| Push Artifact                 | present                       | present                       | present                       | OK     |

#### DIFF-1: Unit tests implemented only in Java

- **Java** `monolith-java-commit-stage.yml:71` — `./gradlew test` (real implementation)
- **.NET** `monolith-dotnet-commit-stage.yml:70-71` — `echo "TODO - not yet implemented"`
- **TypeScript** `monolith-typescript-commit-stage.yml:70-71` — `echo "TODO - not yet implemented"`

#### DIFF-2: Java missing test placeholder steps

Java has no "Run Narrow Integration Tests", "Run Component Tests", or "Run Contract Tests" steps at all. .NET and TypeScript have them as TODO placeholders.

---

## 2. Monolith — Acceptance Stage

Files: `monolith-java-acceptance-stage.yml`, `monolith-dotnet-acceptance-stage.yml`, `monolith-typescript-acceptance-stage.yml`

### Job structure, triggers, permissions

All consistent (schedule daily midnight + workflow_dispatch; jobs: preflight -> check -> run -> summary).

### Step comparison — `run` job

| Step                               | Java                                    | .NET                                    | TypeScript                              | Status |
|------------------------------------|-----------------------------------------|-----------------------------------------|-----------------------------------------|--------|
| Checkout Repository                | present                                 | present                                 | present                                 | OK     |
| Log in to Docker Hub               | present                                 | present                                 | present                                 | OK     |
| Deploy System (Real)               | present                                 | present                                 | present                                 | OK     |
| Deploy System (Stub)               | present                                 | present                                 | present                                 | OK     |
| Setup Runtime                      | `setup-java-gradle` (21)                | `setup-dotnet` (8.0.x)                  | `setup-node` (22.x)                     | OK     |
| Cache NuGet Packages               | (missing)                               | `actions/cache@v4`                      | (missing)                               | **DIFF** |
| Compile / Build Tests              | "Compile System Tests"                  | "Build Test Project"                    | (missing)                               | **DIFF** |
| Cache Playwright Browsers          | present                                 | present                                 | present                                 | OK     |
| Install Playwright Dependencies    | present                                 | present                                 | present                                 | OK     |
| Run Smoke Tests (Stub)             | present                                 | present                                 | present                                 | OK     |
| Run Smoke Tests (Real)             | present                                 | present                                 | present                                 | OK     |
| Run Acceptance Tests - API         | present                                 | present                                 | present                                 | OK     |
| Run Acceptance Tests - UI          | present                                 | present                                 | present                                 | OK     |
| Run Acceptance Tests (Isolated) API| present                                 | present                                 | present                                 | OK     |
| Run Acceptance Tests (Isolated) UI | present                                 | present                                 | present                                 | OK     |
| Run Contract Tests (Stub)          | present                                 | present                                 | present                                 | OK     |
| Run Contract Tests (Isolated) Stub | present                                 | present                                 | present                                 | OK     |
| Run Contract Tests (Real)          | present                                 | present                                 | present                                 | OK     |
| Run E2E Tests - API                | present                                 | present                                 | present                                 | OK     |
| Run E2E Tests - UI                 | present                                 | present                                 | present                                 | OK     |
| Read Target Version                | present                                 | present                                 | present                                 | OK     |
| Promote to RC                      | present                                 | present                                 | present                                 | OK     |

#### DIFF-3: .NET has extra "Cache NuGet Packages" step

- **.NET** `monolith-dotnet-acceptance-stage.yml:132-137` — caches `~/.nuget/packages`
- **Java** — no explicit dependency cache step (may be handled by `setup-java-gradle`)
- **TypeScript** — no explicit dependency cache step

#### DIFF-4: Test compilation step name mismatch and missing in TypeScript

- **Java** `monolith-java-acceptance-stage.yml:139` — step named "Compile System Tests" (`./gradlew clean compileJava compileTestJava`)
- **.NET** `monolith-dotnet-acceptance-stage.yml:139` — step named "Build Test Project" (`dotnet build`)
- **TypeScript** `monolith-typescript-acceptance-stage.yml` — no equivalent step at all

---

## 3. Monolith — Acceptance Stage (Legacy)

Files: `monolith-java-acceptance-stage-legacy.yml`, `monolith-dotnet-acceptance-stage-legacy.yml`, `monolith-typescript-acceptance-stage-legacy.yml`

### Same issues as acceptance-stage

DIFF-3 and DIFF-4 apply identically to the legacy variants.

### Additional issue

#### DIFF-5: .NET legacy contract tests use different filtering approach

For mod11 contract tests:

- **Java** `monolith-java-acceptance-stage-legacy.yml:227`:
  ```
  ./gradlew test -Dversion=mod11 -Dtype=contract -DexcludeTags=isolated -DexternalSystemMode=stub -Denvironment=acceptance
  ```
  Passes `externalSystemMode` as a command-line flag.

- **.NET** `monolith-dotnet-acceptance-stage-legacy.yml:298`:
  ```
  dotnet test --filter "FullyQualifiedName~.Legacy.Mod11.ExternalSystemContractTests&FullyQualifiedName~Stub&Category!=isolated"
  ```
  Filters by test **name** (`FullyQualifiedName~Stub`) instead of setting `EXTERNAL_SYSTEM_MODE` env var. No `EXTERNAL_SYSTEM_MODE` in env block.

- **TypeScript** `monolith-typescript-acceptance-stage-legacy.yml:288`:
  ```
  npm test -- test/legacy/mod11/contract --testPathIgnorePatterns=isolated
  ```
  With `EXTERNAL_SYSTEM_MODE: stub` in env block — consistent with Java's approach.

---

## 4. Monolith — QA Stage

Files: `monolith-java-qa-stage.yml`, `monolith-dotnet-qa-stage.yml`, `monolith-typescript-qa-stage.yml`

No inconsistencies. All consistent across languages.

---

## 5. Monolith — QA Signoff

Files: `monolith-java-qa-signoff.yml`, `monolith-dotnet-qa-signoff.yml`, `monolith-typescript-qa-signoff.yml`

No inconsistencies. All consistent across languages.

---

## 6. Monolith — Prod Stage

Files: `monolith-java-prod-stage.yml`, `monolith-dotnet-prod-stage.yml`, `monolith-typescript-prod-stage.yml`

No inconsistencies. All consistent across languages.

---

## 7. Monolith — Verify

Files: `verify-monolith-java.yml`, `verify-monolith-dotnet.yml`, `verify-monolith-typescript.yml`

No inconsistencies. All consistent across languages.

---

## 8. Multitier — Commit Stage (Backend)

Files: `multitier-backend-java-commit-stage.yml`, `multitier-backend-dotnet-commit-stage.yml`, `multitier-backend-typescript-commit-stage.yml`

### Step comparison — `run` job

| Step                          | Java                          | .NET                          | TypeScript                    | Status |
|-------------------------------|-------------------------------|-------------------------------|-------------------------------|--------|
| Checkout Repository           | present                       | present                       | present                       | OK     |
| Read Component Version        | present                       | present                       | present                       | OK     |
| Setup Runtime                 | `setup-java-gradle` (21)      | `setup-dotnet` (8.0.x)        | `setup-node` (22.x)          | OK     |
| Compile Code                  | `./gradlew build`             | `dotnet build`                | `npm run build`               | OK     |
| Run Unit Tests                | `echo "TODO"` (TODO)          | `echo "TODO"` (TODO)          | `npm test` (IMPL)             | **DIFF** |
| Run Narrow Integration Tests  | `echo "TODO"` (TODO)          | `echo "TODO"` (TODO)          | `echo "TODO"` (TODO)          | OK     |
| Run Component Tests           | `echo "TODO"` (TODO)          | `echo "TODO"` (TODO)          | `echo "TODO"` (TODO)          | OK     |
| Run Contract Tests            | `echo "TODO"` (TODO)          | `echo "TODO"` (TODO)          | `echo "TODO"` (TODO)          | OK     |
| Run Linter                    | `./gradlew checkstyleMain`    | `dotnet format ... --verify`  | `npm run lint`                | OK     |
| Run Code Analysis             | `./gradlew build sonar`       | `dotnet sonarscanner`         | `SonarSource/sonarcloud@v5`   | OK     |
| Build Artifact                | present                       | present                       | present                       | OK     |
| Push Artifact                 | present                       | present                       | present                       | OK     |

#### DIFF-6: Unit tests implemented only in TypeScript

- **TypeScript** `multitier-backend-typescript-commit-stage.yml:77-79` — `npm test` (real implementation)
- **Java** `multitier-backend-java-commit-stage.yml:79` — `echo "TODO - not yet implemented"`
- **.NET** `multitier-backend-dotnet-commit-stage.yml:78` — `echo "TODO - not yet implemented"`

---

## 9. Multitier — Commit Stage (Frontend)

File: `multitier-frontend-react-commit-stage.yml`

No cross-language comparison (only React). All test steps are TODO placeholders, matching the Java/.NET backend pattern.

---

## 10. Multitier — Acceptance Stage

Files: `multitier-java-acceptance-stage.yml`, `multitier-dotnet-acceptance-stage.yml`, `multitier-typescript-acceptance-stage.yml`

Same issues as monolith acceptance stage: DIFF-3 (extra NuGet cache in .NET), DIFF-4 (step name mismatch / missing build in TypeScript).

---

## 11. Multitier — Acceptance Stage (Legacy)

Files: `multitier-java-acceptance-stage-legacy.yml`, `multitier-dotnet-acceptance-stage-legacy.yml`, `multitier-typescript-acceptance-stage-legacy.yml`

Same issues as monolith: DIFF-3, DIFF-4, DIFF-5.

---

## 12. Multitier — QA Stage, QA Signoff, Prod Stage, Verify

All consistent across languages. No inconsistencies.

---

## 13. Cross-Architecture Inconsistency (Monolith vs Multitier)

#### DIFF-8: Commit stage outputs differ between architectures

- **Monolith** `run` job outputs:
  ```yaml
  image-digest-url: ${{ steps.push.outputs.image-digest-url }}
  ```
  Summary references: `success-artifact-url: ${{ needs.run.outputs.image-digest-url }}`

- **Multitier** `run` job outputs:
  ```yaml
  component-version: ${{ steps.read-version.outputs.target-version }}
  image-version-tag: ${{ steps.build.outputs.image-version-tag }}
  ```
  Summary references: `success-artifact-url: ${{ needs.run.outputs.image-version-tag }}`

#### DIFF-9: Version reading step placement and conditionality differ

- **Monolith**: "Read Target Version" is placed late (after Code Analysis), guarded by `if: github.ref == 'refs/heads/main'`
- **Multitier**: "Read Component Version" is placed early (step #2, right after Checkout), with no `if` condition

---

## Summary

| #      | Scope                          | Issue                                                                 | Affected Files |
|--------|--------------------------------|-----------------------------------------------------------------------|----------------|
| DIFF-1 | Monolith commit-stage          | Unit tests implemented in Java only; .NET and TypeScript are TODO     | 3 files |
| DIFF-2 | Monolith commit-stage          | Java missing placeholder steps for integration/component/contract tests | 1 file |
| DIFF-3 | All acceptance stages          | .NET has extra "Cache NuGet Packages" step; Java and TypeScript do not | 4 files |
| DIFF-4 | All acceptance stages          | Test build step: Java="Compile System Tests", .NET="Build Test Project", TypeScript=missing | 4 files |
| DIFF-5 | All acceptance-stage-legacy    | .NET mod11 contract tests filter by name instead of using EXTERNAL_SYSTEM_MODE env var | 2 files |
| DIFF-6 | Multitier backend commit-stage | Unit tests implemented in TypeScript only; Java and .NET are TODO     | 3 files |
| DIFF-8 | Cross-architecture             | Monolith outputs `image-digest-url`; multitier outputs `image-version-tag` | 6 files |
| DIFF-9 | Cross-architecture             | Version reading step placement and conditionality differ               | 6 files |

**Total: 8 distinct inconsistencies**
