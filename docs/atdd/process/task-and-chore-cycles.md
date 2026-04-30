# Task and Chore Cycle Mechanics

## Purpose

This doc defines the **WRITE**, **REVIEW**, **TEST**, and **COMMIT** mechanics for the four structural cycles triggered by ticket types `system-api-task`, `system-ui-task`, `external-api-task`, and `chore`. Cycle-level placement (which cycle dispatches when, and what comes after) is owned by the Go runtime via [process-flow.yaml](process-flow.yaml); see also [cycles.md](cycles.md). This file is the substance of what happens *inside* each phase.

It mirrors the role of the AT per-phase docs (`at-red-test.md`, `at-red-dsl.md`, `at-red-system-driver.md`, `at-green-system.md`) and the CT per-phase docs (`ct-red-test.md`, `ct-red-dsl.md`, `ct-red-external-driver.md`, `ct-green-stubs.md`) for behavioral cycles.

---

## Common conventions

- **Commit message format.** See [at-cycle-conventions.md](at-cycle-conventions.md). Every commit follows `<Ticket> | <Phase>`, optionally prefixed with `#<issue-number> | `. The phase suffix is the phase *prefix only* (e.g. `SYSTEM API REDESIGN`) — never append `- WRITE`, `- REVIEW`, `- TEST`, or `- COMMIT`.
- **Commit confirmation.** See [shared-commit-confirmation.md](shared-commit-confirmation.md). Every COMMIT step asks "Can I commit?" with the proposed message and staged file list, and waits for explicit user approval before running `git commit`.
- **Phase progression.** See [shared-phase-progression.md](shared-phase-progression.md). Phases ending in STOP block on explicit user approval.
- **TEST gate (full | compile | skip).** The entire TEST phase is gated upfront with a single user prompt — nothing inside TEST runs until the user chooses, and the gate covers compile checks (`./compile-all.sh`, `./gradlew build`, `npx tsc --noEmit`, `dotnet build`) and the sample suite (`gh optivem test system --sample`) alike. Never self-initiate any of those commands, even compile-only ones.

---

## Shared structural-cycle TEST

Every structural-cycle TEST runs after REVIEW (which itself runs after WRITE). Goal: verify the change compiles and the sample suite still passes locally before COMMIT. The TEST procedure honours the **Scope** declared in pre-flight (see `.claude/commands/atdd/atdd-implement-ticket.md` "Scope Confirmation"): compile and sample-suite work is restricted to the in-scope architecture(s) and Test Lang(s).

1. **Ask the user upfront which checks to run.** Use this exact prompt, substituting in-scope languages:

   ```
   About to run TEST for <in-scope test languages>. Choose one:
     - full      → compile in-scope projects, then run sample suite (`gh optivem test system --sample`). Sample run takes a few minutes per language.
     - compile   → compile in-scope projects only, no sample suite.
     - skip      → skip TEST entirely, go straight to COMMIT (you accept the risk that compile or sample may fail in CI).
   Choice?
   ```

   Wait for an explicit `full`, `compile`, or `skip`. Never run any compile or sample command before this answer arrives. Never run multiple test/run/stop system commands in parallel without separately asking.

2. **If `skip`:** record that TEST was skipped (note in the post-TEST summary) and print the drift warning if applicable.

3. **If `compile` or `full`:** confirm in-scope components compile (per `CLAUDE.md`: `./compile-all.sh` from the repo root, or a single-project command like `./gradlew build` / `npx tsc --noEmit` / `dotnet build` for narrow changes). On compile failure, STOP and report — do not attempt the sample suite.

4. **If `full` and compile passed:** run the sample suite for each in-scope Test Lang (`gh optivem test system --sample`) and verify it passes.

5. Print a **drift warning** naming any out-of-scope implementations that were deliberately left untouched, e.g.:

   ```
   Out-of-scope implementations not updated by this run:
     - dotnet/monolith
     - dotnet/multitier
     - typescript/monolith
     - typescript/multitier
   Open follow-up tickets or rerun with --architecture both --system-lang all to propagate.
   ```

   If Scope was the broadest option (`Architecture=both`, `System Lang=all`, `Test Lang=all`), skip this step.

6. STOP. Present the chosen mode (`full` / `compile` / `skip`), the test results (if any), and the drift warning (if any) to the user. On failure, fix and re-enter TEST from step 1.

The EXTERNAL API REDESIGN cycle has no standalone TEST — sample-run gating happens inside the CT sub-process.

---

## Shared structural-cycle COMMIT

Every structural-cycle COMMIT (`SYSTEM API REDESIGN`, `SYSTEM UI REDESIGN`, `CHORE`) follows the same four steps, with only the commit-message phase suffix varying.

1. Apply the gate in [shared-commit-confirmation.md](shared-commit-confirmation.md) — ask "Can I commit?" with the proposed message and staged file list, and wait for explicit approval.
2. COMMIT with message `<Ticket> | <PHASE>` where `<PHASE>` is `SYSTEM API REDESIGN`, `SYSTEM UI REDESIGN`, or `CHORE` per the cycle.
3. If a GitHub issue was provided, tick any checklist items completed by this commit (local action; not CI-gated).
4. Move the issue to **TICKET STATUS - IN ACCEPTANCE** — see [shared-ticket-status-in-acceptance.md](shared-ticket-status-in-acceptance.md).

The EXTERNAL API REDESIGN cycle has no standalone COMMIT — see "EXTERNAL API REDESIGN" below.

---

## SYSTEM API REDESIGN / SYSTEM UI REDESIGN

### Purpose

Reshape the system's API or UI surface (controllers, DTOs, routes, status codes, error format; or page structure, form fields, navigation, copy, selectors) without changing observable behaviour through the existing acceptance suite. The driver-adapter absorbs the surface change so DSL, Gherkin, and tests stay untouched.

### What it produces

- A single commit `<Ticket> | SYSTEM API REDESIGN` (or `SYSTEM UI REDESIGN`) containing only `system/` and `driver-adapter/` edits (and, exceptionally, approved `driver-port/` edits).
- All in-scope parallel implementations updated (Java/.NET/TS × monolith/multitier — see [architecture/system.md](../architecture/system.md)).
- The issue moved to **TICKET STATUS - IN ACCEPTANCE**.

### SYSTEM \<boundary\> REDESIGN - WRITE

Where *boundary* ∈ {`API`, `UI`}. The same four steps apply; only the boundary-specific files change.

**Goal:** the System \<boundary\> Driver (interface + impl under `driver-port/.../shop/<api|ui>` and `driver-adapter/.../shop/<api|ui>`) reflects the new System \<boundary\> surface; the system code under `system/` reflects the new \<boundary\>; existing acceptance and contract tests still compile.

1. Update the System \<boundary\> itself under `system/` to match the ticket's checklist:
   - For API: controllers, request/response DTOs, routes, status codes, error format. Apply across **all parallel implementations** — see [architecture/system.md](../architecture/system.md) for the layout (Java/.NET/TS × monolith/multitier) and for where API URLs and their consumers live in each implementation. After editing the source of truth, grep the system tree for residual references (e.g. the old URL string) before moving on.
   - For UI: page structure, form fields, navigation, copy, selectors.
2. Update the matching System \<boundary\> Driver implementation (`driver-adapter/.../shop/<api|ui>`) to absorb the change. Prefer adapter-only changes — keep behaviour observable through the **existing** driver interface.
3. **Driver interface guardrail.** Do NOT modify any file under `driver-port/` casually. If an interface change is unavoidable, STOP separately at that boundary and present to the user: the method(s) you want to change, why the adapter alone cannot absorb the change, the proposed new signature(s). Wait for explicit user approval before editing any `driver-port/` file. (Such changes have no contract-test fallout because this is `shop/`, not `external/` — but they still touch the test surface and must be approved.)
4. Do not modify acceptance tests, DSL, Gherkin, or any code outside the System \<boundary\> layer + its driver. `system-test/<lang>/.../Legacy/` is read-only course-reference material — leave it untouched.

### SYSTEM \<boundary\> REDESIGN - REVIEW (STOP)

STOP. Present the system + driver changes (system code, driver-adapter, any approved driver-port changes) to the user and ask for approval. Do NOT continue.

**Review checklist:**
- All in-scope parallel implementations updated symmetrically (Java/.NET/TS × monolith/multitier).
- Driver-adapter URL / selector strings match the new system surface.
- No residual references to the old URL / route / selector remain (grep the system tree).
- No edits under `driver-port/` unless separately approved at the guardrail STOP.
- No edits under `system-test/<lang>/.../Legacy/`.
- DSL, Gherkin, and acceptance tests untouched.

### Anti-patterns

- Editing under `driver-port/` without an explicit guardrail STOP and approval.
- Changing tests / DSL / Gherkin instead of system code + adapter.
- Forgetting to grep for residual references to the old API URL / selector strings.
- Touching `system-test/<lang>/.../Legacy/` (read-only).
- Updating one implementation but leaving the other parallel implementations drifting.

---

## EXTERNAL API REDESIGN

### Purpose

This cycle has **NO standalone WRITE / REVIEW / TEST / COMMIT** — it routes entirely through the **Contract Test Sub-Process**. See [ct-red-test.md](ct-red-test.md), [ct-red-dsl.md](ct-red-dsl.md), [ct-red-external-driver.md](ct-red-external-driver.md), and [ct-green-stubs.md](ct-green-stubs.md) for the per-phase mechanics, and [ct-cycle-conventions.md](ct-cycle-conventions.md) for the commit-message conventions.

### What it produces

- The four CT commits (`CT - RED - TEST`, `CT - RED - DSL`, `CT - RED - EXTERNAL DRIVER`, `CT - GREEN - STUBS`) — see those per-phase docs.
- The issue moved to **TICKET STATUS - IN ACCEPTANCE**.

### Anti-patterns

- Adding standalone TEST / COMMIT here — there is none; the work IS the CT sub-process.
- Bypassing the CT sub-process by touching the External Driver directly without going through `CT - RED - EXTERNAL DRIVER`.

---

## CHORE

### Purpose

Refactor / rename / move / dependency upgrade / build tweak / dead-code removal / internal abstraction change inside `system/`, with no boundary or behavioral impact. By definition, a chore does not change drivers, tests, DSL, or Gherkin; if it does, it has been misclassified.

### What it produces

- A single commit `<Ticket> | CHORE` containing only `system/` edits.
- Drivers (`driver-port/`, `driver-adapter/`), tests, DSL, and Gherkin untouched.
- The issue moved to **TICKET STATUS - IN ACCEPTANCE**.

### CHORE - WRITE

**Goal:** the structural change is implemented inside `system/`; drivers and tests are untouched; existing acceptance and contract tests still compile.

1. Implement the chore as described in the ticket's checklist of refactor / upgrade steps.
2. Drivers — interfaces (`driver-port/`) and implementations (`driver-adapter/`) — are untouched. If the chore turns out to require driver changes, STOP and reclassify the ticket as a task — chores by definition do not change boundaries.
3. Tests, DSL, and Gherkin are untouched. If the chore turns out to require behavioral test changes, STOP and reclassify the ticket as a story or bug.

### CHORE - REVIEW (STOP)

STOP. Present the implementation to the user and ask for approval. Do NOT continue.

**Review checklist:**
- Changes confined to `system/` (no `driver-port/`, `driver-adapter/`, `system-test/`, or feature-file edits).
- No behavioral change observable through the acceptance suite.
- All in-scope parallel implementations updated symmetrically where the chore applies.
- No edits under `system-test/<lang>/.../Legacy/`.

### Anti-patterns

- Driver / test / DSL changes (chore by definition does not change boundaries — STOP and reclassify as a task).
- Behavioral test changes (likewise — STOP and reclassify as a story or bug).
- Bundling unrelated refactors into one chore commit; keep the diff scoped to the ticket's checklist.
