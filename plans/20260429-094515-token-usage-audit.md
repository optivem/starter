# 20260429-094515 — ATDD Token Usage Audit Plan

🤖 **Picked up by agent** — `ValentinaLaptop` at `2026-04-29T11:46:37Z`

Auto-applicable items (Agent-prompt edits #1–#2, Doc edits #1–#7) executed on 2026-04-29. Remaining items below all require a user decision and were not auto-applied.

## Next steps (read this first)

The two open audit plans (`plans/20260429-094515-token-usage-audit.md` + `plans/20260429-100633-process-audit.md`) have overlapping decision items. The cleanest order to clear them is:

**(A) Cross-cutting design decisions** — each unblocks multiple plan items:

1. `atdd-chore` intake-only vs full-cycle? (token-usage **NDC #1** + process-audit **NDC #2**)
2. Who owns `CT - GREEN - STUBS`? Create `atdd-stub`, fold into `atdd-driver`, or fold into `atdd-test`? (process-audit **NDC #1**)
3. `<Ticket>` vs `<Scenario>` in CT commit messages — does CT batch per ticket or loop per scenario? (process-audit **NDC #5** + deferred process-rule-change **#10**)
4. Dangling TODO at `at-green-system.md:36` — delete or fill with the legacy-handling rule (token-usage **stale #2** + process-audit deferred **#9**)

**Recommended order:** start with **A1** — highest leverage. Answering it unblocks the structural-cycle Phase-to-Agent rows in `cycles.md` (currently punting to a needs-decision), lets the token-usage `@include` cleanup finally land (~112 LE/ticket if intake-only), and frames the answer to **A3** by making the unit-of-work explicit.

## Needs-decision — tradeoffs (NOT auto-applied)

### 1. Is `atdd-chore.md` an intake-only agent or does it execute the Chore Cycle?

**Observation:** `atdd-chore.md` body (lines 14-28) reads like an intake agent — it classifies, scans for legacy coverage, presents to the user, and STOPs. Yet it `@include`s `shared-commit-confirmation.md`, `shared-phase-progression.md`, and `task-and-chore-cycles.md` — docs that only matter if the agent actually executes WRITE / COMMIT phases. By symmetry, the comparable intake-only agents `atdd-story.md` and `atdd-bug.md` `@include` nothing.

**Tradeoff:**
- **Option A — Intake-only.** Match the symmetric design with `atdd-story` / `atdd-bug`. Drop the three `@include`s, saving ~28 + 3 + 81 = ~112 lines per chore ticket. Some other agent (orchestrator? a future `atdd-chore-executor`?) runs CHORE - WRITE / CHORE - COMMIT.
- **Option B — Intake + execution.** `atdd-chore.md` does the full cycle. Keep the includes; add explicit body text describing CHORE - WRITE → CHORE - COMMIT mechanics (currently absent — body STOPs at step 7 line 28).

**Question for the user:** Which design is intended? If A, who runs CHORE - WRITE / COMMIT? `cycles.md` Phase-to-Agent Mapping does not list any agent for the Chore Cycle (or for any of the three Task Cycles).

---

### 2. Is `atdd-task.md` similarly intake-or-execution?

**Observation:** Same shape as `atdd-chore` but more ambiguous. `atdd-task.md` body lines 31-60 do describe execution mechanics (identify the changing layer, implement change, run suites, report). It includes `task-and-chore-cycles.md`, which has the WRITE/COMMIT phases. Yet the body never quotes `task-and-chore-cycles.md`'s phase names (`SYSTEM API REDESIGN - WRITE`, etc.) — it describes its own steps in parallel.

**Tradeoff:**
- **Option A — Make `atdd-task.md` body authoritative.** Drop the include of `task-and-chore-cycles.md` from `atdd-task.md`. The body already contains the full process (steps 1-7). Savings ~81 lines. But `task-and-chore-cycles.md` then has only one consumer (`atdd-chore.md`) — and per Needs-decision #1 that consumer might be wrong too.
- **Option B — Make `task-and-chore-cycles.md` authoritative.** Compress `atdd-task.md` body to "follow `task-and-chore-cycles.md` for the appropriate boundary" + the three lines of subtype-mapping. Savings ~30 lines from agent body.
- **Option C — Status quo.** Both forms exist; the agent body is the operational summary, the doc is the canonical phase definition. Mild duplication is the cost of clarity.

**Question for the user:** Which form is the source of truth for structural-cycle execution mechanics — the agent body or the doc?

---

### 3. Should `atdd-task.md` keep `@include` of the entire `glossary.md`?

**Observation:** `atdd-task.md` references one term from the glossary — *interface change* (line 44, in the driver-interface guardrail). Including the whole 62-line glossary to support one cross-reference is high-cost.

**Tradeoff:**
- **Option A — Keep the include.** Glossary is small (62 lines), and the agent might reference other terms transitively (e.g. *structural change*, *interface change*, *Legacy Coverage*).
- **Option B — Drop the include + inline the one-sentence definition.** Replace `@docs/atdd/process/glossary.md` with one inline footnote: "*Interface change*: any modification to a public contract between layers — methods, signatures, or DTO fields under `driver-port/`."  Savings ~62 lines per task-ticket invocation.
- **Option C — Split the glossary.** Pull "Interface Change" into its own small file; let `atdd-task` include only that. Cost: a doc-split (and a future `process-audit` finding to keep them aligned).

**Question for the user:** Inline the one term, keep the whole glossary, or split the glossary?

---

### 4. Should `atdd-test.md` and `atdd-dsl.md` `@include` `dsl-core.md` (8 lines)?

**Observation:** `dsl-core.md` is 8 lines. Including it from two agents costs 16 line-equivalents per ticket. The agent bodies do reference it ("DSL Core Rules from `dsl-core.md`", "Apply test file rules from `test.md` and DSL Core Rules from `dsl-core.md`"). At 8 lines, this is cheap.

**Tradeoff:**
- **Option A — Keep both includes.** Tiny doc, justified pointer. Status quo.
- **Option B — Inline the 8 rules into the test agent body and drop `dsl-core.md`.** Doc is so small that it adds little value as a separate file. But would create duplication if any other agent later needs it.

**Question for the user:** Acceptable status quo, or worth inlining?

## Stale / orphaned content (NOT auto-applied)

### 1. [dsl-port.md] Orphan — referenced as a `architecture-sync` target but no agent `@include`s it

**Evidence:**
- `Grep` for the file path turns up only two references: `architecture-sync.md:24` (lists it as a target doc the plan proposes edits to — i.e. read-as-input, not `@include`d-as-context) and `diagram-architecture.md:10` (the auto-generated diagram lists it as a source).
- No `@docs/atdd/architecture/dsl-port.md` line exists in any agent's `@include` block.
- Last meaningful update: not measured (skipped per the agent spec note about non-essential `git log`).

**Question for the user:** `dsl-port.md` is a 36-line doc covering Fluent Interface, with*() signatures, Stage Transitions, Then Generics, ThenResultStage, No-arg Assertion Variants. Is this content load-bearing for human readers (browsing the docs site)? If yes, leave as-is — it's not waste, it's reference material. If no, consider whether to (a) merge it into `dsl-core.md` (which is currently `@include`d by 2 agents), or (b) keep it as documentation-only and exclude it from agent context (already excluded — no action needed).

**Recommendation:** Likely "reference-only, leave as-is" — it documents the DSL port shape for human readers and the `architecture-sync` agent reads it as input when comparing code to docs. No token cost as no agent `@include`s it.

---

### 2. [at-green-system.md] Stale TODO at end of file

**Evidence:**
- `docs/atdd/process/at-green-system.md:36` reads: `# TODO: VJ: Need to add insutrctions regarding handling legacy code...`
- The doc is `@include`d by `atdd-backend.md`, `atdd-frontend.md`, `atdd-release.md` (3 agents).
- The line is an author note for a future addition, not operational content. It also contains a typo ("insutrctions").

**Question for the user:** Delete the line (3 line-equivalents per ticket saved across the three agents)? Or replace with the intended legacy-handling rule (if you have it ready)? Or leave it as a visible reminder?

