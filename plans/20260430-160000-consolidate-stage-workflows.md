# Consolidate per-(arch, lang) stage workflows into reusable workflows

## Decision

Replace each family of 6 near-duplicate stage workflows (3 languages × 2 architectures) with **one reusable workflow + 6 thin caller files**. Apply this to every stage family that is currently duplicated 6×: acceptance-stage, acceptance-stage-cloud, acceptance-stage-legacy, qa-stage, qa-stage-cloud, prod-stage, prod-stage-cloud, qa-signoff (8 families × 6 ≈ 48 files → 8 reusables + 48 thin callers, plus ~2 composite actions for runtime setup).

The pattern is the one shop already uses for `_prerelease-pipeline.yml`: a single `_<stage>.yml` reusable takes `architecture`, `language`, `prefix` (and a few other) inputs, with per-language steps gated by `if: inputs.language == ...`.

## Rationale

### Genuine vs incidental divergence

Comparing `monolith-{java,dotnet,typescript}-acceptance-stage.yml`:

| Difference | Type | Lines |
|---|---|---|
| Concurrency group, env name, image name, paths, ports, tag prefix | Incidental — pure substitution | ~80% of diff |
| Runtime setup (Java+Gradle vs .NET+NuGet vs Node+npm) | Genuine — only language-specific block | ~25–35 lines |
| Suite step list (`smoke-stub`, `acceptance-api`, …) | Should be identical, in practice drifts | ~30 lines |

Comparing `monolith-java-…` vs `multitier-java-…`:

| Difference | Lines |
|---|---|
| Image count: 1 (`monolith-system-java`) vs 2 (`multitier-frontend-react` + `multitier-backend-java`) | ~4 |
| Compose `service-names`: `system` vs `frontend\nbackend` | ~6 |
| Working-directory: `docker/java/monolith` vs `docker/java/multitier` | substitution |
| Base-version path: `system/monolith/java/VERSION` vs `system/multitier/java/VERSION` | substitution |
| Tag prefix, env name, concurrency group | substitution |

**Runtime setup is identical between monolith and multitier of the same language** — system-tests live at `system-test/{lang}` regardless of architecture.

So the variation factors are orthogonal:

- Per-**language** (3): runtime setup block → composite action `setup-language-toolchain`
- Per-**architecture** (2): image-urls list, service-names list, working-directory, base-version path → reusable inputs
- Per-**(lang, arch)**: tag prefix, env name, ports → derived inside reusable from inputs

### Drift bug found while planning

`monolith-typescript-acceptance-stage.yml:310` calls the suite `contract-isolated-stub`, while `monolith-{java,dotnet}-acceptance-stage.yml` call the same suite `contract-stub-isolated`. This is exactly the kind of bug that 6-fold duplication produces. Consolidation eliminates the surface area.

### Existing precedent

Shop already uses this pattern: `_prerelease-pipeline.yml` (workflow_call, takes `architecture` + `language` + `prefix` inputs, `if:`-gated per-language steps). The proposal is to apply that same pattern to the stage-stage workflows, not invent a new convention.

### What we're NOT doing

- **Not** moving stage workflows to `.github/workflows/templates/` like the bump-patch-version scaffold-only files. Stage workflows are dual-role (active in shop CI + scaffold sources for student repos), so they must remain top-level.
- **Not** consolidating commit-stage workflows. They're already nearly-distinct per language (different build tools, different test runners), and there are 7 of them (3 monolith × 1 + 3 multitier-backend × 1 + 1 multitier-frontend), not 6 — different shape.
- **Not** consolidating bump-patch-version files in this plan. Those have their own scaffold-vs-active dual role and are tracked under `20260430-055950-move-scaffold-workflows-to-templates-subdir.md`.

## Tradeoff

**Cost**: a regression in `_acceptance-stage.yml` would break all 6 callers simultaneously instead of one. Today's drift is a benefit in disguise — bugs stay localized to one language. Mitigation: the reusable's logic is mostly identity-preserving boilerplate already; the per-language differences live in `setup-language-toolchain`, which can be unit-tested via the composite-action test harness in `optivem/actions`.

**Cost for student repos**: depends on the gh-optivem strategy chosen below. The naive "scaffold both files" approach gives students 2 files per stage (caller + reusable) instead of 1. The "scaffold-time inline" approach keeps students at 1 file per stage but adds tooling complexity to gh-optivem.

## Scope (files that change in shop)

### New files

- `.github/workflows/_acceptance-stage.yml` — reusable with inputs: `architecture`, `language`, `prefix`, `image-base-names` (multi-line list), `service-names` (multi-line list), `endpoint-base-port` (e.g. `3111`), plus the `commit-sha` / `debug-skip-tests` triggers passed through from callers.
- `.github/workflows/_acceptance-stage-cloud.yml` — same inputs, cloud-deploy variant.
- `.github/workflows/_acceptance-stage-legacy.yml` — same inputs, legacy sample variant.
- `.github/workflows/_qa-stage.yml`, `_qa-stage-cloud.yml`, `_prod-stage.yml`, `_prod-stage-cloud.yml`, `_qa-signoff.yml` — same pattern.
- `optivem/actions/setup-language-toolchain/` — composite action that switches on `language: java|dotnet|typescript` and installs the appropriate toolchain + Playwright system deps + caches. Replaces the per-language ~25–35-line setup block.
- `optivem/actions/install-gh-optivem/` — composite action for `gh extension install optivem/gh-optivem` (currently duplicated in every workflow).

### Files that get rewritten as thin callers (~20 lines each)

48 files, all matching `{arch}-{lang}-{stage}.yml`:

```
monolith-{dotnet,java,typescript}-{acceptance-stage,acceptance-stage-cloud,acceptance-stage-legacy,qa-stage,qa-stage-cloud,prod-stage,prod-stage-cloud,qa-signoff}.yml
multitier-{dotnet,java,typescript}-{… same 8 stages …}.yml
```

Each thin caller looks like:

```yaml
name: monolith-java-acceptance-stage
on:
  schedule: [{cron: '0 * * * *'}]
  workflow_dispatch:
    inputs:
      commit-sha: { required: false, type: string }
      debug-skip-tests: { required: false, type: boolean, default: false }
permissions: {}
concurrency: { group: monolith-java-acceptance-stage }
jobs:
  stage:
    uses: ./.github/workflows/_acceptance-stage.yml
    secrets: inherit
    with:
      architecture: monolith
      language: java
      prefix: monolith-java
      image-base-names: |
        ghcr.io/${{ github.repository_owner }}/${{ github.event.repository.name }}/monolith-system-java
      service-names: system
      endpoint-base-port: 3111
      commit-sha: ${{ inputs.commit-sha }}
      debug-skip-tests: ${{ inputs.debug-skip-tests }}
```

### Files that are unchanged

- `_prerelease-pipeline.yml`, `_meta-prerelease-pipeline.yml` — already reusable; will be updated to call the new `_qa-stage.yml` etc. instead of `monolith-java-qa-stage.yml`.
- `prerelease-pipeline-{monolith,multitier}-{lang}.yml` — already thin callers around `_prerelease-pipeline.yml`.
- `meta-bump-all.yml`, `meta-prerelease-stage.yml`, `meta-release-stage.yml`, `meta-prerelease-dry-run.yml` — meta-level, unaffected.
- `bump-patch-version-*.yml` — out of scope for this plan.
- `_*-commit-stage.yml` — out of scope (genuinely per-language, see Rationale).
- `cleanup.yml`, `lint-workflows.yml`, `cross-lang-system-verification.yml`, `move-tickets-to-qa.yml` — single workflows, not duplicated.

## gh-optivem coordination

The scaffolder (`gh-optivem/internal/steps/apply_template.go` and `internal/templates/templates.go`) currently:

1. Picks one shop file matching `{arch}-{testLang}-{stage}.yml` from `.github/workflows/`.
2. Renames it to `{stage}.yml` in the student repo.
3. Runs ~20 text replacements (paths, image names, env names, tag prefixes, prefix-drops).

After consolidation, the per-(arch, lang) caller is a 20-line file that `uses: ./.github/workflows/_acceptance-stage.yml`. The scaffolder must still produce a working `acceptance-stage.yml` in the student repo. Three options:

### Option A — Naive: copy both files, fix paths in both (RECOMMENDED for v1)

Scaffolder copies the chosen caller AND the matching `_<stage>.yml` reusable. Renames caller to `{stage}.yml`. Applies path/image-name fixups to both files. The `uses: ./.github/workflows/_acceptance-stage.yml` line in the caller stays unchanged (the reusable is also at `.github/workflows/_acceptance-stage.yml` in the student repo).

**Student repo file count for stages**: 8 stage callers (`acceptance-stage.yml`, `qa-stage.yml`, …) + 8 reusables (`_acceptance-stage.yml`, …) = 16 files instead of today's 8.

**Implementation effort in gh-optivem**: small. Extend `monolithPipelineWorkflows` / `multitierPipelineWorkflows` to also include `{src: "_acceptance-stage.yml", dst: "_acceptance-stage.yml"}` etc. Most existing fixup rules apply unchanged because the substring patterns (`monolith-system-java`, `system/monolith/java`, …) only appear in the reusable, which is still copied verbatim into the student repo.

**Pedagogy concern**: students see twice as many workflow files. Mitigation: documentation pattern (`.github/workflows/_*.yml = shared logic, .github/workflows/<stage>.yml = entry point`) is teachable and consistent with shop itself.

### Option B — Inline at scaffold time: produce one file per stage in student repo

Scaffolder reads both shop files, parses the reusable's `jobs:` block, inlines it into the caller (mapping `inputs.architecture` → the literal value, etc.), and writes a single `{stage}.yml` to the student repo. Student count stays at 8 stage files.

**Implementation effort in gh-optivem**: large. Requires a YAML-aware transform (parse, walk, substitute, serialize) instead of textual replace. Risk of subtle YAML re-formatting drift between scaffold runs.

**Recommendation**: defer. Worth doing only if Option A's two-files-per-stage proves to be a real teaching obstacle.

### Option C — Cross-repo `uses:` to shop's reusable

Scaffolder rewrites `uses: ./.github/workflows/_acceptance-stage.yml` to `uses: optivem/shop/.github/workflows/_acceptance-stage.yml@v1`. Student repo gets only the caller; the reusable lives in shop.

**Pros**: cleanest student repo (1 file per stage). Bug fixes in shop's reusable propagate automatically via the next `@v1` tag bump.

**Cons**: student CI now depends on shop's release stability. A breaking change in `_acceptance-stage.yml` immediately breaks every student repo on the next workflow run. Defeats the pedagogical principle of "students own their CI". Also requires shop's reusable workflows to be public (currently shop is public, so this is fine, but it's a coupling.)

**Recommendation**: do not adopt. Pedagogy concerns outweigh the file-count savings.

### gh-optivem changes required (Option A)

In `gh-optivem/internal/steps/apply_template.go`:

- `monolithPipelineWorkflows()` and `multitierPipelineWorkflows()` extend the returned map to include the shared reusables alongside the per-(arch,lang) callers:

  ```go
  return map[string]string{
      p + suffixAcceptanceStage + stageSuffix + ".yml": acceptStageYml,
      "_acceptance-stage" + stageSuffix + ".yml":       "_acceptance-stage" + stageSuffix + ".yml",
      // … same for qa-stage, prod-stage, etc.
  }
  ```

- The fixup-replacement helpers (`monolithContentReplacements`, `multitierContentReplacements`) need no changes — the substrings they target (`monolith-system-java`, `system/monolith/java/VERSION`, etc.) only appear in the reusable's body, which gets copied verbatim into the student repo and then text-replaced. The thin caller has only short literal values (e.g. `language: java`) which are also covered by the existing rules.

- `forbiddenTemplateRefs()` (the post-scaffold validator) needs no changes for the same reason — the substrings remain forbidden, and they should still appear nowhere in the scaffolded output.

- New plan needed in `gh-optivem` repo to track its side of this work, including a manual-test-runner-shop run that scaffolds all 6 (arch, lang) combos and verifies actionlint passes on each.

### Coordination with the existing `templates/` move

Plan `20260430-055950-move-scaffold-workflows-to-templates-subdir.md` moves 8 scaffold-only workflows to `.github/workflows/templates/`. The new `_<stage>.yml` reusables proposed here must stay top-level (they are referenced by `uses: ./.github/workflows/_acceptance-stage.yml` from the active per-(arch,lang) callers — moving them to a subdir would break those references). No conflict, but worth noting in both plans.

## Phased rollout

Each phase ends in a green CI run on shop and a successful `gh-optivem/scripts/manual-test-runner-shop.sh` (which scaffolds and validates a sample student repo).

### Phase 0 — Verification baseline

1. Run `./test-all.sh` and capture which stages pass today.
2. Run `gh-optivem/scripts/manual-test-runner-shop.sh` to scaffold all 6 (arch, lang) combos and capture baseline (e.g. actionlint, sample test pass).
3. Document the suite-name drift bug (`contract-isolated-stub` vs `contract-stub-isolated`) as a separate ticket — fix it BEFORE consolidation so the consolidated reusable inherits the canonical name without question.

### Phase 1 — Extract `setup-language-toolchain` composite action (in `optivem/actions`)

Lowest risk, highest immediate payoff. Does not change any workflow filenames or shapes; just replaces the ~25-line per-language setup block with a single `uses: optivem/actions/setup-language-toolchain@v1` call in each of the 48 stage workflows.

- Implement composite action with branches for `java`, `dotnet`, `typescript`.
- Inputs: `language`, `working-directory` (where to cache key off), `playwright` (boolean, default true).
- Replaces: `Setup .NET` / `Setup Java` / `Setup Node` / `Setup Gradle` / `Pre-warm Gradle Wrapper` / `Cache NuGet` / `Cache Playwright` / `Compile System Tests` / `Install Playwright System Dependencies` blocks.
- Roll out per-stage-family: edit all 6 monolith-acceptance-stage files first, run shop CI, confirm green; then 6 multitier-acceptance-stage; etc.

End of Phase 1: 48 stage files lose ~25 lines each, 6× duplicated; same shape, no scaffolder change.

### Phase 2 — Consolidate one stage family (acceptance-stage)

Pilot the reusable pattern on the most-exercised family before generalizing.

1. Author `_acceptance-stage.yml` mirroring `monolith-java-acceptance-stage.yml`, with inputs.
2. Rewrite all 6 `{arch}-{lang}-acceptance-stage.yml` files as thin callers.
3. Run shop CI on all 6.
4. Update `gh-optivem` per "gh-optivem changes required (Option A)". Run `manual-test-runner-shop.sh` for all 6 (arch, lang) combos.
5. Validate: scaffolded student repo's `acceptance-stage.yml` + `_acceptance-stage.yml` pass actionlint and a sample test.

### Phase 3 — Roll out to remaining stage families

Apply the same pattern to: `acceptance-stage-cloud`, `acceptance-stage-legacy`, `qa-stage`, `qa-stage-cloud`, `prod-stage`, `prod-stage-cloud`, `qa-signoff`. One stage family per PR; each PR runs the full Phase-2 validation (shop CI + scaffolder smoke test).

### Phase 4 — Documentation

- Update `docs/operations/*` (or wherever the workflow architecture is described) to describe the reusable pattern.
- Update `gh-optivem/MAPPING.md` / `NAMING.md` to reflect the source-file split (caller + reusable).
- Add a short README in `.github/workflows/` describing the underscore-prefix convention (`_*.yml` = reusable, called by `<stage>.yml` siblings).

## Open questions for the author

1. **Pedagogy weight on Option A vs B.** If 16 stage files in student repos (8 callers + 8 reusables) is acceptable for teaching, Option A is clearly preferred. If you want students to see exactly one workflow per stage as a teaching simplification, Option B (scaffold-time inline) is needed and the gh-optivem effort is meaningfully larger.
2. **Suite-name canonicalisation.** Before consolidating, decide canonical names: is it `contract-stub-isolated` (Java/.NET) or `contract-isolated-stub` (TypeScript)? The TypeScript drift suggests the rest of the codebase agrees on `…-stub-isolated`; pick that and rename TypeScript before consolidating.
3. **Composite action vs reusable workflow for runtime setup.** Phase 1 proposes a composite action; another option is to put the setup steps inside `_acceptance-stage.yml` directly, gated by `if: inputs.language == ...` (the same pattern `_prerelease-pipeline.yml` uses today). Composite action is more reusable across workflows that are NOT stages (e.g. one-off lint/verify workflows that also need a toolchain), but is one more layer. Recommendation: composite action, because at least 8 stage families will reuse it, and any new ad-hoc workflow gets the same setup path.
4. **Whether to also migrate `_prerelease-pipeline.yml`'s embedded compile blocks** (lines 113–179) to call the same composite action. Yes — same Phase 1 scope, just don't forget it.
