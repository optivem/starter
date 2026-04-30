# Script vs agent: separate mechanical orchestration from creative work in the ATDD pipeline

## Motivation

Distinguish **repetitive / mechanical** work from **creative** work in the ATDD pipeline so that:

1. Mechanical steps (flow control, gate checks, dispatching, ticket-state mutations, file lookups) run as deterministic code — no LLM tokens spent on them.
2. Creative steps (writing Gherkin, DSL, drivers, production code, reviewing) remain as agents, where judgment is actually needed.
3. Token usage drops sharply: agents stop re-loading orchestration logic into context every run, and the script no longer pays for an LLM round-trip to decide "which phase is next?".
4. Flow control becomes reproducible and debuggable — a script trace replaces a transcript.
5. **Orchestration becomes unit-testable.** Today, "given AT-RED-DSL committed and `External Driver Interface Changed = yes`, the next phase is CT-RED-TEST" is verified by reading an agent transcript. Once that transition lives in `atdd-next-phase.sh` (or a Python state-machine evaluator), it's a pure function of (current phase, flags) → next phase, and a test suite can pin every branch — including the gap-finding branches the `process-audit` agent has flagged in `plans/20260429-100633-process-audit.md` (CT exit, structural-cycle escape, smoke-test fail resume path). LLM-driven orchestration cannot be unit tested; deterministic orchestration must be.
6. **Docs become about *substance*, not orchestration.** Today `cycles.md`, the per-phase docs, and the agent bodies all carry "after X, do Y" prose. Once the state machine is the single source of orchestration truth (encoded once, generated outward), every other doc is freed to describe *what the phase is for* and *what good output looks like* — conventions, examples, review criteria, anti-patterns. The per-phase doc stops being "step 4 of 11"; it becomes "what AT-RED-DSL is, what it produces, how to recognise a good DSL diff, what mistakes to avoid". That's the content agents actually need at WRITE time, and it's the content humans actually need when reading the docs.

This follows Anthropic's "Building effective agents" framing: **workflows** (predetermined paths) are code; **agents** are reserved for steps where the path itself depends on judgment.

## Current state — what's an agent today

Under `.claude/agents/atdd/`:

| Agent | Primary work | Mechanical or creative? |
|---|---|---|
| `atdd-orchestrator` | Pick ticket source, resolve project, move card to In Progress, resolve repos, hand off | **Mechanical** (rule-based; the only ambiguity is "ask the user a/b") |
| `atdd-dispatcher` | Fetch issue, classify by Type field + labels (with a documented fast path) | **Mostly mechanical** (the rule walkthrough is only needed for conflicts) |
| `atdd-story` | Convert user story → Gherkin scenarios | **Creative** |
| `atdd-bug` | Convert bug repro → Gherkin scenarios | **Creative** |
| `atdd-task` | Implement UX/UI / system-API / external-API changes | **Creative** |
| `atdd-chore` | Refactor / rename / dep-upgrade end-to-end | **Creative** |
| `atdd-test` | Write / commit acceptance tests | **Creative** |
| `atdd-dsl` | Write / commit DSL | **Creative** |
| `atdd-driver` | Write / commit drivers | **Creative** |
| `atdd-backend` | Make API ATs pass | **Creative** |
| `atdd-frontend` | Make UI ATs pass | **Creative** |
| `atdd-release` | Remove `@Disabled`, commit GREEN, close issue | **Mechanical** (script with one human-confirmation gate per repo's "ask before commit" rule) |

Phase sequencing — "after `atdd-test` COMMIT, run `atdd-dsl` WRITE; after AT-RED-DSL, evaluate `External Driver Interface Changed?` flag; if yes, enter CT sub-process; …" — is currently **embedded in agent bodies and `docs/atdd/process/cycles.md`**, and re-interpreted by an LLM each run.

## Proposed split

### Demote to scripts (Bash or Python under `bin/atdd/` or a new `atdd/` package)

1. **`atdd-orchestrator`** → `atdd-pick-ticket.sh`
   - Pure `gh` calls: read project, find Ready column, pick top, move to In Progress (bottom).
   - Specific-issue mode: `gh issue view`, validate state, optionally move card.
   - Repo resolution: read `README.md` for project link; fall back to `git remote get-url origin`.
   - The only step that needs an LLM is the *first* "a or b?" prompt when neither flag is supplied — and that's `read -p` in a script.
2. **`atdd-dispatcher`** → `atdd-classify.sh`
   - The fast path (single canonical label) is already a pure rule and covers most tickets. Implement it as a script that reads `gh issue view --json projectItems,labels` and emits a classification.
   - Only when signals conflict or are missing does the script call out to an `atdd-classify-fallback` agent (small, single-purpose, body-shape only). 90%+ of tickets never hit the LLM.
3. **`atdd-release`** → `atdd-release.sh`
   - `@Disabled` removal is a deterministic edit. Commit + `gh issue close` are deterministic. Keep the "ask before commit" gate as an interactive script prompt — it's already mechanical (the agent body just enforces a fixed checklist).
4. **Phase progression** (cross-cutting) → `atdd-next-phase.sh`
   - Reads `cycles.md`-encoded transition rules (made executable: a YAML or JSON state-machine file derived from `cycles.md`).
   - Given current phase + a small set of flags (`DSL Interface Changed`, `External Driver Interface Changed`, `System Driver Interface Changed`), emits the next phase to run.
   - Replaces the "what do I run next?" LLM round-trip with a table lookup.
5. **Gate checks** → `atdd-gate.sh`
   - "Did the previous phase commit?" — `git log -1 --format=%s | grep -qE '^AT - RED - TEST - COMMIT'` or equivalent.
   - "Are tests passing?" — `./gradlew test` etc., already mechanical.
   - "Is the pipeline green?" — `gh run list … --json conclusion` already mechanical.

### Keep as agents

Every WRITE / REVIEW phase agent stays:

- `atdd-story`, `atdd-bug` — Gherkin authoring requires interpretation of an issue body.
- `atdd-task`, `atdd-chore` — wide-scope implementation work.
- `atdd-test`, `atdd-dsl`, `atdd-driver` — code authoring + review.
- `atdd-backend`, `atdd-frontend` — production code authoring.

These agents become **smaller** because they no longer carry orchestration prose: no "after you finish, hand off to X". The driving script invokes them with the inputs they need and consumes the COMMIT output.

### A new top-level driver

`atdd run --issue=NN` (or `atdd run --board`) replaces the user's current "invoke the orchestrator agent" mental model:

```
atdd run --issue=42
  → atdd-pick-ticket.sh    (script)
  → atdd-classify.sh       (script, may call atdd-classify-fallback agent)
  → atdd-{story|bug|task|chore}  (agent — creative)
  → loop:
      atdd-next-phase.sh    (script)
      atdd-gate.sh          (script)
      atdd-{test|dsl|driver|backend|frontend}  (agent — creative)
  → atdd-release.sh         (script, with one human-confirmation gate)
```

The script owns the **state machine**; agents own the **artefacts**.

## Token-cost argument (the part the user cares about)

Every current agent invocation re-loads:

- The agent body (orchestration prose + classification rules + handoff conventions).
- Whatever `docs/atdd/process/*.md` files the agent `@includes` to know its phase.
- Re-derived "what's the next step?" reasoning, even when the answer is rule-determined.

For a single ticket flowing test → dsl → driver → backend, that's ~5 agent invocations *each* re-loading the cycle prose. After the split:

- `atdd-pick-ticket.sh`, `atdd-classify.sh`, `atdd-next-phase.sh`, `atdd-gate.sh`, `atdd-release.sh` — **0 tokens**.
- Each remaining agent loads only its own narrow phase doc (e.g. `at-red-test.md`), not `cycles.md` + glossary + diagram-process.

Estimate (rough, to confirm with a real run): ~40–60% reduction in tokens per ticket, concentrated in the orchestration overhead that's currently paid every phase.

## Risks / things this plan must not break

1. **`atdd-orchestrator`'s GitHub MCP tool use.** Some of its `gh project` interactions go via the GitHub MCP server, not plain `gh`. Confirm that everything it does is reachable via `gh` CLI before demoting — if any read needs MCP, leave that read as an agent call (or expose it via a small `gh project` wrapper).
2. **Cycle rules drift.** Today, `cycles.md` is the single source of truth and agents read it as prose. Once the state machine is encoded in YAML/JSON, that file becomes a *second* source. Either:
   - Generate the state-machine file from `cycles.md` (a parser script), or
   - Make the state-machine file authoritative and regenerate the cycles.md ASCII diagrams from it.
   Don't keep two hand-edited copies.
3. **Human-in-the-loop gates.** The repo has firm "ask before commit" / "ask before running system tests" rules in user memory. Scripts must surface these prompts on stdin/stdout, not silently `gh issue close`.
4. **The `atdd-orchestrator` ↔ `atdd-dispatcher` handoff convention** (orchestrator hands off the issue number only; dispatcher re-fetches) is already lean — preserve that property. The script equivalent should pass the issue number as an arg, not the issue body.
5. **Discoverability.** `Skill` invocations like `atdd:atdd-implement-ticket` and `atdd:atdd-manage-project` need to point at the new script entrypoints, or the user-facing slash-commands break.
6. **Agent-body audit.** Once orchestration prose is removed from agent bodies, the `token-usage-audit` agent should re-run to verify each agent body now only carries phase-specific WRITE/REVIEW/COMMIT mechanics — nothing about "hand off to X".

## Implementation order

1. **Inventory mechanical surface area.**
   - Read every agent under `.claude/agents/atdd/` and tag each section as *mechanical* (rule-based, deterministic) or *creative*.
   - Tally tokens spent on orchestration vs content per agent, to confirm the savings claim quantitatively before refactoring.
2. **Encode the state machine.**
   - Translate the AT cycle, CT sub-process, and Structural Cycle flows from `cycles.md` into a single YAML file (`docs/atdd/process/state-machine.yaml`) with phase-id + flag-conditions + next-phase-id.
   - Add a generator script that produces the ASCII diagram in `cycles.md` and the Mermaid in `diagram-process.md` from this YAML — keeps the prose / state-machine in lock-step.
   - **Add a unit-test suite for the state machine.** One test per documented transition, plus negative tests for transitions that should *not* exist. The open process-audit questions (CT-exit flag re-evaluation, structural-cycle fix-loop escape, smoke-test fail resume path, Legacy Coverage Cycle interim spec) become *test gaps* that the suite forces a decision on rather than letting them sit as TBDs in prose.
   - Restructure per-phase docs (`at-red-test.md`, `at-red-dsl.md`, `at-green-system.md`, etc.) to drop "what runs next" and focus on substance: the phase's purpose, what it produces, conventions, example diffs, review criteria, anti-patterns. Orchestration prose moves out; phase substance moves in.
3. **Build the scripts in order of impact:**
   - `atdd-next-phase.sh` (highest call count, biggest win).
   - `atdd-gate.sh`.
   - `atdd-classify.sh` (with `atdd-classify-fallback` agent for the conflict path).
   - `atdd-pick-ticket.sh`.
   - `atdd-release.sh`.
   - `atdd run` top-level driver tying them together.
4. **Slim the kept agents.** For each remaining agent, strip orchestration prose and `@includes` of cross-phase docs. Each agent body should describe only its WRITE / REVIEW / COMMIT mechanics for its phase.
5. **Update slash-commands.** Repoint `atdd:atdd-implement-ticket` and `atdd:atdd-manage-project` at `atdd run …`.
6. **Run a real ticket end-to-end** with the new driver, capture token usage, and compare against the same ticket replayed via the agent-only path. Decision gate: ship only if tokens drop ≥ 30% and all human-in-the-loop gates still fire.
7. **Delete demoted agents** only after one full week of green pipeline runs through the new driver.

## Open questions for the user

1. Do you want the script driver to live in `bin/` (Bash) or in a new `atdd/` Python package? Bash is closer to existing tooling; Python is easier for the state-machine evaluator and YAML parsing.
2. Should `atdd-classify` always try the fast path in script and only escalate to an LLM on conflict, or should every classification still go through an agent for traceability? (My recommendation: fast-path-in-script, since the dispatcher doc already documents that path as deterministic.)
3. Is `cycles.md` allowed to become *generated* from a YAML state machine, or must it remain the hand-edited canonical source? This is the biggest doc-architecture decision in the plan.
4. Are there phases of the AT/CT cycle you consider "creative" that I've classified as mechanical (or vice versa)? The split above is my read — worth a sanity-check before any code moves.
