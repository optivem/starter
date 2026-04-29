# Task and Chore Cycle Mechanics

This file defines the **WRITE** and **COMMIT** mechanics for the structural-change cycles — the cycles triggered by ticket types `system-api-task`, `system-ui-task`, `external-api-task`, and `chore`. The high-level flow / placement of each cycle inside the overall pipeline is defined in `cycles.md`; this file defines what happens *inside* each phase.

It mirrors the role of the AT per-phase docs (`at-red-test.md`, `at-red-dsl.md`, `at-red-system-driver.md`, `at-green-system.md`) and the CT per-phase docs (`ct-red-test.md`, `ct-red-dsl.md`, `ct-red-external-driver.md`, `ct-green-stubs.md`) for behavioral-change cycles. Together, those files define every WRITE + COMMIT phase in the pipeline.

## Phase Progression

Proceed to the next phase automatically **unless** the current phase ends with **STOP**. When a phase ends with STOP, wait for the user to explicitly approve before continuing. If the user says something other than approval after a STOP, ask clarifying questions — do not execute the next phase.

## Commit Message Format

Same as `at-cycle-conventions.md`: every commit message follows the pattern `<Ticket> | <Phase>`. If a GitHub issue number was provided as input, prefix every commit message with `#<issue-number> | `. Example: `#59 | Redesigning Order Controller | SYSTEM API REDESIGN`.

The phase suffix in the message is the phase *prefix only* (e.g. `SYSTEM API REDESIGN`). Do **NOT** append `- COMMIT` or `- WRITE` to the phase in the commit message — those suffixes identify the section header only, not the commit message.

The COMMIT step itself is gated by the universal rule in `commit-confirmation.md` — ask the user "Can I commit?" with the proposed message and staged changes, and wait for explicit approval before running `git commit`.

---

## SYSTEM API REDESIGN - WRITE (STOP)

**Goal:** the System API Driver (interface + impl under `driver-port/.../shop/api` and `driver-adapter/.../shop/api`) reflects the new System API surface; the system code under `system/` reflects the new API; existing acceptance and contract tests still compile.

1. Update the System API itself (controllers, request/response DTOs, routes, status codes, error format) under `system/` to match the ticket's checklist. Apply the change across **all parallel implementations** — see `docs/atdd/architecture/system.md` for the layout (Java/.NET/TS × monolith/multitier) and for where API URLs and their consumers live in each implementation. After editing the source of truth, grep the system tree for residual references (e.g. the old URL string) before moving on.
2. Update the System API Driver implementation (`driver-adapter/.../shop/api`) to absorb the change. Prefer adapter-only changes — keep behaviour observable through the **existing** driver interface.
3. **Driver interface guardrail.** Do NOT modify any file under `driver-port/` casually. If you believe an interface change is unavoidable, STOP separately at that boundary and present to the user: the method(s) you want to change, why the adapter alone cannot absorb the change, the proposed new signature(s). Wait for explicit user approval before editing any `driver-port/` file. (Such changes have no contract-test fallout because this is `shop/`, not `external/` — but they still touch the test surface and must be approved.)
4. Do not modify acceptance tests, DSL, Gherkin, or any code outside the System API layer + its driver. Note: `system-test/<lang>/.../Legacy/` is read-only course-reference material — leave it untouched even when it references the old surface directly.
5. STOP. Present the system + driver changes to the user and ask for approval. Do NOT continue.

## SYSTEM API REDESIGN - COMMIT

1. Confirm affected components compile (per `CLAUDE.md`: `./gradlew build` / `npx tsc --noEmit` / `dotnet build`).
2. Run the sample suite for each affected language (per `CLAUDE.md`: `gh optivem test system --sample`) and verify it passes.
3. Apply the gate in `commit-confirmation.md` — ask "Can I commit?" with the proposed message and staged file list, and wait for explicit approval.
4. COMMIT with message `<Ticket> | SYSTEM API REDESIGN`.
5. If a GitHub issue was provided, tick any checklist items in the issue completed by this commit.
6. STOP. The CI **Acceptance Stage** is the verifier from here on; phase progression is controlled by the orchestrator.

---

## SYSTEM UI REDESIGN - WRITE (STOP)

**Goal:** the System UI Driver (interface + impl under `driver-port/.../shop/ui` and `driver-adapter/.../shop/ui`) reflects the new System UI surface; the system code under `system/` reflects the new UI; existing acceptance and contract tests still compile.

1. Update the System UI itself (page structure, form fields, navigation, copy, selectors) under `system/` to match the ticket's checklist.
2. Update the System UI Driver implementation (`driver-adapter/.../shop/ui`) to absorb the change. Prefer adapter-only changes — keep behaviour observable through the **existing** driver interface.
3. **Driver interface guardrail.** Same rule as `SYSTEM API REDESIGN - WRITE` — do not modify `driver-port/` casually; if unavoidable, STOP separately and get user approval before editing.
4. Do not modify acceptance tests, DSL, Gherkin, or any code outside the System UI layer + its driver.
5. STOP. Present the system + driver changes to the user and ask for approval. Do NOT continue.

## SYSTEM UI REDESIGN - COMMIT

1. Confirm affected components compile (per `CLAUDE.md`).
2. Run the sample suite for each affected language and verify it passes.
3. Apply the gate in `commit-confirmation.md` — ask "Can I commit?" with the proposed message and staged file list, and wait for explicit approval.
4. COMMIT with message `<Ticket> | SYSTEM UI REDESIGN`.
5. If a GitHub issue was provided, tick any checklist items completed by this commit.
6. STOP. The CI **Acceptance Stage** is the verifier from here on; phase progression is controlled by the orchestrator.

---

## EXTERNAL API REDESIGN

The External API Task Cycle has **no standalone WRITE or COMMIT phase of its own.** The work routes entirely through the **Contract Test Sub-Process**, whose four-commit sequence (`CT - RED - TEST`, `CT - RED - DSL`, `CT - RED - EXTERNAL DRIVER`, `CT - GREEN - STUBS`) is defined in the per-phase docs `ct-red-test.md`, `ct-red-dsl.md`, `ct-red-external-driver.md`, `ct-green-stubs.md`. See `cycles.md` for cycle placement and those per-phase docs (plus `ct-cycle-conventions.md`) for mechanics.

---

## CHORE - WRITE (STOP)

**Goal:** the structural change (refactor / rename / move / dependency upgrade / build tweak / dead-code removal / internal abstraction change) is implemented inside `system/`; drivers and tests are untouched; existing acceptance and contract tests still compile.

1. Implement the chore as described in the ticket's checklist of refactor / upgrade steps.
2. Drivers — interfaces (`driver-port/`) and implementations (`driver-adapter/`) — are untouched. If the chore turns out to require driver changes, STOP and reclassify the ticket as a task — chores by definition do not change boundaries.
3. Tests, DSL, and Gherkin are untouched. If the chore turns out to require behavioral test changes, STOP and reclassify the ticket as a story or bug.
4. STOP. Present the implementation to the user and ask for approval. Do NOT continue.

## CHORE - COMMIT

1. Confirm affected components compile (per `CLAUDE.md`).
2. Run the sample suite for each affected language and verify it passes.
3. Apply the gate in `commit-confirmation.md` — ask "Can I commit?" with the proposed message and staged file list, and wait for explicit approval.
4. COMMIT with message `<Ticket> | CHORE`.
5. If a GitHub issue was provided, tick any checklist items completed by this commit.
6. STOP. The CI **Acceptance Stage** is the verifier from here on; phase progression is controlled by the orchestrator.
