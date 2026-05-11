# Redesign `gh optivem test system` — test-only by default, add `test setup` verb, remove negation cluster

🤖 **Picked up by agent** — `ValentinaLaptop` at `2026-05-11T13:41:32Z`

## Decisions (resolved 2026-05-11)

1. **Default scope of `test system` → test-only.** Implicit Build / Up / Setup phases are removed from `test system`'s default behavior. The verb runs ONLY suites. The pre-existing health probe (today's `--no-start` codepath in `prepareSystem`) becomes the *only* codepath: if any system isn't responding to its health probe, error out with "start it first with `gh optivem run system`."
2. **Migration → hard cut.** `--no-build`, `--no-start`, `--no-setup`, `--rebuild`, `--restart` are removed from `test system` in the same release that ships `test setup`. No deprecation period, no shims. All 14 shop workflow files plus gh-optivem's scaffold templates are migrated in coordinated PRs (one in gh-optivem, one in shop).
3. **New verb → `gh optivem test setup`.** Runs the `setupCommands` block from `tests.yaml`. Subcommand of `test` (not a top-level `setup` verb) because the commands prepare the *test harness*, not the SUT — matches the existing `setupCommands:` config field name.

## Decision

Two coupled changes across two repos.

**In `gh-optivem`:**
- `gh optivem test system` becomes test-only by default. No implicit Build, Up, or Setup. Health-probes every entry in `systems.yaml`; errors if any aren't up.
- New `gh optivem test setup [--test-config path]` runs `setupCommands` from `tests.yaml`.
- Delete `--no-build`, `--no-start`, `--no-setup`, `--rebuild`, `--restart` flags from `test system`. (`--rebuild` already exists on `build system`; `--restart` already exists on `run system`.)
- `build system`, `run system`, `stop system`, `clean system` unchanged — they're already correctly factored.

**In `shop`:**
- All 14 workflow files migrate to: `gh optivem test setup` → `gh optivem run system` → N × `gh optivem test system --suite <id>` → `gh optivem stop system`. Zero negation flags.
- `CLAUDE.md`'s documented dev sequence updates to include `test setup` before the first `test system --sample` call.

## Rationale

### The negation-cluster smell

234 occurrences of `--no-build` / `--no-start` / `--no-setup` across 14 workflow files. When the same three negation flags ship together at every callsite, the default mode is wrong for the audience that actually uses the command. Mainstream service-lifecycle CLIs split phases into separate verbs:

- **docker compose** — `build`, `up`, `down`, `run`, `exec`
- **systemctl** — `start`, `stop`, `restart`, `status`
- **kubectl** — `apply`, `delete`, `get`
- **terraform** — `init`, `plan`, `apply`, `destroy`

`gh optivem` already has the right verb set (`build`, `run`, `test`, `stop`, `clean`). The problem is purely that `test system` overloads itself with three other phases and gives users negation flags to peel them off.

### Why not "smart implicit"

The alternative — keep `test system` implicit but rely on state detection — was considered and rejected:

- `Up` already short-circuits via `IsAnyURLUp` (`internal/runner/system.go:131`).
- `Build` is already incremental (compose layer cache).
- But each implicit phase still costs 1–5s of probe / cache-check overhead per call. Across ~8 suites × 14 workflows ≈ 112 callsites paying that cost on every CI run.
- `setup` has no state detection today; adding a marker file would introduce magic that fights "explicit is better than implicit."

### Why `test setup` is the only meaningful phase to split out

- **`Build`** (`docker compose build`) is already incremental. Calling it explicitly adds nothing in CI; calling it implicitly costs nothing.
- **`Up`** is already idempotent via the `IsAnyURLUp` short-circuit. Same story.
- **`setupCommands`** is the only block that (a) has no idempotency, (b) costs 30–90s per call, and (c) is meaningful to lift to its own callable verb. It's also the only one users actually need `--no-*` for *correctness*, not just speed.

### Why `setup`, not `compile` / `build` / `install`

The `setupCommands` block is heterogeneous across languages:

| Language | Today's setupCommands |
|---|---|
| TypeScript | `npm ci` + `npx playwright install chromium` (dep install + browser asset download) |
| Java | `.\gradlew.bat clean compileJava compileTestJava` (compile test sources) |
| .NET | restore + build of test project (restore + compile) |

- **`compile`** is wrong for TypeScript — no compilation happens; Playwright runs `.ts` directly.
- **`build`** is partially wrong for TypeScript for the same reason.
- **`install`** is wrong for Java/.NET, where the commands compile test sources rather than install dependencies.
- **`restore`** is dotnet-specific jargon.
- **`setup`** is the only word accurate across all three. It also matches the existing `setupCommands:` config field name — verb names should match the config keys they operate on (cf. `docker compose up` matching `services:`, `kubectl apply` matching `kind:`).
- **`setup`** is also open-ended for future work: if someone later wants to seed a database, hydrate secrets, or pre-warm a cache in this block, "setup" still describes it.

### Why hard cut, no deprecation

`gh-optivem` is consumed by `shop` and by student-scaffolded repos. The deprecation-shim alternative was rejected because:

- Student repos in this academy are short-lived per-course artifacts; the stability contract is weaker than for a public CLI.
- All current consumers (shop's workflows + recent scaffolded repos) can be updated in one coordinated PR.
- Keeping the negation flags as shims forever is the worst outcome — the smell stays in `--help` output, in the codebase, and in muscle memory.
- A clean break forces all consumers to the new shape immediately, which is the goal.

## Scope

### `gh-optivem` (CLI repo)

**`runner_commands.go`:**
- `newTestSystemCmd()`: delete `noBuild`, `rebuild`, `noStart`, `restart`, `noSetup` variables and their `cmd.Flags().BoolVar(...)` definitions. Strip those fields from the `runner.TestOptions{...}` construction. Update the example block to drop `--no-build --no-start` and the `--rebuild` example.
- Add `newTestSetupCmd()` returning a Cobra command for `gh optivem test setup`. Wires `--test-config` only (setupCommands run in `testsCwd`; no SUT access needed). Calls a new `runner.RunSetup(tests, testsCwd)`.
- `newTestCmd()`: register both child commands.

**`internal/runner/tests.go`:**
- `TestOptions`: delete `NoBuild`, `Rebuild`, `NoStart`, `Restart`, `NoSetup` fields.
- `RunTests`: delete the `prepareSystem(sys, systemCwd, opts)` call. Replace with a probe-only precheck (today's `NoStart=true` branch lifted up): for each system in `sys.Systems`, if `!IsAnyURLUp(s, opts.Health)`, return `fmt.Errorf("system %s is not running — start it first with `gh optivem run system`", s.Label)`. Note: drop the trailing `(or omit --no-start)` from today's error string. Delete the `setupCommands` loop (today's lines 102–109) — moved to `RunSetup`.
- `prepareSystem`: delete entire function.
- New: `func RunSetup(tests *TestsConfig, testsCwd string) error` — iterates `tests.SetupCommands`, calls `runShell(sc.Command, testsCwd, sc.Env)` for each, wraps errors with `fmt.Errorf("setup %q: %w", sc.Name, err)`. (Today's lines 102–109 lifted verbatim.)

**Tests:**
- `runner_commands_test.go`: remove cases asserting the deleted flags; add cases for `test setup`.
- `internal/runner/tests_test.go`: drop `TestOptions` field assertions for the removed fields; add a `RunSetup` happy-path test.

**Docs in CLI repo:**
- `README.md`: update examples to show the new verb sequence.
- `MAPPING.md`, `NAMING.md`, `BACKLOG.md`: update wherever they reference the old flags.

**Scaffold templates in CLI repo:**
- `internal/templates/` (or wherever workflow templates are sourced) — update every template that includes `gh optivem test system --no-*` to use the new verb sequence. To be enumerated during execution.

**Untouched in CLI repo:**
- `internal/runner/system.go` — `Build`, `Up`, `Down`, `Clean` unchanged.
- `cmd/build system`, `run system`, `stop system`, `clean system` — unchanged.
- Config loading (`projectconfig`, `resolveSystemPath`, `resolveTestsPath`) — unchanged.

### `shop` (consumer repo)

**Workflow files (14, all under `.github/workflows/`):**

| Family | Files |
|---|---|
| Acceptance (monolith) | `monolith-{java,dotnet,typescript}-acceptance-stage.yml` (3) |
| Acceptance (multitier) | `multitier-{java,dotnet,typescript}-acceptance-stage.yml` (3) |
| Acceptance legacy (monolith) | `monolith-{java,dotnet,typescript}-acceptance-stage-legacy.yml` (3) |
| Acceptance legacy (multitier) | `multitier-{java,dotnet,typescript}-acceptance-stage-legacy.yml` (3) |
| Cross-language | `cross-lang-system-verification.yml` (1) |
| Prerelease | `_prerelease-pipeline.yml` (1) |

Pattern in each file:

```yaml
# BEFORE
- name: smoke
  run: gh optivem test system --suite smoke --no-build --no-start
- name: acceptance-api
  run: gh optivem test system --suite acceptance-api --no-setup --no-build --no-start
- name: acceptance-ui
  run: gh optivem test system --suite acceptance-ui --no-setup --no-build --no-start
- ... (etc, ~8 suites per workflow)

# AFTER
- name: setup
  run: gh optivem test setup
- name: start
  run: gh optivem run system
- name: smoke
  run: gh optivem test system --suite smoke
- name: acceptance-api
  run: gh optivem test system --suite acceptance-api
- name: acceptance-ui
  run: gh optivem test system --suite acceptance-ui
- ... (etc)
- name: stop
  if: always()
  run: gh optivem stop system
```

Each workflow already has a step that brings up the SUT earlier in the job (compose-up or `gh optivem run system`). Where that's already present, the `start` step in the new sequence is redundant and should be removed — to be verified per-workflow during execution.

**Docs in shop:**
- `CLAUDE.md`: "System Test Verification" section — insert `gh optivem test setup` before the first `test system --sample` in the documented dev sequence.
- `docs/operations/running-system-tests.md`: update if it references the old flags.
- `README.md`: update if it lists the CLI invocation pattern.

### Coordination with consolidate-stage-workflows plan

The consolidation plan (`20260430-160000-consolidate-stage-workflows.md`) is mid-flight and consolidates 48 stage workflow files into 8 reusables + 48 thin callers. **This redesign should land first.** Reasons:

- Smaller current surface area: 14 current workflow files vs. 48 thin-caller + 8 reusable files after consolidation.
- The consolidation work copies whatever shape exists at consolidation time; landing this redesign first means the reusables start clean.
- The redesign reduces per-workflow line count by collapsing the negation cluster, making consolidation diffs smaller and easier to review.

If the consolidation plan lands first instead, this redesign migrates the 8 reusables (the per-suite step blocks) rather than the 14 originals — same logical change, slightly different file set.

## Phases

### Phase 1 — gh-optivem changes (one PR)

1. Add `runner.RunSetup` + `newTestSetupCmd()`.
2. Replace `prepareSystem` call in `RunTests` with the probe-only precheck.
3. Delete the five flags from `test system` and the corresponding `TestOptions` fields. Delete `prepareSystem`.
4. Update tests.
5. Update gh-optivem's scaffold templates so newly-scaffolded student repos generate the new shape.
6. Update README / MAPPING / NAMING / BACKLOG.
7. Bump version, tag release.

### Phase 2 — shop migration (one PR, depends on Phase 1 release)

1. Bump gh-optivem version pin in shop workflows if pinned (to be verified — `gh extension install optivem/gh-optivem` may be `@latest`).
2. Migrate all 14 workflow files: replace `--no-*` callsites with the new sequence; add explicit `test setup` step; remove redundant start steps if a `run system` step already exists.
3. Update `CLAUDE.md`, `README.md`, `docs/operations/running-system-tests.md`.
4. Run `./compile-all.sh` (per project guideline).
5. Run `gh optivem test system --sample` per language locally to verify migrated workflows work end-to-end before pushing.

### Phase 3 — verify in CI

1. Push and let one workflow per (arch, language) pair complete successfully on the schedule.
2. If any fail, fix the workflow inline. Do not roll back gh-optivem — the new shape is the contract.

## Tradeoffs

**Cost: hard cut breaks any student repo that doesn't re-scaffold.** Mitigated by: student repos are short-lived course artifacts; the new pattern is more learnable than the negation trio anyway (the explicit verb sequence is what the course teaches).

**Cost: workflows get one extra step per job.** Today's first suite step implicitly runs setup; the new shape makes it a dedicated step. This is +1 step per workflow job (no extra wall time — the same `setupCommands` run once either way). The benefit is readability: a reader of the workflow YAML sees the lifecycle top-down without mentally inverting three negation flags.

**Cost: `gh optivem test system` against a not-yet-started SUT now errors instead of magically starting it.** Intentional — the implicit start was magic and conflated concerns. For local hacking, users learn the sequence once; CI already chains the steps. Mainstream CLIs (`npm test`, `cargo test`) don't auto-start external services either.

**Benefit: 234 callsite occurrences of the negation trio collapse to zero. The CLI's `--help` output loses three confusing flags. The workflow YAML reads top-down as a clear lifecycle. New consumers learn the explicit sequence once instead of decoding negations.**

## What we're NOT doing

- **Not** adding a bundled `gh optivem verify system` / `ci system` convenience verb. Mainstream CLIs trust users to chain verbs; a bundle reintroduces the conflation problem this redesign exists to solve. Local one-liner convenience belongs in a shell function or alias.
- **Not** changing `run system`, `build system`, `stop system`, or `clean system` shapes. They're already correctly factored.
- **Not** auto-detecting / auto-starting the SUT from `test system`. Spooky action at a distance.
- **Not** adding state detection / marker files for `test setup`. The verb is called explicitly when needed; if a caller invokes it twice, that's a caller bug, not a CLI concern.
- **Not** renaming the `setupCommands:` config field. The YAML field stays the same; only the CLI verb is new.
- **Not** removing `--rebuild` from `build system` or `--restart` from `run system`. Those are single, opt-in flags on the correct verb — no smell.

## Open questions

None. The three load-bearing decisions (test-only default, hard cut, `test setup` naming) are resolved above. Execution can begin with Phase 1 in `gh-optivem`.
