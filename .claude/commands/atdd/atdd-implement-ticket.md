Implement the following ticket using the multi-agent ATDD workflow.

Input: $ARGUMENTS

The input is a GitHub issue number (e.g. `#42`), optionally followed by flags. A ticket is always required — free-text stories are not accepted.

**Autonomous mode:** if `--autonomous` is present in the input, skip all STOP/human-approval steps — agents self-approve and the pipeline runs end-to-end without waiting for the user.

**Docs-only mode:** if `--no-memory` is present in the input, run the pipeline as if no auto-memory exists. The orchestrator must not apply any remembered preferences, feedback, or project context from `MEMORY.md` or its referenced files, and must instruct every dispatched sub-agent to do the same (include a literal "Ignore auto-memory; rely solely on `docs/atdd/**` and the ticket." line in each agent prompt). The intent is to validate that the docs are self-sufficient — if a run drifts or fails because guidance only existed in memory, that is a docs gap to surface, not a reason to consult memory.

**Rehearsal mode:** if `--rehearsal` is present in the input (optionally followed by `<label>`), the skill resolves an id, prints the commands needed to enter a throwaway rehearsal worktree (sibling at `../rehearsal-<id>` on branch `rehearsal/<id>`), and **exits without running the pipeline**. The id is always timestamp-led so rehearsals are sortable and parallel-safe:

- `--rehearsal` alone → `<id>` = `<ts>` (e.g. `20260429-141230`), where `<ts>` comes from `date +%Y%m%d-%H%M%S`.
- `--rehearsal <label>` → `<id>` = `<ts>-<label>` (e.g. `20260429-141230-demo1`). `<label>` must match `[A-Za-z0-9_-]+`.

You then run the printed commands, open a fresh Claude Code session inside the worktree, and re-invoke the skill there (without `--rehearsal`, since the worktree CWD is already enough to put the run in rehearsal mode). The flag is a setup shortcut, not an in-session driver — it cannot run the pipeline against the worktree from your current session because sub-agents inherit the harness's working directory, which is fixed at session start. If the current session is already inside a rehearsal worktree, `--rehearsal` is ignored. Default: no `--rehearsal` flag → commits land in the current repo on the current branch.

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

Before any other work, handle the `--rehearsal` fast-path, then detect and confirm the run mode.

##### Rehearsal flag fast-path

Detect whether the current session is already inside a rehearsal worktree by checking BOTH:
- repo root basename matches `rehearsal-*` (run `basename "$(git rev-parse --show-toplevel)"`)
- current branch matches `rehearsal/*` (run `git rev-parse --abbrev-ref HEAD`)

If either matches → `in_rehearsal = true`. Otherwise `in_rehearsal = false`.

Now look for `--rehearsal` in `$ARGUMENTS` and resolve the rehearsal `<id>`:
1. Always generate the timestamp prefix: `<ts>` = output of `date +%Y%m%d-%H%M%S` (e.g. `20260429-141230`).
2. If `--rehearsal` is followed by a token that does NOT start with `--`, treat that token as `<label>`. Validate it against `[A-Za-z0-9_-]+`; if it fails validation, STOP with an error explaining usage: `--rehearsal [<label>]` (e.g. `--rehearsal demo1`). Set `<id>` = `<ts>-<label>`.
3. Otherwise (no token, or the next token is another flag): set `<id>` = `<ts>`.

Then act:
- If `--rehearsal` is present AND `in_rehearsal = false` → **print the setup commands and EXIT immediately**, doing nothing else (no status validation, no story, nothing). Print this exact block, substituting the resolved `<id>`, `<N>`, and `<other-flags>` (every flag from `$ARGUMENTS` except `--rehearsal` and its optional `<label>` value):

  Print the commands flush-left (no leading whitespace) so they paste cleanly into a shell. Use this exact shape:

  ```
  Rehearsal mode requested. Run these commands yourself:

  ./scripts/atdd-rehearsal-start.sh <id>
  cd ../rehearsal-<id>
  claude

  Then in the new Claude Code session, re-invoke the skill (without --rehearsal):

  /atdd:atdd-implement-ticket #<N> <other-flags>

  Nothing has been done in this session. The fresh session is required so
  Claude Code's working directory becomes the rehearsal worktree — otherwise
  sub-agents would commit into your real repo.
  ```

- If `--rehearsal` is present AND `in_rehearsal = true` → ignore the flag (you're already in a rehearsal) and proceed.
- If `--rehearsal` is absent → proceed.

##### Confirmation block

For all paths that proceed past the fast-path, detect the run mode along two axes and present it for confirmation. **Skip this entire confirmation in autonomous mode** — just print the detected mode and proceed.

**Memory mode** — whether auto-memory will be applied:
- `OFF` if `--no-memory` is in `$ARGUMENTS`
- `ON`  otherwise (this is the default)

**Rehearsal mode** — `ON` if `in_rehearsal = true` from the fast-path detection above, else `OFF`. The default is `OFF`; rehearsal mode is only entered by running `--rehearsal [<label>]` (which prints setup instructions) and starting a fresh Claude Code session inside the resulting worktree.

Output a confirmation block of this exact shape, filled with the detected values:

```
Run mode for issue #<N>:
  Memory:    <ON|OFF>   (default: ON  — applies MEMORY.md preferences; --no-memory to disable)
  Rehearsal: <ON|OFF>   (default: OFF — commits land in current repo on branch <current-branch>; --rehearsal [<label>] for setup instructions, omit label for an auto timestamp id)
```

When `Rehearsal: ON`, change the Rehearsal parenthetical to `commits land in <worktree-path> on branch <rehearsal-branch>; scripts/atdd-rehearsal-end.sh <id> to discard` (substituting the actual `<id>` from the worktree path) so the user can see the throwaway location and how to clean it up.

Then ask: **"Proceed with this mode? (yes to start, or cancel to change mode)"**.

If the user wants to change:
- Memory: cancel, re-invoke the skill with or without `--no-memory`.
- Rehearsal ON → OFF: cancel, exit this Claude Code session, start a new one in the real shop checkout.
- Rehearsal OFF → ON: cancel, re-invoke with `--rehearsal [<label>]` and follow the printed setup commands.

Only proceed past this gate after explicit confirmation (or in autonomous mode).

#### Status Validation

Check the issue's status on the GitHub project board:
- **Ready** → move it to **In Progress** and proceed.
- **In Progress** → proceed (resume case).
- **Any other status** → STOP. Tell the user the issue is in status `<status>` and ask whether to proceed.

#### Resume Detection

Before doing anything else, scan the test repository for `@Disabled` annotations with known phase markers (see cycles.md resume detection table). If found, skip to the indicated phase. If not found, proceed from step 1.

### Step 1: Story

Launch **atdd-story** with the issue number. It will read the GitHub issue and produce Gherkin scenarios.
- **Normal mode:** Present the Gherkin scenarios and wait for human approval.
- **Autonomous mode:** Auto-approve and proceed immediately.

After approval, update the issue body with the approved Gherkin scenarios (use `gh issue edit` preserving the user story preamble).

### Step 2: Per-Scenario Loop

For each scenario, follow the AT cycle decision tree from `cycles.md`:

1. **AT - RED - TEST:** Launch atdd-test (WRITE → STOP → COMMIT).
2. **Decision:** DSL Interface Changed? If no → skip to GREEN.
3. **AT - RED - DSL:** Launch atdd-dsl (WRITE → STOP → COMMIT).
4. **Decision:** External System Driver Interface Changed? If yes → run Contract Test Sub-Process.
5. **Decision:** System Driver Interface Changed? If no → skip to GREEN.
6. **AT - RED - SYSTEM DRIVER:** Launch atdd-driver (WRITE → STOP → COMMIT).
7. **AT - GREEN - SYSTEM:** Launch atdd-backend → atdd-frontend → atdd-release.
8. If remaining `// TODO:` scenarios exist, loop back to step 1.

### Escalation

If any agent reports it cannot proceed, STOP and present the blocker to the user — **even in autonomous mode**.
