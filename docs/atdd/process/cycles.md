# ATDD Cycles

This document defines the decision flow for the ATDD pipeline. Each phase is defined in detail in its own per-phase file (`at-red-test.md`, `at-red-dsl.md`, `at-red-system-driver.md`, `at-green-system.md`, `ct-red-test.md`, `ct-red-dsl.md`, `ct-red-external-driver.md`, `ct-green-stubs.md`), with cycle-wide conventions in `at-cycle-conventions.md` / `ct-cycle-conventions.md` and the universal STOP rule in `shared-phase-progression.md` — this file controls **which phases run and in what order**.

> **Naming note**: The word *shop* appears in two distinct senses in ATDD content — `shop/` (with slash) is a package/folder convention inside the driver layer; `shop` (without slash) is the SUT repository name. See `glossary.md` for details.

## Intake (per ticket)

Before any cycle runs, the picked ticket is classified by `atdd-orchestrator` into one of six ticket types: **story**, **bug**, **system-api-task**, **system-ui-task**, **external-api-task**, or **chore**. Classification by ticket type happens first; cycle routing is decided afterwards by two orthogonal gates (see below). See `glossary.md` for the full definitions of *behavioral change*, *structural change*, and *Legacy Coverage*.

Six **ticket types** are routed through **four intake agents**:

- `atdd-story` (ticket type `story`) → reads the story's acceptance criteria; produces 1+ change-driven AC scenarios (one per acceptance criterion). Behavioral.
- `atdd-bug` (ticket type `bug`) → reads the bug's reproduction paths; produces 1+ change-driven AC scenarios (one per distinct reproduction path; default: one). Behavioral.
- `atdd-task` — single agent that handles ticket types `system-api-task`, `system-ui-task`, and `external-api-task`. The `atdd-dispatcher` passes the subtype (`system-api-redesign`, `system-ui-redesign`, or `external-system-api-change`) into `atdd-task` so the agent knows which boundary it is touching. Driver *interfaces* may grow; existing AC must keep passing through them. Single-driver scope by construction. Produces no change-driven AC scenarios. Structural.
- `atdd-chore` (ticket type `chore`) → reads the structural change description; the change is internal-only (refactor a class, rename, dependency upgrade). Drivers untouched. Produces no change-driven AC scenarios. Structural.

**Naming note — ticket type vs dispatcher subtype.** `cycles.md` and `glossary.md` use the ticket-type vocabulary (`system-api-task`, `system-ui-task`, `external-api-task`, `chore`); `atdd-dispatcher.md` and `atdd-task.md` use the subtype vocabulary (`system-api-redesign`, `system-ui-redesign`, `external-system-api-change`). The two are 1:1 — `system-api-task` ↔ `system-api-redesign`, `system-ui-task` ↔ `system-ui-redesign`, `external-api-task` ↔ `external-system-api-change`. The `redesign` / `change` form is the agent-input form passed by the dispatcher; the `-task` form is the cycle-routing form used in this doc. The COMMIT-message form is a third variant — uppercase, e.g. `SYSTEM API REDESIGN` (see `task-and-chore-cycles.md`).

In addition, **all four intake agents** produce 0+ legacy-coverage AC scenarios from the optional Legacy Coverage section in the ticket schema (see [Legacy Coverage in glossary.md](glossary.md#legacy-coverage)).

All four intake agents end with **STOP** for human approval before any cycle begins.

After STOP, two **orthogonal gates** are evaluated per ticket:

1. **Ticket has a Legacy Coverage section?** — Universal; applies to all six ticket types.
   - Yes → enter the **Legacy Coverage Cycle** (test-last; retroactive AC for already-built behavior; tests should pass on first run; **not ATDD**).
   - No → skip the Legacy Coverage Cycle.
2. **Change-driven AC produced?** — Determined by ticket type: yes for story/bug, no for system-api-task / system-ui-task / external-api-task / chore. This *is* the behavioral-vs-structural distinction.
   - Yes → enter the **AT Cycle** (test-first ATDD; Red → Green per scenario).
   - No → skip the AT Cycle.

**Order when both gates fire: Legacy Coverage Cycle first, then AT Cycle.** Rationale: fill the coverage gap before piling new behavior on top.

The full per-ticket flow is captured by the table below. The combinatorics are: **{6 ticket types} × {Legacy Coverage section: yes/no}** = **12 flows**, all of which collapse to the rule "Legacy Coverage Cycle first (if present), then the type-specific cycle, then **TICKET STATUS - IN ACCEPTANCE**."

The three Task Cycles and the Chore Cycle are all governed by the rule that **existing AC must stay green** locally before the final ticket commit. After that commit the ticket enters **TICKET STATUS - IN ACCEPTANCE** and the agent stops — see [`shared-ticket-status-in-acceptance.md`](shared-ticket-status-in-acceptance.md). Agents are CI-unaware.[^green]

[^green]: "Existing AC must stay green" — verified locally via sample-suite runs before the final commit, per `CLAUDE.md`. CI's Acceptance Stage runs the full suite afterwards but is human-watched, not agent-watched. Referenced from each structural-cycle row in the table below as `[^green]`.

**Output asymmetry — change-driven AC vs legacy-coverage AC.** The two artifact streams are produced under different rules:

- **Change-driven AC** is **ticket-type-specific** — only `atdd-story` and `atdd-bug` produce it (one scenario per acceptance criterion or per distinct reproduction path). It is the input to the AT Cycle (see below — note that the unit of work is the **ticket**, with all scenarios batched, so there is no per-scenario inner loop; see the AT Cycle section).
- **Legacy-coverage AC** is **universal-optional** — any ticket type may produce it, gated by whether the ticket schema carries a Legacy Coverage section. It is the input to the Legacy Coverage Cycle, which is **test-last** (retroactive tests for already-built behavior; tests should pass on first run; not ATDD).

| Ticket type | Agent | Class | Change-driven AC | Legacy-coverage AC | Routes to |
|-------------|-------|-------|------------------|--------------------|-----------|
| `story` | `atdd-story` | Behavioral | One scenario per acceptance criterion | 0+ scenarios if the ticket has a Legacy Coverage section | AT Cycle (always); Legacy Coverage Cycle first if the ticket has a Legacy Coverage section. |
| `bug` | `atdd-bug` | Behavioral | One scenario per distinct reproduction path (default: one) | 0+ scenarios if the ticket has a Legacy Coverage section | AT Cycle (always); Legacy Coverage Cycle first if the ticket has a Legacy Coverage section. |
| `system-api-task` | `atdd-task` (subtype `system-api-redesign`) | Structural | None | 0+ scenarios if the ticket has a Legacy Coverage section | System API Task Cycle (always); Legacy Coverage Cycle first if the ticket has a Legacy Coverage section.[^green] |
| `system-ui-task` | `atdd-task` (subtype `system-ui-redesign`) | Structural | None | 0+ scenarios if the ticket has a Legacy Coverage section | System UI Task Cycle (always); Legacy Coverage Cycle first if the ticket has a Legacy Coverage section.[^green] |
| `external-api-task` | `atdd-task` (subtype `external-system-api-change`) | Structural | None | 0+ scenarios if the ticket has a Legacy Coverage section | External API Task Cycle (always); Legacy Coverage Cycle first if the ticket has a Legacy Coverage section.[^green] |
| `chore` | `atdd-chore` | Structural | None | 0+ scenarios if the ticket has a Legacy Coverage section | Chore Cycle (always); Legacy Coverage Cycle first if the ticket has a Legacy Coverage section.[^green] |

From AT - RED - TEST onward the AT Cycle pipeline is identical regardless of which behavioral intake variant produced the scenarios. The Legacy Coverage Cycle's internal phases are TBD; see `glossary.md`.

## AT Cycle (per ticket)

_Triggered when the ticket produces change-driven AC (i.e. **change-driven AC = yes** — story or bug). Task tickets enter the matching task cycle instead — **System API Task Cycle**, **System UI Task Cycle**, or **External API Task Cycle** depending on the task subtype; chore tickets enter the **Chore Cycle** instead. See the Intake gates above and the dedicated cycle sections below._

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
3. **Provision a dockerized stand-in** following the json-server pattern established in `system/external-real-sim` (Node.js + json-server-based mock, runs in docker, mounted at a known port; the existing `external-real-sim` already emulates ERP, Tax, and Clock subsystems and is the reference shape for new stand-ins).
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

The three structural cycles (`system-api-task`, `system-ui-task`, `chore`) share one flow, parameterised by the **cycle-specific implementation step** in step 1:

```
Triggered: ticket type ∈ {system-api-task, system-ui-task, chore}
    │
    ▼
Implement change for this cycle:
    - system-api-task → Update System API Driver (interface + impl)
    - system-ui-task  → Update System UI Driver  (interface + impl)
    - chore           → Implement chore (refactor / upgrade / rename / etc.)
    │
    ▼
STOP - HUMAN REVIEW (present implementation for approval)
    │
    ▼
COMMIT: <Ticket> | <PHASE> where PHASE ∈ {SYSTEM API REDESIGN, SYSTEM UI REDESIGN, CHORE}
    │
    ▼
TICKET STATUS - IN ACCEPTANCE (see shared-ticket-status-in-acceptance.md)
```

All three are governed by the rule that **existing AC must stay green** locally before the final ticket commit.[^green] There is no per-scenario RED/GREEN (no change-driven AC is produced); the sample suite runs locally before the COMMIT to verify the change. The post-commit CI Acceptance Stage is human-watched, not agent-watched — see [`shared-ticket-status-in-acceptance.md`](shared-ticket-status-in-acceptance.md).

The External API Task Cycle is structurally similar but routes through the Contract Test Sub-Process instead of a direct Implement → Review → COMMIT step; see "External API Task Cycle" below.

---

## System API Task Cycle

_Triggered when ticket type = system-api-task (System API Driver redesign at the system boundary, no change-driven AC, no other boundaries touched)._

A System API task changes the System API at the boundary — request/response DTOs, endpoints, status codes, and the like. The System API Driver is updated to match. Driver *interfaces* may grow or change; existing acceptance tests must keep passing through them. Single-driver scope by construction (single-boundary ticket); multi-boundary work is split into multiple coordinated tickets at creation. The cycle ends with a **single COMMIT** covering the driver update.

WRITE and COMMIT mechanics (`SYSTEM API REDESIGN - WRITE` and the shared structural-cycle COMMIT with phase suffix `SYSTEM API REDESIGN`) live in [`task-and-chore-cycles.md`](task-and-chore-cycles.md). Flow: see **Structural Cycle Flow (shared)** above.

The system-api-task ticket carries a **checklist of structural change items** in its body; the agent ticks them off as the work is done, and on the final ticket commit moves the issue to **IN ACCEPTANCE** (see [`shared-ticket-status-in-acceptance.md`](shared-ticket-status-in-acceptance.md)).

---

## System UI Task Cycle

_Triggered when ticket type = system-ui-task (System UI Driver redesign at the system boundary, no change-driven AC, no other boundaries touched)._

A System UI task changes the System UI at the boundary — page structure, form fields, navigation, and the like. The System UI Driver is updated to match. Driver *interfaces* may grow or change; existing acceptance tests must keep passing through them. Single-driver scope by construction (single-boundary ticket); multi-boundary work is split into multiple coordinated tickets at creation. The cycle ends with a **single COMMIT** covering the driver update.

WRITE and COMMIT mechanics (`SYSTEM UI REDESIGN - WRITE` and the shared structural-cycle COMMIT with phase suffix `SYSTEM UI REDESIGN`) live in [`task-and-chore-cycles.md`](task-and-chore-cycles.md). Flow: see **Structural Cycle Flow (shared)** above.

The system-ui-task ticket carries a **checklist of structural change items** in its body; the agent ticks them off as the work is done, and on the final ticket commit moves the issue to **IN ACCEPTANCE** (see [`shared-ticket-status-in-acceptance.md`](shared-ticket-status-in-acceptance.md)).

---

## External API Task Cycle

_Triggered when ticket type = external-api-task (an external system changed its API; we are reacting to a third-party change, no change-driven AC of our own)._

An external system updated its API — new version, breaking change, deprecated endpoint, or similar. We update the External System Driver to match the new external surface. The work routes through the **Contract Test Sub-Process** (which itself routes through the **External System Onboarding Sub-Process** if no Driver yet exists). Single-driver scope by construction (single-boundary ticket); multi-boundary work is split into multiple coordinated tickets at creation. After CT completes its four-commit sequence, the ticket enters **TICKET STATUS - IN ACCEPTANCE** — see [`shared-ticket-status-in-acceptance.md`](shared-ticket-status-in-acceptance.md).

This cycle has no standalone WRITE / COMMIT phases of its own — all WRITE and COMMIT mechanics live in the per-phase CT docs (`ct-red-test.md`, `ct-red-dsl.md`, `ct-red-external-driver.md`, `ct-green-stubs.md`) plus `ct-cycle-conventions.md`. See [`task-and-chore-cycles.md`](task-and-chore-cycles.md) for the cross-reference.

The external-api-task ticket carries a **checklist of structural change items** in its body; the agent ticks them off as the work is done, and on the final ticket commit moves the issue to **IN ACCEPTANCE** (see [`shared-ticket-status-in-acceptance.md`](shared-ticket-status-in-acceptance.md)).

```
Triggered: ticket type = external-api-task
    │
    ▼
Contract Test Sub-Process (see above)
    │
    ▼
TICKET STATUS - IN ACCEPTANCE (see shared-ticket-status-in-acceptance.md)
```

There is no standalone STOP - HUMAN REVIEW or COMMIT in this cycle — those happen inside the Contract Test Sub-Process (which has its own per-phase STOPs and four-commit sequence). The cycle is governed by the rule that **existing AC must stay green** locally; the sample suite runs locally as part of CT's COMMIT steps. The post-commit CI Acceptance Stage is human-watched, not agent-watched — see [`shared-ticket-status-in-acceptance.md`](shared-ticket-status-in-acceptance.md).

---

## Chore Cycle

_Triggered when ticket type = chore (internal-only structural change, drivers untouched, no change-driven AC)._

A chore changes nothing at the boundary — it's an internal refactor, rename, dependency upgrade, or similar. Drivers (interfaces and implementations) are untouched. The cycle is therefore a single-step implementation followed by review, commit, and the move to **TICKET STATUS - IN ACCEPTANCE**.

WRITE and COMMIT mechanics (`CHORE - WRITE` and the shared structural-cycle COMMIT with phase suffix `CHORE`) live in [`task-and-chore-cycles.md`](task-and-chore-cycles.md). Flow: see **Structural Cycle Flow (shared)** above.

The chore ticket carries a **checklist of refactor / upgrade steps** in its body; the agent ticks them off as the work is done, and on the final ticket commit moves the issue to **IN ACCEPTANCE** (see [`shared-ticket-status-in-acceptance.md`](shared-ticket-status-in-acceptance.md)).

---

## Phase-to-Agent Mapping

| Phase | Agent | Notes |
|-------|-------|-------|
| Intake (story) | atdd-story | Behavioral. Change-driven AC: one scenario per acceptance criterion. Optional legacy-coverage AC if the ticket has a Legacy Coverage section. STOP for approval. Routes to AT Cycle (always); Legacy Coverage Cycle first if the ticket has a Legacy Coverage section. |
| Intake (bug) | atdd-bug | Behavioral. Change-driven AC: one scenario per distinct reproduction path (default: one). Optional legacy-coverage AC if the ticket has a Legacy Coverage section. STOP for approval. Routes to AT Cycle (always); Legacy Coverage Cycle first if the ticket has a Legacy Coverage section. |
| Intake (system-api-task) | `atdd-task` (subtype `system-api-redesign`) | Structural. System API redesign at the system boundary; single-driver scope; no change-driven AC. Optional legacy-coverage AC if the ticket has a Legacy Coverage section. STOP for approval. Routes to System API Task Cycle (always); Legacy Coverage Cycle first if the ticket has a Legacy Coverage section.[^green] |
| Intake (system-ui-task) | `atdd-task` (subtype `system-ui-redesign`) | Structural. System UI redesign at the system boundary; single-driver scope; no change-driven AC. Optional legacy-coverage AC if the ticket has a Legacy Coverage section. STOP for approval. Routes to System UI Task Cycle (always); Legacy Coverage Cycle first if the ticket has a Legacy Coverage section.[^green] |
| Intake (external-api-task) | `atdd-task` (subtype `external-system-api-change`) | Structural. External System API change at the system boundary; single-driver scope; no change-driven AC. Optional legacy-coverage AC if the ticket has a Legacy Coverage section. STOP for approval. Routes to External API Task Cycle (always); Legacy Coverage Cycle first if the ticket has a Legacy Coverage section.[^green] |
| Intake (chore) | `atdd-chore` | Structural. Internal-only change; no change-driven AC. Optional legacy-coverage AC if the ticket has a Legacy Coverage section. STOP for approval. Routes to Chore Cycle (always); Legacy Coverage Cycle first if the ticket has a Legacy Coverage section.[^green] |
| AT - RED - TEST | `atdd-test` | All scenarios for the ticket batched. WRITE = STOP (review tests). COMMIT = compile, conditional DSL-prototype STOP, run, disable, commit. |
| AT - RED - DSL | `atdd-dsl` | WRITE = STOP (review DSL + Driver-interface-changed flags). COMMIT = conditional Driver-prototype impl, commit. |
| AT - RED - SYSTEM DRIVER | `atdd-driver` | System Drivers only (`shop/`). WRITE = STOP, COMMIT = commit. |
| AT - GREEN - SYSTEM (backend) | `atdd-backend` | Implements backend changes for API channel. |
| AT - GREEN - SYSTEM (frontend) | `atdd-frontend` | Implements frontend changes for UI channel. |
| AT - GREEN - SYSTEM (commit + close) | `atdd-release` | Removes `@Disabled`, COMMITs the final GREEN, ticks AC checkboxes, moves the issue to **IN ACCEPTANCE** (see [`shared-ticket-status-in-acceptance.md`](shared-ticket-status-in-acceptance.md)). |
| CT - RED - TEST | `atdd-test` | WRITE = STOP, COMMIT = commit + push |
| CT - RED - DSL | `atdd-dsl` | WRITE = STOP, COMMIT = commit + push |
| CT - RED - EXTERNAL DRIVER | `atdd-driver` | External Drivers only (`external/`). WRITE = STOP, COMMIT = commit + push. |
| CT - GREEN - STUBS | _no dedicated agent — see "Stubs ownership" needs-decision in the audit plan_ | Implement External System Stubs; commit. |
| SYSTEM API REDESIGN - WRITE / COMMIT | `atdd-task` (subtype `system-api-redesign`) | WRITE = update System API + driver impl + STOP. COMMIT = shared structural-cycle COMMIT (compile, sample, commit, tick checklist). |
| SYSTEM UI REDESIGN - WRITE / COMMIT | `atdd-task` (subtype `system-ui-redesign`) | WRITE = update System UI + driver impl + STOP. COMMIT = shared structural-cycle COMMIT. |
| EXTERNAL API REDESIGN | `atdd-task` (subtype `external-system-api-change`) | No standalone WRITE/COMMIT — routes entirely through the Contract Test Sub-Process. |
| CHORE - WRITE / COMMIT | `atdd-chore` _(see "Chore agent stops at intake" needs-decision in the audit plan)_ | WRITE = implement chore + STOP. COMMIT = shared structural-cycle COMMIT. |

## STOP Behaviour

Every WRITE phase ends with **STOP** — present results to the user and wait for explicit approval before proceeding to COMMIT. The orchestrator does not auto-approve; phase progression always requires a human decision at every STOP.

## Commit Confirmation

Every COMMIT step in every cycle is gated by the rule defined in [`shared-commit-confirmation.md`](shared-commit-confirmation.md): the agent must ask "Can I commit?" and receive an explicit yes before running `git commit` (or `gh issue close`, or any other GitHub state mutation). The rule lives in its own file because it is a shared, low-level gate that leaf committing agents import directly — independent of the routing flow defined here.

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
