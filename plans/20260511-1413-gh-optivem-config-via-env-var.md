# Plan — Migrate workflows to `GH_OPTIVEM_CONFIG`, drop `--system-config` / `--test-config` flags

**Date:** 2026-05-11
**Scope:** spans `gh-optivem` (CLI schema patch — DONE) and shop (workflows + docs).

## Background

Today every `gh optivem run|build|test|stop|clean system` invocation in the workflows passes both `--system-config docker/<lang>/<arch>/systems.yaml` and `--test-config system-test/<lang>/tests.yaml`. That's ~227 flag pairs across 14 workflow files. The latest/legacy split is encoded by which `tests.*.yaml` the flag points at — there is no single variant identifier.

Meanwhile the repo root has 6 variant yamls (`gh-optivem-<arch>-<lang>.yaml`) consumed only by `gh optivem compile` (via `compile-all.sh`). They describe each variant's system + system-test paths but did not carry the docker or tests config paths.

## CLI patch (DONE — 2026-05-11)

Before today: the CLI's `projectconfig.Config` schema had `SystemConfig` and `TestConfig` at **top level** as `system_config:` / `test_config:`. Logically they belong nested under `system:` / `system_test:` (they're properties of those blocks, not cross-cutting).

Patched the CLI to nest them: `system.config:` and `system_test.config:`. Old top-level keys now produce a clear migration error at Load time so silent fallback to `./systems.yaml` defaults is impossible.

Files changed in `gh-optivem`:
- `internal/projectconfig/config.go` — moved `Config` field from `Config` struct → `System` struct; added `Config` field to `TierSpec` (only meaningful on `system_test`); added Validate rules to reject `system.backend.config` / `system.frontend.config` and to reject the legacy top-level keys with a migration hint.
- `internal/projectconfig/config_test.go` — updated round-trip test; added `TestValidate_RejectsLegacyTopLevelConfigKeys` and `TestValidate_RejectsConfigOnBackendOrFrontend`.
- `internal/steps/optivem_yaml.go` — scaffolder now writes `pc.System.Config` / `pc.SystemTest.Config` (was `pc.SystemConfig` / `pc.TestConfig`).
- `runner_commands.go` — `resolveSystemPath` / `resolveTestsPath` and `loadSystem` / `loadTests` hints updated to the nested spelling.
- `runner_commands_test.go` — `writeYAMLConfig` helper now emits nested yaml; hint-check strings updated.

`go build ./...` and `go test ./...` both green.

## Shop yamls (DONE earlier today, no rework needed)

The 12 variant yamls created earlier (`gh-optivem-<arch>-<lang>[-legacy].yaml`) already use the nested form (`system.config:` / `system_test.config:`). They validate cleanly against the patched CLI:

```
gh optivem config validate -c gh-optivem-monolith-dotnet.yaml
→ gh-optivem-monolith-dotnet.yaml is valid
```

Confirmed across all 12 files. No edits to shop yamls required.

## Plan items remaining

### Phase 1 — smoke test (DONE — 2026-05-11)

Verified `$env:GH_OPTIVEM_CONFIG` end-to-end against `gh-optivem-monolith-dotnet.yaml` and its `-legacy` sibling. All 11 latest sample suites and all 26 legacy sample suites passed. Schema + env-var wiring confirmed.

**Note:** the `gh` CLI used in workflows must be rebuilt with the patched gh-optivem. If the workflows pull a tagged release, bump the gh-optivem version after committing the CLI patch.

### Phase 2 — migrate workflows (DONE — 2026-05-11)

**2.1** Set `GH_OPTIVEM_CONFIG` once per workflow (at job level), drop both flags from every `gh optivem ... system` line.

Pattern:
```yaml
jobs:
  acceptance:
    runs-on: ubuntu-latest
    env:
      GH_OPTIVEM_CONFIG: gh-optivem-monolith-dotnet.yaml      # or -legacy
    steps:
      - run: gh optivem test system --suite smoke-stub --no-build --no-start
      - run: gh optivem test system --suite smoke-real --no-setup --no-build --no-start
      ...
```

**2.2** Files to update (14 total — ~227 flag-pair removals):

| Workflow | Variant yaml | Flag-pair removals |
|---|---|---|
| `monolith-dotnet-acceptance-stage.yml` | `gh-optivem-monolith-dotnet.yaml` | 11 |
| `monolith-dotnet-acceptance-stage-legacy.yml` | `gh-optivem-monolith-dotnet-legacy.yaml` | 26 |
| `monolith-java-acceptance-stage.yml` | `gh-optivem-monolith-java.yaml` | 11 |
| `monolith-java-acceptance-stage-legacy.yml` | `gh-optivem-monolith-java-legacy.yaml` | 26 |
| `monolith-typescript-acceptance-stage.yml` | `gh-optivem-monolith-typescript.yaml` | 12 |
| `monolith-typescript-acceptance-stage-legacy.yml` | `gh-optivem-monolith-typescript-legacy.yaml` | 26 |
| `multitier-dotnet-acceptance-stage.yml` | `gh-optivem-multitier-dotnet.yaml` | 11 |
| `multitier-dotnet-acceptance-stage-legacy.yml` | `gh-optivem-multitier-dotnet-legacy.yaml` | 26 |
| `multitier-java-acceptance-stage.yml` | `gh-optivem-multitier-java.yaml` | 11 |
| `multitier-java-acceptance-stage-legacy.yml` | `gh-optivem-multitier-java-legacy.yaml` | 26 |
| `multitier-typescript-acceptance-stage.yml` | `gh-optivem-multitier-typescript.yaml` | 11 |
| `multitier-typescript-acceptance-stage-legacy.yml` | `gh-optivem-multitier-typescript-legacy.yaml` | 26 |
| `cross-lang-system-verification.yml` | varies — needs case-by-case look | 2 |
| `_prerelease-pipeline.yml` | varies — needs case-by-case look | 3 |

**2.3** For each workflow, verify the `runs-on`/job topology fits a single env var (i.e. one workflow → one variant). If a workflow runs multiple variants in a matrix, the env var goes at the matrix step level instead of the job level.

**Outcome:**
- 12 single-variant acceptance-stage workflows: GH_OPTIVEM_CONFIG set at the run-job env block, both flag pairs stripped from every per-suite step.
- `_prerelease-pipeline.yml` (`local` job): job-level env var pinned to the `<arch>-<lang>` variant; the legacy-sample step overrides at step level to the `-legacy` sibling.
- `cross-lang-system-verification.yml`: **left flag-driven by design.** Each matrix entry independently mixes `system-lang` (for `--system-config`) and `test-lang` (for `--test-config`). Migrating to GH_OPTIVEM_CONFIG would require either 12 new cross-lang variant yamls (one per non-same-lang combination) or materializing a yaml per matrix entry at runtime — both more code than the explicit flag form, which also reads more clearly because the two language axes are visible on the same line.

### Phase 3 — docs cleanup (DONE — 2026-05-11)

**3.1** Update `CLAUDE.md` "System Test Verification" block — replace the three example commands with the env-var form. Current text:

```bash
gh optivem run system --system-config docker/<language>/monolith/systems.yaml
gh optivem test system --system-config docker/<language>/monolith/systems.yaml --test-config system-test/<language>/tests.yaml --sample
gh optivem stop system --system-config docker/<language>/monolith/systems.yaml
```

New text (PowerShell idiom since shop is Windows-first):

```pwsh
$env:GH_OPTIVEM_CONFIG = "gh-optivem-monolith-<language>.yaml"
gh optivem run  system
gh optivem test system --sample
gh optivem stop system
```

**3.2** Check `docs/operations/running-system-tests.md` and `test-all.sh` for the same flag usage. Migrate to the env var pattern there too.

Both updated. `test-all.sh` now resolves `gh-optivem-<arch>-<lang>[-legacy].yaml` per phase and exports `GH_OPTIVEM_CONFIG` ahead of each `gh optivem ... system` call.

### Phase 4 — compile-all.sh decision (DONE — 2026-05-11)

`compile-all.sh` globs `gh-optivem-*.yaml`, which now picks up 12 files instead of 6 — and each `*-legacy.yaml` compiles the same code as its latest sibling. Options:

- **A.** Leave as-is. Cost: ~50% extra compile time, but the summary table has a row per yaml so you can see all 12 results.
- **B.** Exclude `*-legacy.yaml` from the glob. Cheapest fix, no information loss because the latest yaml already covers each project.
- **C.** Dedupe by hashing `system` fields. Most "correct" but over-engineered for one duplicate per variant.

**Recommendation:** B. The legacy yamls exist to select a test-config, not to change the system code. The compile step is system-only — duplicating it is pure waste.

**Applied:** option B. `compile-all.sh` glob narrowed to `gh-optivem-!(*-legacy).yaml` (extglob). Verified: 6 variants compile clean (~1:17 total).

## Validation

After all phases:
1. Run `./compile-all.sh` from repo root → all 6 variants compile clean (or 12 if option A chosen).
2. Locally trigger one latest + one legacy acceptance workflow path via `gh optivem` smoke commands (Phase 1.2 pattern). Confirm both pick up the right tests config.
3. Push to a branch, watch CI for one latest + one legacy acceptance-stage workflow. Confirm green.
4. Once one pair passes, the other 10 workflows are mechanical follow-ups (same edit pattern, different variant).

## Risk

- **Low** — the CLI already supports this end-to-end; no new code paths exercised.
- The flag-removal in workflows is mechanical (sed-able). Easy to revert per-workflow if any specific job has a hidden dependency on the flag form.
- `--system-config` / `--test-config` stay supported by the CLI, so this migration is additive — old workflows / docs that miss the migration keep working.

## Open question for the user (RESOLVED — 2026-05-11)

- `_prerelease-pipeline.yml` migrated: it's single-variant per invocation, so it fits the env-var model cleanly.
- `cross-lang-system-verification.yml` left flag-driven: matrix dimensions independently select `system-lang` and `test-lang`, so the flag form is the right tool. See Phase 2 outcome note.
