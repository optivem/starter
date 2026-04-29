# CT Cycle Conventions

_The Contract Test sub-process is only triggered when the AT cycle's DSL phase reports **external system interfaces changed = yes** — i.e. new methods were added to interfaces under `external/` (e.g. `driver-port/.../external/erp`). It is initiated by the orchestrator as defined in `cycles.md`._

_Before entering CT - RED - TEST, the orchestrator runs the External System Onboarding Sub-Process (see `cycles.md`) to ensure an External System Driver and accessible Test Instance exist. If the Driver already exists, Onboarding returns immediately; otherwise it provisions a dockerized stand-in (json-server pattern, see `system/external-real-sim`), defines a minimal Driver interface and implementation, and proves it works with a single Smoke Test._

## Suite Selection

Each contract test runs against two parallel suites — the real external-system Test Instance and the dockerized stub. Use the matching suite placeholder throughout all CT phases:

- `<suite-contract-real>` — the contract suite executed against the **real external system** (Test Instance).
- `<suite-contract-stub>` — the contract suite executed against the **dockerized stub** External System.

A CT phase that names only one of these placeholders is exercising one side of the real-vs-stub contract pair; CT - RED - TEST runs both, CT - RED - DSL and CT - GREEN - STUBS run the stub side only.

## Commit Message Format

Every commit message follows the pattern: `<Ticket> | <Phase>`.

The unit of work in the CT sub-process is a **ticket**, not an individual scenario — CT is entered once per ticket when AT's DSL phase reports external system interfaces changed = yes, and all CT phases run as a single per-ticket pass (mirroring the AT Cycle batching rule). Commit messages reflect the ticket title.

If a GitHub issue number was provided as input, prefix every commit message with `#<issue-number> | `. Example: `#42 | Register Customer | CT - RED - TEST`.

**Important:** The phase suffix in the message is the phase *prefix only* (e.g. `CT - RED - TEST`). Do **NOT** append `- WRITE`, `- REVIEW`, or `- COMMIT` to the phase in the commit message — those suffixes identify the section header only, not the commit message. (REVIEW is a STOP-only phase that produces no commit.)
