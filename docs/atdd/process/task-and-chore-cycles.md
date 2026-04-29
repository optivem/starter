# Task and Chore Cycle Mechanics

This file defines the **WRITE**, **TEST**, and **COMMIT** mechanics for the structural-change cycles — the cycles triggered by ticket types `system-api-task`, `system-ui-task`, `external-api-task`, and `chore`. The high-level flow / placement of each cycle inside the overall pipeline is defined in `cycles.md`; this file defines what happens *inside* each phase.

It mirrors the role of the AT per-phase docs (`at-red-test.md`, `at-red-dsl.md`, `at-red-system-driver.md`, `at-green-system.md`) and the CT per-phase docs (`ct-red-test.md`, `ct-red-dsl.md`, `ct-red-external-driver.md`, `ct-green-stubs.md`) for behavioral-change cycles. Together, those files define every WRITE + COMMIT phase in the pipeline.

## Commit Message Format

Same as `at-cycle-conventions.md`: every commit message follows the pattern `<Ticket> | <Phase>`. If a GitHub issue number was provided as input, prefix every commit message with `#<issue-number> | `. Example: `#59 | Redesigning Order Controller | SYSTEM API REDESIGN`.

The phase suffix in the message is the phase *prefix only* (e.g. `SYSTEM API REDESIGN`). Do **NOT** append `- COMMIT` or `- WRITE` to the phase in the commit message — those suffixes identify the section header only, not the commit message.

The COMMIT step itself is gated by the universal rule in `shared-commit-confirmation.md` — ask the user "Can I commit?" with the proposed message and staged changes, and wait for explicit approval before running `git commit`.

---

## Structural-cycle TEST (shared procedure)

Every structural-cycle TEST runs after WRITE and before COMMIT. Goal: verify the change compiles and the sample suite still passes locally before asking to commit. The sample run is **explicitly gated** because it spins up docker stacks and takes several minutes per language — never run it without user approval.

1. Confirm affected components compile (per `CLAUDE.md`: run `./compile-all.sh` from the repo root, or a single-project command for narrow changes). Compile is fast and runs without prompting.
2. Ask the user for explicit approval before running the sample suite: "About to run `gh optivem test system --sample` for <languages> — this takes a few minutes per language. Approve? (yes/no)". Wait for an explicit `yes` before proceeding. Never self-initiate; never run in parallel with other system-test commands without separately asking.
3. On approval, run the sample suite for each affected language (per `CLAUDE.md`: `gh optivem test system --sample`) and verify it passes.
4. STOP. Present the test results to the user. On failure, fix and re-enter TEST from step 1. On pass, proceed to COMMIT.

The EXTERNAL API REDESIGN cycle has no standalone TEST — its sample-run gating happens inside the CT sub-process.

---

## Structural-cycle COMMIT (shared procedure)

Every structural-cycle COMMIT (`SYSTEM API REDESIGN`, `SYSTEM UI REDESIGN`, `CHORE`) follows the same four steps, with only the commit-message phase suffix varying. TEST must have passed before entering COMMIT.

1. Apply the gate in `shared-commit-confirmation.md` — ask "Can I commit?" with the proposed message and staged file list, and wait for explicit approval.
2. COMMIT with message `<Ticket> | <PHASE>` where `<PHASE>` is `SYSTEM API REDESIGN`, `SYSTEM UI REDESIGN`, or `CHORE` per the cycle.
3. If a GitHub issue was provided, tick any checklist items completed by this commit (local action; not CI-gated).
4. Move the issue to **TICKET STATUS - IN ACCEPTANCE** — see [`shared-ticket-status-in-acceptance.md`](shared-ticket-status-in-acceptance.md). The cycle ends here; the agent is CI-unaware.

The EXTERNAL API REDESIGN cycle has no standalone COMMIT — see "EXTERNAL API REDESIGN" below for the CT-sub-process redirect.

---

## SYSTEM \<boundary\> REDESIGN - WRITE (STOP)

Where *boundary* ∈ {`API`, `UI`}. The same five steps apply; only the boundary-specific files change.

**Goal:** the System \<boundary\> Driver (interface + impl under `driver-port/.../shop/<api|ui>` and `driver-adapter/.../shop/<api|ui>`) reflects the new System \<boundary\> surface; the system code under `system/` reflects the new \<boundary\>; existing acceptance and contract tests still compile.

1. Update the System \<boundary\> itself under `system/` to match the ticket's checklist:
   - For API: controllers, request/response DTOs, routes, status codes, error format. Apply across **all parallel implementations** — see `docs/atdd/architecture/system.md` for the layout (Java/.NET/TS × monolith/multitier) and for where API URLs and their consumers live in each implementation. After editing the source of truth, grep the system tree for residual references (e.g. the old URL string) before moving on.
   - For UI: page structure, form fields, navigation, copy, selectors.
2. Update the matching System \<boundary\> Driver implementation (`driver-adapter/.../shop/<api|ui>`) to absorb the change. Prefer adapter-only changes — keep behaviour observable through the **existing** driver interface.
3. **Driver interface guardrail.** Do NOT modify any file under `driver-port/` casually. If you believe an interface change is unavoidable, STOP separately at that boundary and present to the user: the method(s) you want to change, why the adapter alone cannot absorb the change, the proposed new signature(s). Wait for explicit user approval before editing any `driver-port/` file. (Such changes have no contract-test fallout because this is `shop/`, not `external/` — but they still touch the test surface and must be approved.)
4. Do not modify acceptance tests, DSL, Gherkin, or any code outside the System \<boundary\> layer + its driver. `system-test/<lang>/.../Legacy/` is read-only course-reference material — leave it untouched.
5. STOP. Present the system + driver changes to the user and ask for approval. Do NOT continue.

Then proceed via the shared structural-cycle TEST, then the shared structural-cycle COMMIT procedure with phase suffix `SYSTEM API REDESIGN` or `SYSTEM UI REDESIGN`.

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

Then proceed via the shared structural-cycle TEST, then the shared structural-cycle COMMIT procedure with phase suffix `CHORE`.
