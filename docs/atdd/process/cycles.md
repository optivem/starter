# ATDD Cycles

This document defines the decision flow for the ATDD pipeline. Each phase is defined in detail in its own per-phase file (`at-red-test.md`, `at-red-dsl.md`, `at-red-system-driver.md`, `at-green-system.md`, `ct-red-test.md`, `ct-red-dsl.md`, `ct-red-external-driver.md`, `ct-green-stubs.md`), with cycle-wide conventions in `at-cycle-conventions.md` / `ct-cycle-conventions.md` and the universal STOP rule in `shared-phase-progression.md` — this file controls **which phases run and in what order**.

> **Naming note**: The word *shop* appears in two distinct senses in ATDD content — `shop/` (with slash) is a package/folder convention inside the driver layer; `shop` (without slash) is the SUT repository name. See `glossary.md` for details.

## Intake (per ticket)

Before any cycle runs, the picked ticket is classified by the `gh-optivem` Go runtime (the deterministic fast path in `internal/atdd/runtime/classify`) into one of three native GitHub issue types — **Story**, **Bug**, or **Task** — and Task tickets carry one of three `subtype:*` labels: `subtype:system-interface-redesign`, `subtype:external-system-interface-redesign`, or `subtype:system-implementation-change`. Classification happens first; cycle routing is decided afterwards by two orthogonal gates (see below). See `glossary.md` for the full definitions of *behavioral change*, *structural change*, and *Legacy Coverage*.

The three ticket types are routed by intake:

- **Story** → reads the story's acceptance criteria; produces 1+ change-driven AC scenarios (one per acceptance criterion). Behavioral.
- **Bug** → reads the bug's reproduction paths; produces 1+ change-driven AC scenarios (one per distinct reproduction path; default: one). Behavioral.
- **Task** → reads the structural change description and the `subtype:*` label. The runtime passes the subtype value (`system-interface-redesign`, `external-system-interface-redesign`, or `system-implementation-change`) into the structural-cycle dispatch so downstream nodes know which boundary (or no boundary, for `system-implementation-change`) is touched. For interface-change subtypes, driver *implementations* may grow; existing AC must keep passing through unchanged driver interfaces. Produces no change-driven AC scenarios. Structural.

**Naming note — subtype labels vs commit-message phase.** The `subtype:*` label is the runtime-input form (e.g. `subtype:system-interface-redesign`). The COMMIT-message form is uppercase (e.g. `SYSTEM INTERFACE REDESIGN`); see `task-and-chore-cycles.md`. Internally, `subtype:system-implementation-change` maps to the commit suffix `CHORE` for backwards compatibility with existing commit conventions.

In addition, **intake on every ticket type** may produce 0+ legacy-coverage AC scenarios from the optional Legacy Coverage section in the ticket schema (see [Legacy Coverage in glossary.md](glossary.md#legacy-coverage)).

Intake ends with **STOP** for human approval before any cycle begins.

After STOP, two **orthogonal gates** are evaluated per ticket:

1. **Ticket has a Legacy Coverage section?** — Universal; applies to every ticket type.
   - Yes → enter the **Legacy Coverage Cycle** (test-last; retroactive AC for already-built behavior; tests should pass on first run; **not ATDD**).
   - No → skip the Legacy Coverage Cycle.
2. **Change-driven AC produced?** — Determined by ticket type: yes for Story / Bug, no for Task. This *is* the behavioral-vs-structural distinction.
   - Yes → enter the **AT Cycle** (test-first ATDD; Red → Green per scenario).
   - No → skip the AT Cycle.

**Order when both gates fire: Legacy Coverage Cycle first, then AT Cycle.** Rationale: fill the coverage gap before piling new behavior on top.

The full per-ticket flow is captured by the table below. The combinatorics are: **{Story / Bug / Task × (3 subtypes for Task)} × {Legacy Coverage section: yes/no}** — all of which collapse to the rule "Legacy Coverage Cycle first (if present), then the type/subtype-specific cycle, then **TICKET STATUS - IN ACCEPTANCE**."

The structural cycles are all governed by the rule that **existing AC must stay green** locally before the final ticket commit. After that commit the ticket enters **TICKET STATUS - IN ACCEPTANCE** and the agent stops — see [`shared-ticket-status-in-acceptance.md`](shared-ticket-status-in-acceptance.md). Agents are CI-unaware.[^green]

[^green]: "Existing AC must stay green" — verified locally via sample-suite runs before the final commit, per `CLAUDE.md`. CI's Acceptance Stage runs the full suite afterwards but is human-watched, not agent-watched. Referenced from each structural-cycle row in the table below as `[^green]`.

**Output asymmetry — change-driven AC vs legacy-coverage AC.** The two artifact streams are produced under different rules:

- **Change-driven AC** is **ticket-type-specific** — only Story and Bug produce it (one scenario per acceptance criterion or per distinct reproduction path). It is the input to the AT Cycle (see below — note that the unit of work is the **ticket**, with all scenarios batched, so there is no per-scenario inner loop; see the AT Cycle section).
- **Legacy-coverage AC** is **universal-optional** — any ticket type may produce it, gated by whether the ticket schema carries a Legacy Coverage section. It is the input to the Legacy Coverage Cycle, which is **test-last** (retroactive tests for already-built behavior; tests should pass on first run; not ATDD).

| Ticket type | Subtype label (Task only) | Class | Change-driven AC | Legacy-coverage AC | Routes to |
|-------------|---------------------------|-------|------------------|--------------------|-----------|
| Story | — | Behavioral | One scenario per acceptance criterion | 0+ scenarios if the ticket has a Legacy Coverage section | AT Cycle (always); Legacy Coverage Cycle first if the ticket has a Legacy Coverage section. |
| Bug | — | Behavioral | One scenario per distinct reproduction path (default: one) | 0+ scenarios if the ticket has a Legacy Coverage section | AT Cycle (always); Legacy Coverage Cycle first if the ticket has a Legacy Coverage section. |
| Task | `subtype:system-interface-redesign` | Structural | None | 0+ scenarios if the ticket has a Legacy Coverage section | Driver Adapter Cycle → `structural_cycle` (always); Legacy Coverage Cycle first if the ticket has a Legacy Coverage section.[^green] |
| Task | `subtype:external-system-interface-redesign` | Structural | None | 0+ scenarios if the ticket has a Legacy Coverage section | Driver Adapter Cycle → Contract Test Sub-Process (always); Legacy Coverage Cycle first if the ticket has a Legacy Coverage section.[^green] |
| Task | `subtype:system-implementation-change` | Structural | None | 0+ scenarios if the ticket has a Legacy Coverage section | System Under Test Cycle (always); Legacy Coverage Cycle first if the ticket has a Legacy Coverage section.[^green] |

From AT - RED - TEST onward the AT Cycle pipeline is identical regardless of which behavioral intake variant produced the scenarios. The Legacy Coverage Cycle's internal phases are TBD; see `glossary.md`.

## AT Cycle (per ticket)

_Triggered when the ticket produces change-driven AC (i.e. **change-driven AC = yes** — Story or Bug). Task tickets enter the matching structural cycle instead — the **Driver Adapter Cycle** (for `subtype:system-interface-redesign` or `subtype:external-system-interface-redesign`) or the **System Under Test Cycle** (for `subtype:system-implementation-change`). See the Intake gates above and the dedicated cycle sections below._

The unit of work in the AT Cycle is the **ticket** — all change-driven AC scenarios for the ticket are batched through each phase together. AT - RED - TEST writes all scenarios at once; AT - RED - DSL, AT - RED - SYSTEM DRIVER, and AT - GREEN - SYSTEM each operate over the full set. There is no per-scenario inner loop.

```
AT - RED - TEST
    │
    ├── DSL Interface Changed? ──── No ──→ AT - GREEN - SYSTEM
    │
    Yes
    ▼
AT - RED - DSL
    │
    ├── External System Driver Interface Changed? ──── Yes ──→ Contract Test Sub-Process (see below)
    │                                                                │
    │                                                                ▼
    │                                                          (then continue ↓)
    │
    ├── System Driver Interface Changed? ──── No ──→ AT - GREEN - SYSTEM
    │
    Yes
    ▼
AT - RED - SYSTEM DRIVER
    │
    ▼
AT - GREEN - SYSTEM
    │
    ▼
TICKET STATUS - IN ACCEPTANCE (see shared-ticket-status-in-acceptance.md)
```

### Decision criteria

- **DSL Interface Changed?** — Did AT - RED - TEST need to extend the DSL with new methods (and therefore add `"TODO: DSL"` prototypes)? Determined by compile failure during AT - RED - TEST - COMMIT. If no new DSL methods were needed, the answer is No.
- **External System Driver Interface Changed?** — Did AT - RED - DSL add or modify any external-system Driver interface? See `glossary.md` for the definition of *interface change*. Set as an explicit flag at the end of AT - RED - DSL - WRITE.
- **System Driver Interface Changed?** — Did AT - RED - DSL add or modify any system Driver interface? Set as an explicit flag at the end of AT - RED - DSL - WRITE.

---

## Contract Test Sub-Process

_Triggered when the AT cycle detects external driver interface changes._

Before entering CT - RED - TEST, the orchestrator runs the **External System Onboarding Sub-Process** (see below) as a prerequisite to ensure an External System Driver and an accessible Test Instance exist for the system being integrated. If the Driver already exists, Onboarding returns immediately; otherwise it provisions a dockerized stand-in, defines a minimal Driver interface and implementation, and proves it works with a single Smoke Test before CT - RED - TEST begins.

```
External System Onboarding Sub-Process (see below)
    │
    ▼
CT - RED - TEST
    │
    ├── DSL Interface Changed? ──── No ──→ CT - GREEN - STUBS
    │
    Yes
    ▼
CT - RED - DSL
    │
    ├── External System Driver Interface Changed? ──── No ──→ CT - GREEN - STUBS
    │
    Yes
    ▼
CT - RED - EXTERNAL DRIVER
    │
    ▼
CT - GREEN - STUBS
```

After the contract test sub-process completes, return to the AT cycle and continue with the system driver check.

---

## External System Onboarding Sub-Process

_Triggered when the orchestrator is about to enter the Contract Test Sub-Process and needs to ensure an External System Driver and Test Instance exist for the system being integrated._

This is a one-time-per-external-system sub-process that handles the prerequisites for contract testing. It separates two orthogonal concerns from the per-scenario CT loop:

1. **Code question** — do we already have an External System Driver (interface + impl) for this system?
2. **Environment question** — do we have an accessible Test Instance to talk to (real sandbox or dockerized stand-in)?

These are independent: a Driver may exist without an accessible Test Instance, or vice versa. The sub-process resolves both before per-scenario contract testing starts.

### Steps

1. **Check whether an External System Driver exists** for the system being integrated (interface + impl under `external/`). If yes → return immediately to the Contract Test Sub-Process; skip the rest of onboarding.
2. **Check whether an External System Test Instance is accessible** (real sandbox or already-running dockerized stand-in). If yes → skip step 3.
3. **Provision a dockerized stand-in** following the json-server pattern established in `external-systems/simulators` (Node.js + json-server-based mock, runs in docker, mounted at a known port; the existing `simulators` directory already emulates ERP, Tax, and Clock subsystems and is the reference shape for new stand-ins).
4. **Define a minimal External System Driver interface** — only the methods needed to support a single Smoke Test for this external system. Resist the urge to flesh out the full surface area; per-scenario interface growth happens in the CT loop.
5. **Implement the Driver impl just enough for one Smoke Test** to compile and run against either the real Test Instance or the dockerized stand-in.
6. **Write a single Smoke Test** for this External System.
7. **Run the Smoke Test and verify it passes.** If it fails, ask the user for support and STOP. Do NOT continue.
8. **STOP — HUMAN REVIEW.** Present the dockerized stand-in (if newly provisioned), the minimal Driver interface, the Driver impl, and the Smoke Test for approval. Do NOT continue.
9. **COMMIT** with message `External System Onboarding | <External System Name>`.
10. **Return to the Contract Test Sub-Process** at CT - RED - TEST.

The Onboarding sub-process internally handles the "Driver already exists" early return, so the Contract Test Sub-Process can refer to it unconditionally; no per-entry diamond is needed at the CT layer.

---

## Structural Cycle Flow (shared)

The structural paths share one flow, parameterised by the **cycle-specific implementation step** in step 1:

```
Triggered: Task ticket with subtype ∈ {system-interface-redesign, system-implementation-change}
    │
    ▼
WRITE — implement change for this cycle:
    - subtype:system-interface-redesign       → Update the System Driver(s) the ticket targets (interface + impl)
    - subtype:system-implementation-change    → Implement the internal change (refactor / upgrade / rename / etc.)
    │
    ▼
REVIEW — STOP, present implementation for human approval
    │
    ▼
TEST — compile + sample suite (entire phase gated upfront — ask user to choose full / compile / skip; run nothing until the choice arrives)
    │
    ▼
COMMIT — <Ticket> | <PHASE> where PHASE ∈ {SYSTEM INTERFACE REDESIGN, CHORE}
    │
    ▼
TICKET STATUS - IN ACCEPTANCE (see shared-ticket-status-in-acceptance.md)
```

Both are governed by the rule that **existing AC must stay green** locally before the final ticket commit.[^green] There is no per-scenario RED/GREEN (no change-driven AC is produced); the sample suite runs locally during the TEST phase (after REVIEW, before COMMIT) to verify the change, gated by explicit user approval. The post-commit CI Acceptance Stage is human-watched, not agent-watched — see [`shared-ticket-status-in-acceptance.md`](shared-ticket-status-in-acceptance.md).

The `subtype:external-system-interface-redesign` path is structurally similar but routes through the Contract Test Sub-Process instead of a direct WRITE → REVIEW → TEST → COMMIT sequence; see "Driver Adapter Cycle — external-system path" below.

---

## Driver Adapter Cycle

_Triggered when a Task ticket carries `subtype:system-interface-redesign` or `subtype:external-system-interface-redesign`. Both subtypes touch a Driver Adapter; the runtime routes them differently inside this cycle._

The Driver Adapter Cycle (`da_cycle` in the runtime) splits on the `subtype:*` label:

- **`subtype:system-interface-redesign`** — a system-side Driver Adapter we own (API, UI, mobile, CLI, admin, …). Routes through the **shared structural cycle** (WRITE → REVIEW → TEST → COMMIT). The WRITE agent reads the Checklist and the system tree to figure out which driver(s) to modify; the framework no longer pre-classifies the channel.
- **`subtype:external-system-interface-redesign`** — an External System Driver Adapter (we are reacting to a third-party API change). Routes through the **Contract Test Sub-Process** (which itself wraps the **External System Onboarding Sub-Process** if no Driver yet exists). After CT completes its four-commit sequence, the ticket enters **TICKET STATUS - IN ACCEPTANCE** — see [`shared-ticket-status-in-acceptance.md`](shared-ticket-status-in-acceptance.md).

### Driver Adapter Cycle — system-side path

A system-side interface change reshapes the System surface — API endpoints/DTOs/status codes, UI page structure/forms/copy/selectors, mobile screens, CLI commands, admin pages, or whatever channel the ticket targets. The relevant System Driver(s) are updated to match. Driver *interfaces* may grow or change; existing acceptance tests must keep passing through them. The cycle ends with a **single COMMIT** covering the driver update.

WRITE / REVIEW mechanics live in [`system-interface-redesign.md`](system-interface-redesign.md). TEST and COMMIT use the shared structural-cycle procedures in [`task-and-chore-cycles.md`](task-and-chore-cycles.md) (commit suffix `SYSTEM INTERFACE REDESIGN`). Flow: see **Structural Cycle Flow (shared)** above.

The Task ticket carries a **checklist of structural change items** in its body; the agent ticks them off as the work is done, and on the final ticket commit moves the issue to **IN ACCEPTANCE** (see [`shared-ticket-status-in-acceptance.md`](shared-ticket-status-in-acceptance.md)).

### Driver Adapter Cycle — external-system path

An external system updated its API — new version, breaking change, deprecated endpoint, or similar. We update the External System Driver to match the new external surface. The work routes through the **Contract Test Sub-Process** (which itself routes through the **External System Onboarding Sub-Process** if no Driver yet exists).

This path has no standalone WRITE / COMMIT phases of its own — all WRITE and COMMIT mechanics live in the per-phase CT docs (`ct-red-test.md`, `ct-red-dsl.md`, `ct-red-external-driver.md`, `ct-green-stubs.md`) plus `ct-cycle-conventions.md`. See [`task-and-chore-cycles.md`](task-and-chore-cycles.md) for the cross-reference.

The Task ticket carries a **checklist of structural change items** in its body; the agent ticks them off as the work is done, and on the final ticket commit moves the issue to **IN ACCEPTANCE** (see [`shared-ticket-status-in-acceptance.md`](shared-ticket-status-in-acceptance.md)).

```
Triggered: Task ticket with subtype:external-system-interface-redesign
    │
    ▼
Contract Test Sub-Process (see above)
    │
    ▼
TICKET STATUS - IN ACCEPTANCE (see shared-ticket-status-in-acceptance.md)
```

There is no standalone STOP - HUMAN REVIEW or COMMIT in this path — those happen inside the Contract Test Sub-Process (which has its own per-phase STOPs and four-commit sequence). The path is governed by the rule that **existing AC must stay green** locally; the sample suite runs locally as part of CT's COMMIT steps. The post-commit CI Acceptance Stage is human-watched, not agent-watched — see [`shared-ticket-status-in-acceptance.md`](shared-ticket-status-in-acceptance.md).

---

## System Under Test Cycle

_Triggered when a Task ticket carries `subtype:system-implementation-change` (internal-only structural change, drivers untouched, no change-driven AC)._

A `system-implementation-change` ticket changes nothing at the boundary — it's an internal refactor, rename, dependency upgrade, or similar. Drivers (interfaces and implementations) are untouched. The cycle (`sut_cycle` in the runtime) is therefore a single-step implementation followed by review, test, commit, and the move to **TICKET STATUS - IN ACCEPTANCE**.

WRITE, TEST, and COMMIT mechanics (`CHORE - WRITE`, the shared structural-cycle TEST, and the shared structural-cycle COMMIT with phase suffix `CHORE`) live in [`task-and-chore-cycles.md`](task-and-chore-cycles.md). Flow: see **Structural Cycle Flow (shared)** above.

The Task ticket carries a **checklist of refactor / upgrade steps** in its body; the agent ticks them off as the work is done, and on the final ticket commit moves the issue to **IN ACCEPTANCE** (see [`shared-ticket-status-in-acceptance.md`](shared-ticket-status-in-acceptance.md)).

---

## Phase-to-Agent Mapping

| Phase | Agent | Notes |
|-------|-------|-------|
| Intake (Story) | parser (no agent) | Behavioral. Change-driven AC: one scenario per acceptance criterion. Optional legacy-coverage AC if the ticket has a Legacy Coverage section. STOP for approval. Routes to AT Cycle (always); Legacy Coverage Cycle first if the ticket has a Legacy Coverage section. |
| Intake (Bug) | parser (no agent) | Behavioral. Change-driven AC: one scenario per distinct reproduction path (default: one). Optional legacy-coverage AC if the ticket has a Legacy Coverage section. STOP for approval. Routes to AT Cycle (always); Legacy Coverage Cycle first if the ticket has a Legacy Coverage section. |
| Intake (Task, `subtype:system-interface-redesign`) | parser (no agent) | Structural. System-side interface change; no change-driven AC. Optional legacy-coverage AC if the ticket has a Legacy Coverage section. STOP for approval. Routes to Driver Adapter Cycle → `structural_cycle` (always); Legacy Coverage Cycle first if the ticket has a Legacy Coverage section.[^green] |
| Intake (Task, `subtype:external-system-interface-redesign`) | parser (no agent) | Structural. External-system interface change; no change-driven AC. Optional legacy-coverage AC if the ticket has a Legacy Coverage section. STOP for approval. Routes to Driver Adapter Cycle → Contract Test Sub-Process (always); Legacy Coverage Cycle first if the ticket has a Legacy Coverage section.[^green] |
| Intake (Task, `subtype:system-implementation-change`) | parser (no agent) | Structural. Internal-only change; no change-driven AC. Optional legacy-coverage AC if the ticket has a Legacy Coverage section. STOP for approval. Routes to System Under Test Cycle (always); Legacy Coverage Cycle first if the ticket has a Legacy Coverage section.[^green] |
| AT - RED - TEST | `atdd-test` | All scenarios for the ticket batched. WRITE = write tests. REVIEW = STOP for human approval. COMMIT = compile, conditional DSL-prototype STOP, run, disable, commit. |
| AT - RED - DSL | `atdd-dsl` | WRITE = implement DSL + Driver-interface-changed flags. REVIEW = STOP for human approval. COMMIT = conditional Driver-prototype impl, commit. |
| AT - RED - SYSTEM DRIVER | `atdd-driver` | System Drivers only (`shop/`). WRITE = implement System Drivers, run tests. REVIEW = STOP. COMMIT = commit. |
| AT - GREEN - SYSTEM - WRITE (backend) | `atdd-backend` | Implements backend changes for API channel; one slice of the WRITE phase. |
| AT - GREEN - SYSTEM - WRITE (frontend) | `atdd-frontend` | Implements frontend changes for UI channel; the other slice of the WRITE phase. |
| AT - GREEN - SYSTEM - REVIEW | _orchestrator_ | STOP for human approval after both backend and frontend slices of WRITE complete. No agent invoked; the orchestrator runs the gate directly. |
| AT - GREEN - SYSTEM - COMMIT | `atdd-release` | Removes `@Disabled`, COMMITs the final GREEN, ticks AC checkboxes, moves the issue to **IN ACCEPTANCE** (see [`shared-ticket-status-in-acceptance.md`](shared-ticket-status-in-acceptance.md)). |
| CT - RED - TEST | `atdd-test` | WRITE = write contract tests, run real (pass) + stub (fail), disable. REVIEW = STOP. COMMIT = commit + push. |
| CT - RED - DSL | `atdd-dsl` | WRITE = implement DSL + flag. REVIEW = STOP. COMMIT = commit + push. |
| CT - RED - EXTERNAL DRIVER | `atdd-driver` | External Drivers only (`external/`). WRITE = implement External Drivers, run tests. REVIEW = STOP. COMMIT = commit + push. |
| CT - GREEN - STUBS | _no dedicated agent — see "Stubs ownership" needs-decision in the audit plan_ | WRITE = implement Stubs, run tests. REVIEW = STOP. COMMIT = commit. |
| SYSTEM INTERFACE REDESIGN - WRITE / REVIEW / TEST / COMMIT | `atdd-task` (subtype `system-interface-redesign`) | WRITE = update the targeted System Driver(s) + adapter impl(s) (channel determined by Checklist). REVIEW = STOP for human approval. TEST = shared structural-cycle TEST (compile, ask-then-sample, STOP). COMMIT = shared structural-cycle COMMIT (commit + tick checklist + status move). See [`system-interface-redesign.md`](system-interface-redesign.md). |
| (external-system path) | `atdd-task` (subtype `external-system-interface-redesign`) | No standalone WRITE / REVIEW / TEST / COMMIT — routes entirely through the Contract Test Sub-Process. |
| CHORE - WRITE / REVIEW / TEST / COMMIT | `atdd-chore` (subtype `system-implementation-change`) | WRITE = implement the internal change. REVIEW = STOP. TEST = shared structural-cycle TEST. COMMIT = shared structural-cycle COMMIT (suffix `CHORE`). |

## Scope

Every pipeline run is bounded by a **Scope** declared in `gh-optivem.yaml` at the repo root (loaded by gh-optivem's `internal/projectconfig` package). Scope has three axes — **Architecture** (`monolith` | `multitier`), **System Lang** (`java` | `dotnet` | `typescript`), and **Test Lang** (same enum) — and is propagated into every dispatched sub-agent prompt as `${architecture}`, `${system_lang}`, `${test_lang}`.

Each invocation targets one combination of values (the schema does not accept `both` or `all`). To run against a different combination, point the CLI at an alternate config with `--config <path>` (e.g. `gh optivem atdd implement-ticket --issue 42 --config gh-optivem-multitier.yaml`).

Sub-agents — notably `atdd-task` and `atdd-chore` — restrict ALL file edits, residual-reference greps, compile checks, and sample-suite runs to in-scope paths. The shared structural-cycle TEST procedure (see `task-and-chore-cycles.md`) runs the sample suite only for the in-scope Test Lang and prints a drift warning naming any out-of-scope implementations that were deliberately left untouched.

## STOP Behaviour

Every cycle separates the work step (**WRITE**) from the human-approval step (**REVIEW**). REVIEW is a STOP-only phase: the agent does no further work, just presents what WRITE produced and waits for explicit approval. The orchestrator does not auto-approve; phase progression always requires a human decision at every STOP.

Structural cycles add a second STOP at the end of **TEST** (after the chosen checks complete, before COMMIT) so the user can review the test results before commit confirmation. **The entire TEST phase is gated upfront** — the agent asks the user to choose `full` (compile + sample), `compile` (compile only), or `skip` before running anything. Compile is not silent; even single-project compile commands like `./gradlew build` or `npx tsc --noEmit` require approval. See `task-and-chore-cycles.md` for the procedure. AT and CT cycles do not have a standalone TEST phase — test execution is folded into WRITE (the agent runs the tests as part of doing the work) and the relevant verification is repeated inside COMMIT.

## Commit Handoff

Agents never run `git commit`, `git add`, or `gh issue close`. When the work is done, the agent exits cleanly with the working-tree delta untouched. The wrapping CLI that invoked the dispatch then runs the post-dispatch human gates (the "Can I commit?" prompt and any phase-boundary STOPs) and owns the actual commit. This separation keeps the commit-approval moment outside the agent's transcript so the human reviews the staged delta with normal review tools rather than scrolling back through the agent's reasoning.

Per-phase docs name the commit-message format (`<Ticket> | <Phase>`) — the wrapping CLI applies it. The agent's only commit-related responsibility is to leave the working tree in a state that produces the right commit when the wrapper stages and commits it.

## Resume Detection

Scan for `@Disabled` annotations to determine where to resume:

| Marker | Resume at |
|--------|-----------|
| `AT - RED - TEST` | Check for `"TODO: DSL"` prototypes → if found, AT - RED - DSL; if not, AT - GREEN - SYSTEM |
| `AT - RED - DSL` | Check the `External System Driver Interface Changed` and `System Driver Interface Changed` flags from the WRITE phase: if external = yes, enter the Contract Test Sub-Process; otherwise if system = yes, AT - RED - SYSTEM DRIVER; otherwise AT - GREEN - SYSTEM. If the flags were not preserved, fall back to checking for `"TODO: Driver"` prototypes — found in `external/` → CT sub-process; found in `shop/` → AT - RED - SYSTEM DRIVER; not found → AT - GREEN - SYSTEM. |
| `AT - RED - SYSTEM DRIVER` | AT - GREEN - SYSTEM |
| `CT - RED - TEST` | Check for `"TODO: DSL"` prototypes → if found, CT - RED - DSL; if not, CT - GREEN - STUBS |
| `CT - RED - DSL` | Check for `"TODO: Driver"` prototypes in external drivers → if found, CT - RED - EXTERNAL DRIVER; if not, CT - GREEN - STUBS |
| `CT - RED - EXTERNAL DRIVER` | CT - GREEN - STUBS |

## Escalation

If any agent reports it cannot proceed (stuck, unexpected pattern, test failure it cannot explain), STOP and present the blocker to the user.
