Implement the following ticket using the multi-agent ATDD workflow.

Input: $ARGUMENTS

The input is a GitHub issue number (e.g. `#42`), optionally followed by flags. A ticket is always required — free-text stories are not accepted.

**Autonomous mode:** if `--autonomous` is present in the input, skip all STOP/human-approval steps — agents self-approve and the pipeline runs end-to-end without waiting for the user.

**Docs-only mode:** if `--no-memory` is present in the input, run the pipeline as if no auto-memory exists. The orchestrator must not apply any remembered preferences, feedback, or project context from `MEMORY.md` or its referenced files, and must instruct every dispatched sub-agent to do the same (include a literal "Ignore auto-memory; rely solely on `docs/atdd/**` and the ticket." line in each agent prompt). The intent is to validate that the docs are self-sufficient — if a run drifts or fails because guidance only existed in memory, that is a docs gap to surface, not a reason to consult memory.

**Repositories:** optionally specify which repositories the pipeline operates on:
- `--test-repos <repo1>,<repo2>,...` — the test repositories to implement in (e.g. `shop`)
- `--system-repos <repo1>,<repo2>,...` — the system (backend/frontend) repositories (e.g. `shop`)
<!-- TODO(gh-optivem): multirepo support — install-time substitution should produce repo lists matching the consumer's scaffold: `<repo>-backend`,`<repo>-frontend` (multitier), `<repo>-system` (multirepo monolith), or just `<repo>` (monorepo). v1 install is monorepo-only. -->

If not specified, infer the appropriate repositories from the GitHub issue context (labels, title, existing tests, etc.) and confirm with the user before proceeding.

## Orchestration

Follow the decision flow defined in `docs/atdd/process/cycles.md`. That document defines:
- The AT cycle decision tree (TEST → DSL → SYSTEM DRIVER → GREEN, with skip logic)
- The Contract Test sub-process (triggered when external driver interfaces change)
- The scenario loop (repeat until all scenarios are GREEN)
- Phase-to-agent mapping
- STOP behaviour (normal vs autonomous mode)
- Resume detection via `@Disabled` markers

### Pre-flight

#### Run Mode Confirmation

Before any other work, detect the current run mode along two axes and present it to the user for confirmation. **Skip this entire confirmation in autonomous mode** — just print the detected mode and proceed.

**Memory mode** — whether auto-memory will be applied:
- `OFF` if `--no-memory` is in `$ARGUMENTS`
- `ON`  otherwise (this is the default)

**Rehearsal mode** — whether commits land in a throwaway worktree or in the real repo. Detect by checking BOTH:
- The current working directory: does the repo root basename match `shop-rehearsal-*`? (Run `basename "$(git rev-parse --show-toplevel)"`.)
- The current branch: does it match `rehearsal/*`? (Run `git rev-parse --abbrev-ref HEAD`.)

If either matches → `ON` (rehearsal). If neither matches → `OFF` (real repo). The default is `OFF` — rehearsal mode is only entered by explicitly running `scripts/atdd-rehearsal-start.sh <name>` and starting a fresh Claude Code session inside the resulting worktree.

Output a confirmation block of this exact shape, filled with the detected values:

```
Run mode for issue #<N>:
  Memory:    <ON|OFF>   (default: ON  — applies MEMORY.md preferences; --no-memory to disable)
  Rehearsal: <ON|OFF>   (default: OFF — commits land in <repo-root> on branch <current-branch>)
```

When `Rehearsal: ON`, change the parenthetical on that line to `commits land in <worktree-path> on branch <rehearsal-branch>` so the user can see the throwaway location.

Then ask: **"Proceed with this mode? (yes to start, or cancel to change mode)"**.

If the user wants to change:
- Memory: cancel, re-invoke the skill with or without `--no-memory`.
- Rehearsal ON → OFF: cancel, exit this Claude Code session, start a new one in the real shop checkout.
- Rehearsal OFF → ON: cancel, run `scripts/atdd-rehearsal-start.sh <name>`, then start a fresh Claude Code session inside `../shop-rehearsal-<name>` and re-invoke the skill there.

Only proceed past this gate after explicit confirmation (or in autonomous mode).

#### Status Validation

Check the issue's status on the GitHub project board:
- **Ready** → move it to **In Progress** and proceed.
- **In Progress** → proceed (resume case).
- **Any other status** → STOP. Tell the user the issue is in status `<status>` and ask whether to proceed.

#### Resume Detection

Before doing anything else, scan the test repository for `@Disabled` annotations with known phase markers (see cycles.md resume detection table). If found, skip to the indicated phase. If not found, proceed from step 1.

### Step 1: Story

Launch **story-agent** with the issue number. It will read the GitHub issue and produce Gherkin scenarios.
- **Normal mode:** Present the Gherkin scenarios and wait for human approval.
- **Autonomous mode:** Auto-approve and proceed immediately.

After approval, update the issue body with the approved Gherkin scenarios (use `gh issue edit` preserving the user story preamble).

### Step 2: Per-Scenario Loop

For each scenario, follow the AT cycle decision tree from `cycles.md`:

1. **AT - RED - TEST:** Launch test-agent (WRITE → STOP → COMMIT).
2. **Decision:** DSL Interface Changed? If no → skip to GREEN.
3. **AT - RED - DSL:** Launch dsl-agent (WRITE → STOP → COMMIT).
4. **Decision:** External System Driver Interface Changed? If yes → run Contract Test Sub-Process.
5. **Decision:** System Driver Interface Changed? If no → skip to GREEN.
6. **AT - RED - SYSTEM DRIVER:** Launch driver-agent (WRITE → STOP → COMMIT).
7. **AT - GREEN - SYSTEM:** Launch backend-agent → frontend-agent → release-agent.
8. If remaining `// TODO:` scenarios exist, loop back to step 1.

### Escalation

If any agent reports it cannot proceed, STOP and present the blocker to the user — **even in autonomous mode**.
