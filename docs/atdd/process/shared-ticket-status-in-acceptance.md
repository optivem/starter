# TICKET STATUS - IN ACCEPTANCE

A shared, post-commit ticket state. After the **final commit of a ticket**, the ticket is moved to status **IN ACCEPTANCE**. This is the **maximum status any agent ever sets** — agents never advance a ticket past IN ACCEPTANCE.

This rule is intentionally separate from `cycles.md` (which decides which phases run) and from `shared-commit-confirmation.md` (which gates the act of committing). This file defines what happens *at the end of the final ticket commit*.

## Agents are CI-unaware

Agents do not observe, wait on, or react to the CI Acceptance Stage. The verifier is CI; the watcher is the human. Specifically, agents do **not**:

- Wait for the Acceptance Stage to complete.
- Fix breakage when the Acceptance Stage goes red.
- Move tickets to DONE.

If the Acceptance Stage breaks after a ticket has been moved to IN ACCEPTANCE, the human decides what to do — manual fix, new ticket, or re-invocation of the pipeline. None of that is agent-driven.

Nothing in this procedure is CI-gated. Both the checklist ticking and the status transition happen **before** CI runs, on the local-green completion of the final ticket commit.

## When the ticket enters IN ACCEPTANCE

Immediately after the **final ticket commit** has been pushed:

- **AT Cycle (story / bug)** — after `AT - GREEN - SYSTEM - COMMIT` (the `atdd-release` commit that re-enables tests and pushes the final GREEN).
- **System API Task / System UI Task / Chore** — after the single `<Ticket> | <PHASE>` commit produced by the shared structural-cycle COMMIT procedure in `task-and-chore-cycles.md`.
- **External API Task** — after the final commit of the Contract Test Sub-Process (`CT - GREEN - STUBS`).
- **Legacy Coverage Cycle** — after its terminal commit (TBD; see `glossary.md`).
- **External System Onboarding Sub-Process** — after its `External System Onboarding | <External System Name>` commit.

Per-phase intermediate commits (e.g. `AT - RED - TEST`, `CT - RED - DSL`) do **not** trigger this status change — only the commit that ends the ticket does.

## Procedure (agent side)

This procedure runs **without re-asking the user**. The COMMIT immediately before it was already gated by [`shared-commit-confirmation.md`](shared-commit-confirmation.md); the steps below are routine post-commit bookkeeping. The agent just performs them and informs the user afterwards.

1. Tick the ticket's checklist items completed by this work (acceptance-criterion checkboxes for behavioral cycles; structural-change checklist items for task / chore cycles). Not CI-gated.
2. Move the GitHub issue to status **IN ACCEPTANCE** in the project board.
3. Stop. The ticket is out of agent scope.

## Beyond IN ACCEPTANCE (human responsibility)

Pipeline-watching, fix-loops on red CI, and the move from IN ACCEPTANCE to DONE are human responsibilities. Agents end the ticket at IN ACCEPTANCE; they have no awareness of what CI does next.
