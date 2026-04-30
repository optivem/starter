# Script vs agent: separate mechanical orchestration from creative work in the ATDD pipeline

🤖 **Picked up by agent** — `Valentina_Desk` at `2026-04-30T10:32:08Z`

## Token-efficient execution strategy (recommended)

This plan is too large for a single agent batch. Push it through in **separate `/execute-plan` sessions**, scoped per chunk below — each session starts with a small context, reads only the files it needs, deletes its items from this plan, and exits. That keeps the conversation transcript from accumulating across the full ~5000+ LOC of new Go + ~9 agent rewrites + ~10 doc rewrites.

**Recommended chunk order** (each its own `/execute-plan` session, batch-then-review mode):

1. **Session 1 — Foundation (this session).** Items: 2.statemachine engine + YAML loader; 1a transitions_test.go; 1c regenerate diagram-process.md; 2.override decorator scaffold; 2.gates + 2.verify interface skeletons. Pure-logic Go + tests; no external integration. Delivers a runnable engine that pins the YAML.
2. **Session 2 — Real `gh` integration.** Items: 2.classify (fast path + LLM fallback); 2.board (project read / pick top / move column); 2.release (regex `@Disabled` removal + commit + close). Each shells out to `gh`; needs to be written against real outputs from your project. One session because they share the `gh` shell-wrapper layer.
3. **Session 3 — Driver + cmd wiring.** Items: 2.driver (the loop); cmd/optivem atdd subcommands; item 4 slash-command repointing. Depends on sessions 1 + 2 being in place.
4. **Session 4 — Per-phase doc rewrites.** Item 1b. Substantive content judgment (purpose / conventions / examples / anti-patterns per phase). Best done as a single focused doc-edit session — possibly with sub-tasks delegated to a subagent for parallel rewrites of independent docs.
5. **Session 5 — Slim kept agents.** Item 3. 9 agent files; each strip is independent; **parallelize via subagents** (one subagent per agent file) — the main session stays small, each subagent gets isolated context for its one file.
6. **Sessions 6–7 — Items 5 + 6** (token-measurement decision; one-week soak). Out of scope for an agent batch; user-driven.

**Why fresh sessions beat one mega-batch.** Within a 5-min window, prompt caching keeps a single session warm, but the cached prefix grows with every read and edit. By item 6+ in a single batch, every tool call is replaying tens of thousands of tokens of prior tool output. Splitting on natural seams (engine → integration → driver → docs → agents) means each session's prefix stays small and the cache is never wasted on irrelevant prior work.

**Within a session, secondary token levers:**
- Read files once, use `Edit` afterward (not re-read).
- Delegate bulk independent rewrites (item 1b docs, item 3 agents) to subagents — subagent context is isolated from the main conversation.
- Avoid running build/test loops mid-conversation when a final `compile-all.sh` at the end suffices.

**Hand-off contract.** At the end of every session in this plan, the agent will re-state these next-step instructions explicitly — what to `/clear`, which session is next, and the exact `/execute-plan` invocation to run. The user does not need to remember the chunk order; it lives in this plan file and gets re-surfaced at every hand-off.

## Motivation

Distinguish **repetitive / mechanical** work from **creative** work in the ATDD pipeline so that:

1. Mechanical steps (flow control, gate checks, dispatching, ticket-state mutations, file lookups) run as deterministic code — no LLM tokens spent on them.
2. Creative steps (writing Gherkin, DSL, drivers, production code, reviewing) remain as agents, where judgment is actually needed.
3. Token usage drops sharply: agents stop re-loading orchestration logic into context every run, and the script no longer pays for an LLM round-trip to decide "which phase is next?".
4. Flow control becomes reproducible and debuggable — a script trace replaces a transcript.
5. **Orchestration becomes unit-testable.** Today, "given AT-RED-DSL committed and `External Driver Interface Changed = yes`, the next phase is CT-RED-TEST" is verified by reading an agent transcript. Once that transition lives in a Go process-flow evaluator, it's a pure function of (current node, flags) → next node, and a test suite can pin every branch — including the gap-finding branches the `process-audit` agent has flagged in `plans/20260429-100633-process-audit.md` §"Missing branches / gaps — NOT auto-applied" items 1, 2, 4 (CT exit, smoke-test fail resume path, structural-cycle escape). LLM-driven orchestration cannot be unit tested; deterministic orchestration must be.
6. **Docs become about *substance*, not orchestration.** Today `cycles.md`, the per-phase docs, and the agent bodies all carry "after X, do Y" prose. Once the process flow is the single source of orchestration truth (encoded once in YAML, Mermaid-and-prose generated outward), every other doc is freed to describe *what the phase is for* and *what good output looks like* — conventions, examples, review criteria, anti-patterns. The per-phase doc stops being "step 4 of 11"; it becomes "what AT-RED-DSL is, what it produces, how to recognise a good DSL diff, what mistakes to avoid". That's the content agents actually need at WRITE time, and it's the content humans actually need when reading the docs.

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

### Demote to Go subcommands (single binary `optivem`, packages under `gh-optivem/internal/atdd/runtime/`)

Each demoted agent maps to a subcommand of the existing `optivem` binary (`gh-optivem`), backed by a package under `gh-optivem/internal/atdd/runtime/`. The user-facing surface mirrors today's slash commands so discoverability transfers; helper subcommands sit underneath for debugging individual phases.

**Public top-level subcommands** (replace today's slash commands; stable API contract):

| Slash command | New | Replaces agent | Backed by |
|---|---|---|---|
| `/atdd:atdd-implement-ticket` | `gh optivem atdd implement-ticket --issue 42` | `atdd-orchestrator` (single-issue mode) | `internal/atdd/runtime/driver/` |
| `/atdd:atdd-manage-project` | `gh optivem atdd manage-project [--project ...]` | `atdd-orchestrator` (board mode) | calls `pick-top-ready` then `implement-ticket` |

**Diagnostic subcommands under `gh optivem atdd debug …`** (porcelain/plumbing split — invokable for debugging, not part of the stable API). All listed below are wired in Cobra under a parent `Use: "debug"` command marked `Hidden: true`, so they don't appear in `gh optivem atdd --help` unless the user passes `--help-all`. The driver calls these helpers internally as it walks the process flow; users can invoke them standalone to reproduce a single phase in isolation. The `Hidden: true` flag signals "not a stable contract" — argument shapes can change without notice.

The two-tier split keeps the daily-use surface small (2 commands) while preserving the debuggability that comes from each helper being invokable on its own.

1. **`atdd-orchestrator` (board-pick logic)** → `gh optivem atdd debug pick-top-ready` (`internal/atdd/runtime/board/`)
   - Pure `gh` calls: read project, find Ready column, pick top, move to In Progress (bottom).
   - Repo resolution: read `README.md` for project link; fall back to `git remote get-url origin`.
   - The only step that might need an LLM is the *first* "a or b?" prompt when neither flag is supplied — and that's `bufio.Scanner` on stdin.

2. **`atdd-dispatcher`** → `gh optivem atdd debug classify --issue 42` (`internal/atdd/runtime/classify/`)
   - The fast path (single canonical label) is already a pure rule and covers most tickets. Implement it in Go: shell out to `gh issue view --json projectItems,labels`, unmarshal, apply the rule, emit a classification.
   - Only when signals conflict or are missing does the subcommand call out to an `atdd-classify-fallback` agent (small, single-purpose, body-shape only). 90%+ of tickets never hit the LLM.

3. **`atdd-release`** → `gh optivem atdd debug release --issue 42` (`internal/atdd/runtime/release/`)
   - `@Disabled` removal is a deterministic edit (regex over the in-scope test files). Commit + `gh issue close` are deterministic. Keep the "ask before commit" gate as an interactive prompt — it's already mechanical (the agent body just enforces a fixed checklist).

4. **Phase progression** (cross-cutting) → `gh optivem atdd debug next-phase` (`internal/atdd/runtime/statemachine/`)
   - Reads the YAML process-flow encoding (see Proposed architecture).
   - Given current phase + a small set of flags (`DSL Interface Changed`, `External Driver Interface Changed`, `System Driver Interface Changed`), emits the next phase to run.
   - Replaces the "what do I run next?" LLM round-trip with a table lookup.

5. **Gateway evaluators (= BPMN exclusive gateways)** → `gh optivem atdd debug gate <name>` (`internal/atdd/runtime/gates/`)
   - Each gateway node in the YAML (`type: gateway`) is bound to a Go function: `GATE_DSL` → `gates.DSLChanged(ctx)`, `GATE_EXT_DRIVER` → `gates.ExternalDriverChanged(ctx)`, etc.
   - The CLI subcommand evaluates one binding standalone — useful for debugging "why did the driver follow the No edge from GATE_DSL?".

6. **Phase verifications (pre-/post-conditions)** → middleware in `internal/atdd/runtime/verify/` (no CLI surface, not even under `debug`)
   - Pre-/post-condition guards wrapping each node's run: "before AT_RED_DSL_WRITE, HEAD must match `^AT - RED - TEST - COMMIT`"; "after AT_RED_TEST_COMMIT, HEAD must match `^AT - RED - TEST - COMMIT`".
   - Implemented as decorator middleware in the driver loop (same pattern as the override hook).
   - Not invocable standalone — they're not standalone questions, only meaningful in the context of an advancing pipeline.

External-state polls (e.g. "is the GitHub Actions pipeline green for the latest commit?") are *not* gates — they're the body of mechanical wait-tasks like `WAIT_FOR_PIPELINE`. They live inside `internal/atdd/runtime/actions/` as the implementation of the corresponding service task, not as a separate concept.

### Keep as agents

Every WRITE / REVIEW phase agent stays:

- `atdd-story`, `atdd-bug` — Gherkin authoring requires interpretation of an issue body.
- `atdd-task`, `atdd-chore` — wide-scope implementation work.
- `atdd-test`, `atdd-dsl`, `atdd-driver` — code authoring + review.
- `atdd-backend`, `atdd-frontend` — production code authoring.

These agents become **smaller** because they no longer carry orchestration prose: no "after you finish, hand off to X". The Go driver invokes them with the inputs they need and consumes the COMMIT output.

### A new top-level driver

`gh optivem atdd implement-ticket --issue 42` (and `gh optivem atdd manage-project` for board mode) replaces the user's current "invoke the orchestrator agent" mental model. `gh-optivem` is a `gh` extension, so users invoke it as `gh optivem ...`; internally the Cobra binary is named `optivem`, and code paths reference it by that name.

```
gh optivem atdd implement-ticket --issue 42
  → validate issue + move to In Progress         (internal: actions/board)
  → classify (internal call into runtime/classify)(also exposed as: gh optivem atdd debug classify --issue 42)
  → atdd-{story|bug|task|chore}                   (agent — creative)
  → driver loop (walks the process flow):
      next-phase via statemachine.Next            (debug-exposed: gh optivem atdd debug next-phase)
      gateway evaluation via gates.<binding>      (debug-exposed: gh optivem atdd debug gate <name>)
      phase verification middleware               (internal-only)
      atdd-{test|dsl|driver|backend|frontend}     (agent — creative; user_task in BPMN terms)
  → release (internal call into runtime/release)  (also exposed as: gh optivem atdd debug release --issue 42, with one human-confirmation gate)
```

Internal calls don't shell out to the binary recursively — they go through the Go package directly. The `debug` subcommands are thin wrappers around the same packages, exposed for standalone diagnosis.

The driver owns the **process flow**; agents own the **artefacts**.

## Proposed architecture

### Language and location

- **Language: Go.** Single static binary, no runtime dependency on the consumer's machine, easy YAML parsing in stdlib via `gopkg.in/yaml.v3`, easy `os/exec` for `gh` / `git`, easy unit testing for the process-flow evaluator. Ships as one artefact, distributed via the existing `gh-optivem` plugin.
- **Location: `gh-optivem` from day one.** `gh-optivem` is already a Cobra-based Go binary (`Use: "optivem"`), already has an `internal/atdd/install.go` for scaffold-time setup, and already builds locally as `gh-optivem.exe` with plain `go build`. Iteration is fast — no detour through `shop` is needed. New runtime code lives at `gh-optivem/internal/atdd/runtime/` (subpackages: `driver`, `statemachine`, `gates`, `verify`, `actions`, `agents`, `override`, `board`, `classify`, `release`), kept separate from the existing install-time `internal/atdd/install.go` to avoid mixing the two concerns. The user-facing entrypoint is `gh optivem atdd …` (since `gh-optivem` is a `gh` extension; `gh` strips the prefix and delegates to the Cobra root command `optivem`).
- **Reading from the consumer's CWD.** The binary reads the YAML process-flow file and the per-phase docs from the consumer's working directory (`docs/atdd/process/process-flow.yaml`, `docs/atdd/process/<phase>.md`). The flow definition is part of each consumer's repo, not bundled in the binary. This is a *good* constraint to lock in from day one: it forces the binary to be repo-agnostic, which it has to be to ship via `gh-optivem`.

### Process flow: YAML-canonical, Mermaid-derived view

The single source of truth for orchestration is **`docs/atdd/process/process-flow.yaml`** — a small machine-readable file (~30 nodes, ~50 sequence flows). The Mermaid diagram in `diagram-process.md` is regenerated from the YAML by the `diagram-generator` agent, so it stays in lock-step but is a *view*, not the truth. The per-phase prose docs (`at-red-test.md`, `at-red-dsl.md`, …) are hand-edited and describe the *substance* of each phase — purpose, conventions, examples, review criteria, anti-patterns — exactly what an agent needs at WRITE time and what a human needs when reading the docs. Orchestration (the "what runs next?" prose) moves out of the per-phase docs entirely; phase substance moves in. This realises motivation §6.

#### BPMN vocabulary, not FSM vocabulary

The flow is process-shaped, not state-shaped: nodes represent *activities* (work happens in them) with *gateways* (branch points). This isn't a finite-state machine in the classic sense — there are no "external events" that fire transitions; control passes when an activity completes. We borrow BPMN's vocabulary because it fits, but we don't use a BPMN engine — the runtime is a small hand-coded Go graph traversal (see "Engine design" below).

| Our element | BPMN equivalent | What it does |
|---|---|---|
| `START` | Start event | Pipeline entry point |
| `END` | End event | Pipeline terminus |
| Mechanical node (`PICK_TOP_READY`, `MOVE_TO_IN_PROGRESS`, `CLOSE_ISSUE`, `WAIT_FOR_PIPELINE`) | Service task | Auto-execute Go function from `internal/atdd/runtime/actions/`; no agent dispatch |
| Creative node (`AT_RED_TEST_WRITE`, `AT_GREEN_BACKEND`) | User task | Dispatches the bound agent and waits for its COMMIT output |
| REVIEW phase | User task with human approval | Blocks on stdin until user approves; same shape as creative node but the agent's role is "present what was just written" |
| Decision diamond (`GATE_DSL`, `GATE_EXT_DRIVER`) | Exclusive gateway (XOR) | Calls the bound `gates.<name>(ctx)` function and follows the matching outgoing edge |
| CT sub-process | Call activity | Embedded sub-flow with its own start/end; control returns to the parent's next sequence flow on completion. Resolves the process-audit gap on CT termination semantics |
| Per-scenario loop | Loop activity (or sequence flow returning to AT-RED-TEST) | Iterates while `// TODO:` scenarios remain |

We do **not** adopt: BPMN XML (verbose; YAML is shorter), workflow engines (Camunda/Zeebe/Flowable assume long-running multi-actor processes), or boundary events (timeouts on stuck phases — noted as a v2 candidate in Open Questions).

#### YAML schema (sketch)

```yaml
nodes:
  - id: START
    type: start_event
  - id: PICK_TOP_READY
    type: service_task
    action: pick_top_ready          # → actions.PickTopReady(ctx)
  - id: MOVE_TO_IN_PROGRESS
    type: service_task
    action: move_to_in_progress
  - id: AT_RED_TEST_WRITE
    type: user_task
    agent: atdd-test
    phase_doc: docs/atdd/process/at-red-test.md
  - id: AT_RED_TEST_COMMIT
    type: service_task
    action: commit_phase
  - id: GATE_DSL
    type: gateway
    binding: dsl_changed             # → gates.DSLChanged(ctx)
  - id: AT_RED_DSL_WRITE
    type: user_task
    agent: atdd-dsl
    phase_doc: docs/atdd/process/at-red-dsl.md
  - id: AT_GREEN_SYSTEM
    type: user_task
    agent: atdd-backend
    phase_doc: docs/atdd/process/at-green-system.md
  - id: CT_SUBPROCESS
    type: call_activity
    flow: docs/atdd/process/ct-subflow.yaml
  - id: END
    type: end_event

sequence_flows:
  - {from: START,                to: PICK_TOP_READY}
  - {from: PICK_TOP_READY,       to: MOVE_TO_IN_PROGRESS}
  - {from: MOVE_TO_IN_PROGRESS,  to: AT_RED_TEST_WRITE}
  - {from: AT_RED_TEST_WRITE,    to: AT_RED_TEST_COMMIT}
  - {from: AT_RED_TEST_COMMIT,   to: GATE_DSL}
  - {from: GATE_DSL,             to: AT_RED_DSL_WRITE,  when: "dsl_changed == true"}
  - {from: GATE_DSL,             to: AT_GREEN_SYSTEM,   when: "dsl_changed == false"}
  # …more flows; CT_SUBPROCESS slots in after AT_RED_DSL_COMMIT when external_driver_changed == true…
  - {from: AT_GREEN_SYSTEM,      to: END}
```

This is the entire orchestration spec. ~30 nodes for the AT cycle + CT sub-process + per-scenario loop. Every transition is a YAML row that the unit-test suite can assert against.

#### Mermaid view, regenerated

The `diagram-generator` agent's contract inverts: instead of reading prose to produce Mermaid, it reads `process-flow.yaml` to produce Mermaid. The diagram in `diagram-process.md` is a derived artefact — visual debugging without the YAML being load-bearing in two places. The agent body shrinks accordingly (most of it currently describes prose-to-diagram heuristics that are no longer needed).

Per-phase prose docs (`at-red-test.md`, etc.) remain hand-edited but are now *validated* against the YAML by an `gh optivem atdd doctor` subcommand (v2 nice-to-have): every `user_task` has a matching `phase_doc:` file, every gateway `binding:` resolves to a registered Go function, every node ID is unique. Doctor failures are wired into CI.

#### Unit test contract

`gh-optivem/internal/atdd/runtime/statemachine/transitions_test.go` — one test per documented sequence flow, plus negative tests for transitions that should *not* exist. Synthetic inputs `(currentNode, flagState) → expectedNextNode`. The open process-audit gaps (CT exit re-evaluation, smoke-test resume path, structural-cycle escape) become concrete test cases — they cannot remain TBDs once the suite is green. The Legacy Coverage Cycle's TBD interim phases get a single explicit test case for whatever interim spec the user lands on (e.g. "the orchestrator stops and asks the user"), forcing a decision at write-time.

### Engine design: hand-coded functor pattern (no library)

#### Library vs hand-coded — why hand-coded

Two library categories were considered and rejected:

- **Go FSM libraries** (`looplab/fsm`, `qmuntal/stateless`). Designed for *object has state, transitions on external events* (a TCP connection's lifecycle, an order's status). Wrong shape for our model: in our case each node *is itself* an action that runs to completion and produces a result that picks the next edge. Adapting an FSM library to "node-as-activity" semantics costs more than the ~200 lines of graph traversal we'd write directly. The interesting logic isn't "what's the next state?" (a one-line table lookup) — it's "what does *this* node do?" (gate evaluation, agent dispatch, shell call). Libraries help with the former; we mostly need the latter.
- **BPMN engines** (Camunda, Zeebe, Flowable, Activiti). Designed for *long-running, multi-actor, persisted* processes — durable execution across process restarts, role-based task queues, compensation handlers, transaction boundaries, message correlation, visual modelling tools for non-developers. We have one ticket end-to-end in one terminal session, one human, one developer editing the flow definition directly. The engines are 1000× the weight we need; their persistence and concurrency models add complexity we'd have to actively work around.

**Conclusion:** borrow BPMN's *vocabulary and thinking* (above), hand-code the runtime. The whole engine is ~200 lines of Go.

#### Core types

In `gh-optivem/internal/atdd/runtime/statemachine/`:

```go
type Outcome struct {
    Bool   bool   // for gateway nodes: which edge to take
    Commit string // for user_task nodes: phase commit SHA
    Err    error  // STOP and surface to the user
}

type NodeFn func(*Context) Outcome

type Node struct {
    ID   string
    Kind NodeKind  // StartEvent | EndEvent | ServiceTask | UserTask | Gateway | CallActivity
    Fn   NodeFn    // the functor
}

type Edge struct {
    From string
    To   string
    When func(Outcome) bool  // gateway edges check Bool against `when:` predicate; sequential edges always-true
}

type Flow struct {
    Nodes map[string]Node
    Edges []Edge
    Start string
}

func (f *Flow) Run(ctx *Context) error {
    cur := f.Start
    for cur != "" {
        out := f.Nodes[cur].Fn(ctx)
        if out.Err != nil { return out.Err }
        cur = f.nextEdge(cur, out).To
    }
    return nil
}
```

Each kind of node registers a `NodeFn` — idiomatic Go function value, not a class with `.execute()`:

```go
gates.DSLChanged       = func(ctx *Context) Outcome { /* read flags / diff */ return Outcome{Bool: ...} }
actions.PickTopReady   = func(ctx *Context) Outcome { /* gh project ... */ }
agents.AtddTestWrite   = func(ctx *Context) Outcome { return dispatch("atdd-test", ctx) }
```

#### Why split "what the node does" from "where to go next"

`Fn` returns an `Outcome`, not the next node. Routing lives in the edge list, not inside each node. This keeps gateway nodes clean (their job is "compute one boolean", nothing more) and lets the unit-test suite assert routing without mocking node bodies. It also matches BPMN's separation of *activity* and *sequence flow*: the activity does the work; the flows around it decide where control goes.

#### Decorator pattern for cross-cutting concerns

Phase verifications (B in the gate split) and the future override hook are middleware decorators wrapping each `Fn`:

```go
func WithVerify(orig NodeFn, nodeID string) NodeFn {
    return func(ctx *Context) Outcome {
        if err := verify.Pre(ctx, nodeID); err != nil { return Outcome{Err: err} }
        out := orig(ctx)
        if out.Err == nil { verify.Post(ctx, nodeID, out) }
        return out
    }
}

func WithOverride(orig NodeFn, nodeID string) NodeFn { /* see Per-step override hook */ }
```

At driver startup, walk the node map and rewrap every `Fn` with the decorators that apply. Verifications are always on; the override decorator is always wrapped (so v2 only adds CLI parsing, not engine plumbing).

#### Resume detection

The driver resolves the start node before the loop begins:
1. Default: `flow.Start` (the YAML `START` node).
2. If the working tree contains `@Disabled` markers with known phase prefixes (existing convention from `cycles.md`), resolve to the corresponding node ID instead — the pipeline picks up where a previous session left off.

Resume is computed once at driver startup, not re-evaluated mid-flow, so it doesn't intrude on the engine's hot path.

### Per-step override hook (FUTURE — out of scope for v1, but design must allow it)

Before each node executes, the driver checks for a per-node override:
- **CLI form:** `gh optivem atdd implement-ticket --issue 42 --extra AT_RED_DSL_WRITE="Use record types instead of classes"` → extra text is appended to the agent prompt for that one dispatch.
- **Replace form:** `--replace AT_RED_DSL_WRITE="<full prompt>"` → swaps the default prompt entirely (escape hatch for debugging or one-off departures).
- **Interactive form:** `gh optivem atdd implement-ticket --issue 42 --interactive` → before every creative node, the driver prints `About to dispatch atdd-dsl. Press Enter to proceed, or type extra instructions:` and reads stdin.
- **Mechanical override:** for mechanical nodes, `--override PICK_TOP_READY="<shell snippet>"` runs the snippet instead of the default function. Discouraged but useful for local experimentation.

The hook lives in `gh-optivem/internal/atdd/runtime/override/` and wraps every node dispatch in the driver loop. v1 ships with the hook present but no CLI surface — you cannot pass overrides yet, but the wrapping is in place so v2 only needs to expose flags.

## Token-cost argument (the part the user cares about)

Every current agent invocation re-loads:

- The agent body (orchestration prose + classification rules + handoff conventions).
- Whatever `docs/atdd/process/*.md` files the agent `@includes` to know its phase.
- Re-derived "what's the next step?" reasoning, even when the answer is rule-determined.

For a single ticket flowing test → dsl → driver → backend, that's ~5 agent invocations *each* re-loading the cycle prose. After the split:

- `runtime/board`, `runtime/classify` (fast path), `runtime/statemachine`, `runtime/gates`, `runtime/verify`, `runtime/release` — **0 tokens** (pure Go). All are also exposed under `gh optivem atdd debug …` for standalone diagnostics; same code, same zero-token cost.
- Each remaining agent loads only its own narrow phase doc (e.g. `at-red-test.md`), not `cycles.md` + glossary + diagram-process.

Estimate (rough, to confirm with a real run): ~40–60% reduction in tokens per ticket, concentrated in the orchestration overhead that's currently paid every phase.

## Risks / things this plan must not break

1. **`atdd-orchestrator`'s GitHub MCP tool use.** Some of its `gh project` interactions go via the GitHub MCP server, not plain `gh`. Confirm that everything it does is reachable via `gh` CLI before demoting — if any read needs MCP, leave that read as an agent call (or expose it via a small `gh project` wrapper).
2. **Cycle rules drift.** Today, `cycles.md` carries both orchestration prose ("after X, do Y") and phase substance ("what AT-RED-DSL is for"). After the rewrite, those two responsibilities split: orchestration moves into `process-flow.yaml` (single source of truth, the only place transitions are encoded); per-phase substance stays in the per-phase docs (`at-red-test.md`, etc.); `cycles.md` itself either shrinks to a high-level overview or gets retired in favour of the diagram + per-phase docs (see Open Questions). The risk: someone re-introduces orchestration prose into a per-phase doc out of habit. Mitigation: the `gh optivem atdd doctor` subcommand (v2) checks for this; in the meantime, the `token-usage-audit` agent flags it.

The `diagram-generator` agent's contract also inverts (read YAML → write Mermaid, instead of read prose → write Mermaid). The agent body shrinks accordingly. This is a non-trivial contract change — call it out in the rewrite of that agent's body.
3. **Human-in-the-loop gates.** The repo has firm "ask before commit" / "ask before running system tests" rules in user memory. Scripts must surface these prompts on stdin/stdout, not silently `gh issue close`.
4. **The `atdd-orchestrator` ↔ `atdd-dispatcher` handoff convention** (orchestrator hands off the issue number only; dispatcher re-fetches) is already lean — preserve that property. The script equivalent should pass the issue number as an arg, not the issue body.
5. **Discoverability.** `Skill` invocations like `atdd:atdd-implement-ticket` and `atdd:atdd-manage-project` need to point at the new script entrypoints, or the user-facing slash-commands break.
6. **Agent-body audit.** Once orchestration prose is removed from agent bodies, the `token-usage-audit` agent should re-run to verify each agent body now only carries phase-specific WRITE/REVIEW/COMMIT mechanics — nothing about "hand off to X".

## Implementation order — remaining

Sessions 1 + 2 shipped: the statemachine engine, transitions test suite, regenerated diagrams, and registry/middleware scaffolds (Session 1); plus the real Go bindings for `gates/`, `actions/`, and `verify/` with hermetic unit tests (Session 2). What's left:

1. **Per-phase doc rewrites** (Session 4). Restructure `at-red-test.md`, `at-red-dsl.md`, `at-red-system-driver.md`, `at-green-system.md`, `ct-red-test.md`, `ct-red-dsl.md`, `ct-red-external-driver.md`, `ct-green-stubs.md`, `task-and-chore-cycles.md` to drop "what runs next" prose (now lives in the YAML) and focus on substance: the phase's purpose, what it produces, conventions, example diffs, review criteria, anti-patterns. Best done as a single focused session, possibly with sub-tasks delegated to subagents for parallel rewrites of independent docs.
2. **Driver + cmd wiring** (Session 3). `internal/atdd/runtime/driver/` (the top-level loop wiring engine.RunFlow with bound registries, override hooks, and verify decorators) + a new `gh-optivem/atdd_commands.go` exposing `gh optivem atdd implement-ticket --issue N` and `gh optivem atdd manage-project` (and the hidden `debug` parent for the diagnostic helpers). One line into `main.go` rootCmd.
3. **Update slash-commands** (Session 3). Repoint `atdd:atdd-implement-ticket` and `atdd:atdd-manage-project` at `gh optivem atdd implement-ticket` and `gh optivem atdd manage-project`. Pass through `--issue`, `--project`, `--autonomous`, `--rehearsal`, `--no-memory`, etc.
4. **Run a real ticket end-to-end** with the new driver, capture token usage, and compare against the same ticket replayed via the agent-only path. Decision gate: ship only if tokens drop ≥ 30% and all human-in-the-loop gates still fire. _User-driven; cannot be executed by an agent batch._
5. **Delete demoted agents** only after one full week of green pipeline runs through the new driver. _User-driven._

## Decisions made

1. **Language:** Go. Single static binary, ships via the existing `gh-optivem` plugin.
2. **Location:** `gh-optivem` from day one — no detour through `shop`. Runtime code at `gh-optivem/internal/atdd/runtime/` (kept separate from existing scaffold-time `internal/atdd/install.go`). User-facing entrypoint is `gh optivem atdd …`.
3. **Process-flow encoding:** YAML-canonical (`docs/atdd/process/process-flow.yaml`); Mermaid diagram regenerated from YAML; per-phase prose docs hand-edited but validated against the YAML.
4. **Vocabulary:** BPMN-shaped (process flow with activities and gateways), not FSM. Borrow vocabulary, don't import a BPMN engine.
5. **Engine:** hand-coded functor pattern, ~200 lines of Go. No FSM library, no workflow engine. (Rationale in §Engine design.)
6. **CLI surface (porcelain/plumbing split):** `gh optivem atdd implement-ticket` and `gh optivem atdd manage-project` are the only public commands, mirroring today's slash commands. Diagnostic helpers (`pick-top-ready`, `classify`, `next-phase`, `gate`, `release`) live under a hidden parent `gh optivem atdd debug …` (Cobra `Hidden: true`) — invokable for debugging, not part of the stable API contract.
7. **Override hook:** decorator middleware shipped in v1 with no CLI surface; v2 adds `--extra` / `--replace` / `--interactive` flags.
8. **"Gate" three-way split:** gateway nodes (BPMN exclusive gateways) are first-class YAML nodes with CLI surface; phase verifications are internal middleware; external-state polls are bodies of mechanical wait-tasks.
9. **Classify fallback policy:** fast-path in Go (deterministic rule on canonical type-label); LLM (`atdd-classify-fallback` agent) called only on conflict, missing labels, or ambiguity. Every classification — fast-path or fallback — writes a one-line entry to a local `classify.log` (issue number, labels seen, classification, route taken) so the audit trail exists without paying the always-LLM cost.
10. **Boundary events deferred to v2.** v1 (MVP) ships without BPMN boundary events (timeouts, error escalations on user_tasks). v1 runs in a single terminal session with a human present, so "stuck" phases are caught by the user, not by a timer. Engine design preserves room for v2: the existing `Outcome.Err` path and decorator pattern extend naturally to wrap each `NodeFn` with a timer goroutine + cancel channel, no engine surgery needed.
11. **`cycles.md` shrinks to a high-level overview.** Most of today's `cycles.md` content moves: phase sequencing prose → `process-flow.yaml` (and is deleted from prose); ASCII diagrams → regenerated Mermaid in `diagram-process.md`; phase-to-agent mapping → YAML `agent:` fields; resume-detection table → `at-cycle-conventions.md`; flag definitions → per-phase docs + glossary; onboarding sub-process → its own per-phase doc or sub-flow YAML. What remains in `cycles.md` is a ~30-line landing page: "ATDD has an AT cycle, a CT sub-process, and a Structural Cycle; orchestration lives in `process-flow.yaml`; diagram in `diagram-process.md`; each phase has its own doc." Hand-edited but small enough to maintain. Provides the docs a clear front door without holding orchestration prose that would drift from the YAML.
12. **Cobra wiring fits the existing `gh-optivem` pattern.** Confirmed against `runner_commands.go`: parent commands have `Use:` + `Short:`, no `Run:`, then `AddCommand(...)` for children; each subcommand has its own `newXxxCmd()` factory. Our additions live in a new `gh-optivem/atdd_commands.go` (mirroring `runner_commands.go`'s naming convention — file named after the package it wires, `internal/atdd/runtime`). One line in `main.go` rootCmd: `rootCmd.AddCommand(newAtddCmd())`. The `debug` parent uses `Hidden: true` to keep diagnostic subcommands out of `--help`. No deviation from established conventions.

## Open questions for the user

_None remaining — all decisions locked in §Decisions made above._
