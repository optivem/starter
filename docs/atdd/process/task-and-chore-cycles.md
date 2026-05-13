# Structural Cycle Mechanics

## Purpose

This doc defines the **WRITE**, **REVIEW**, **TEST**, and **COMMIT** mechanics for the structural cycles triggered by the three `subtype:*` labels on Task tickets: `subtype:system-interface-redesign`, `subtype:external-system-interface-redesign`, and `subtype:system-implementation-change`. Cycle-level placement (which cycle dispatches when, and what comes after) is owned by the Go runtime in `gh-optivem` (canonical YAML embedded in the binary; see the rendered [process-flow diagram](https://github.com/optivem/gh-optivem/blob/main/docs/process-flow-diagram.md)); see also [cycles.md](cycles.md). This file is the substance of what happens *inside* each phase.

It mirrors the role of the AT per-phase docs (`at-red-test.md`, `at-red-dsl.md`, `at-red-system-driver.md`, `at-green-system.md`) and the CT per-phase docs (`ct-red-test.md`, `ct-red-dsl.md`, `ct-red-external-driver.md`, `ct-green-stubs.md`) for behavioral cycles.

---

## Common conventions

- **Commit message format.** See [at-cycle-conventions.md](at-cycle-conventions.md). Every commit follows `<Ticket> | <Phase>`, optionally prefixed with `#<issue-number> | `. The phase suffix is the phase *prefix only* (e.g. `SYSTEM INTERFACE REDESIGN`) — never append `- WRITE`, `- REVIEW`, `- TEST`, or `- COMMIT`.
- **Commit handoff.** See [cycles.md § Commit Handoff](cycles.md#commit-handoff). Agents do not run `git commit`; they exit with the working-tree delta and the wrapping CLI runs the human gate ("Can I commit?") and the commit.
- **Phase progression.** See [shared-phase-progression.md](shared-phase-progression.md). Phases ending in STOP block on explicit user approval.
- **TEST scope.** COMPILE always runs first (`./compile-all.sh`, `./gradlew build`, `npx tsc --noEmit`, or `dotnet build` for the in-scope projects). Sample-suite scope is then picked at the CHOOSE_TESTS menu (`[a]`ll / `[s]`ome suites / `[p]`ecific tests / `[n]`o tests / `[x]` reject) — never self-initiate `gh optivem test run --sample` before the operator answers. Compile or test RED routes through a human STOP (Enter dispatches the fix-agent, `abort` halts the cycle); the fix-agent never commits.

---

## Shared structural-cycle TEST

Every structural-cycle TEST runs after REVIEW (which itself runs after WRITE). Goal: verify the change compiles and (optionally) the sample suite still passes locally before COMMIT. The TEST procedure honours the **Scope** declared in `gh-optivem.yaml` (see [`cycles.md`](cycles.md) "Scope"): compile and sample-suite work is restricted to the in-scope architecture and Test Lang.

The structural-cycle TEST has two stages — COMPILE (always runs) and a sample-suite stage whose scope is picked at a menu — and a shared failure path that routes through a human STOP and then the `atdd-fix-verify` agent.

1. **COMPILE — always runs.** Compile the in-scope components (per `CLAUDE.md`: `./compile-all.sh` from the repo root, or a single-project command like `./gradlew build` / `npx tsc --noEmit` / `dotnet build` for narrow changes). On compile RED, jump to step 5.

2. **CHOOSE_TESTS — operator picks scope.** When compile passes, the orchestrator presents the test-selection menu. Use this exact prompt, substituting the in-scope test language:

   ```
   About to run sample suite for <in-scope test language>. Choose one:
     - [a] all              → every category × every language in scope
     - [s] some suites      → comma-separated suite names
     - [p] specific tests   → comma-separated test names
     - [n] no tests         → skip the suite, go straight to COMMIT
     - [x] reject           → abort the cycle (no commit)
   Choice?
   ```

   Wait for an explicit answer. The selection is stored as a list of `gh optivem test run` invocations (`selected_test_commands`) and is re-used on retry — operators who want a different scope must abort and re-enter the cycle.

3. **RUN_TESTS.** Execute the selected commands (or no-op for `[n]`). The runner classifies the outcome as `ok`, `red`, or `infra` (infra halts the run upstream; `ok` advances to COMMIT; `red` jumps to step 5).

4. **Drift warning.** Print a notice naming any out-of-scope implementations that were deliberately left untouched, e.g.:

   ```
   Out-of-scope implementations not updated by this run:
     - dotnet/monolith
     - dotnet/multitier
     - typescript/monolith
     - typescript/multitier
   Open follow-up tickets or rerun with --architecture both --system-lang all to propagate.
   ```

   If Scope was the broadest option (`Architecture=both`, `System Lang=all`, `Test Lang=all`), skip this step.

5. **Failure → STOP → fix or abort.** On compile or test RED, the orchestrator halts at a human-review STOP (`Press Enter to continue, or type abort to halt`). `abort` exits the cycle cleanly with the working tree preserved. `Enter` dispatches `atdd-fix-verify` with `failure_type=compile` or `failure_type=test` — one agent, branching on the param — which applies the smallest fix and exits without committing. The orchestrator then retries COMPILE (compile failure) or RUN_TESTS (test failure) once. A second RED halts for human takeover.

The `subtype:external-system-interface-redesign` path has no standalone TEST — sample-run gating happens inside the CT sub-process.

---

## Shared structural-cycle COMMIT

Every structural-cycle COMMIT (`SYSTEM INTERFACE REDESIGN`, `CHORE`) follows the same four steps, with only the commit-message phase suffix varying.

1. Leave the working-tree delta in place and exit cleanly. The wrapping CLI runs the "Can I commit?" gate and produces the commit with message `<Ticket> | <PHASE>` where `<PHASE>` is `SYSTEM INTERFACE REDESIGN` or `CHORE` per the cycle. See [cycles.md § Commit Handoff](cycles.md#commit-handoff).
2. If a GitHub issue was provided, tick any checklist items completed by this commit (local action; not CI-gated).
3. Move the issue to **TICKET STATUS - IN ACCEPTANCE** — see [shared-ticket-status-in-acceptance.md](shared-ticket-status-in-acceptance.md).

The `subtype:external-system-interface-redesign` path has no standalone COMMIT — see "EXTERNAL SYSTEM INTERFACE REDESIGN" below.

---

## SYSTEM INTERFACE REDESIGN

The WRITE / REVIEW phase mechanics for `subtype:system-interface-redesign` (the channel-agnostic system-side driver redesign — API, UI, mobile, CLI, admin, …) live in their own phase doc: see [system-interface-redesign.md](system-interface-redesign.md). TEST and COMMIT use the shared structural-cycle procedures defined above (commit suffix `SYSTEM INTERFACE REDESIGN`).

---

## EXTERNAL SYSTEM INTERFACE REDESIGN

### Purpose

The `subtype:external-system-interface-redesign` path has **NO standalone WRITE / REVIEW / TEST / COMMIT** — it routes entirely through the **Contract Test Sub-Process**. See [ct-red-test.md](ct-red-test.md), [ct-red-dsl.md](ct-red-dsl.md), [ct-red-external-driver.md](ct-red-external-driver.md), and [ct-green-stubs.md](ct-green-stubs.md) for the per-phase mechanics, and [ct-cycle-conventions.md](ct-cycle-conventions.md) for the commit-message conventions.

### What it produces

- The four CT commits (`CT - RED - TEST`, `CT - RED - DSL`, `CT - RED - EXTERNAL DRIVER`, `CT - GREEN - STUBS`) — see those per-phase docs.
- The issue moved to **TICKET STATUS - IN ACCEPTANCE**.

### Anti-patterns

- Adding standalone TEST / COMMIT here — there is none; the work IS the CT sub-process.
- Bypassing the CT sub-process by touching the External Driver directly without going through `CT - RED - EXTERNAL DRIVER`.

---

## CHORE

This is the WRITE phase of `subtype:system-implementation-change` — the cycle still exists, just under the runtime name `sut_cycle` rather than a separate ticket-type cycle. The phase suffix on the commit message is still `CHORE`.

### Purpose

Refactor / rename / move / dependency upgrade / build tweak / dead-code removal / internal abstraction change inside `system/`, with no boundary or behavioral impact. By definition, a `subtype:system-implementation-change` ticket does not change drivers, tests, DSL, or Gherkin; if it does, it has been misclassified.

### What it produces

- A single commit `<Ticket> | CHORE` containing only `system/` edits.
- Drivers (`driver-port/`, `driver-adapter/`), tests, DSL, and Gherkin untouched.
- The issue moved to **TICKET STATUS - IN ACCEPTANCE**.

### CHORE - WRITE

**Goal:** the structural change is implemented inside `system/`; drivers and tests are untouched; existing acceptance and contract tests still compile.

1. Implement the change as described in the ticket's checklist of refactor / upgrade steps.
2. Drivers — interfaces (`driver-port/`) and implementations (`driver-adapter/`) — are untouched. If the work turns out to require driver changes, STOP and reclassify the ticket — `subtype:system-implementation-change` by definition does not change boundaries; relabel as `subtype:system-interface-redesign` (or `subtype:external-system-interface-redesign` for an external-system change).
3. Tests, DSL, and Gherkin are untouched. If the work turns out to require behavioral test changes, STOP and reclassify the ticket as a Story or Bug.

### CHORE - REVIEW (STOP)

STOP. Present the implementation to the user and ask for approval. Do NOT continue.

**Review checklist:**
- Changes confined to `system/` (no `driver-port/`, `driver-adapter/`, `system-test/`, or feature-file edits).
- No behavioral change observable through the acceptance suite.
- All in-scope parallel implementations updated symmetrically where the change applies.
- No edits under `system-test/<lang>/.../Legacy/`.

### Anti-patterns

- Driver / test / DSL changes (`subtype:system-implementation-change` by definition does not change boundaries — STOP and relabel as `subtype:system-interface-redesign`).
- Behavioral test changes (likewise — STOP and reclassify as a Story or Bug).
- Bundling unrelated refactors into one commit; keep the diff scoped to the ticket's checklist.
