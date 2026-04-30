Implement the following ticket by delegating cycle-level orchestration to the `gh optivem atdd` driver and dispatching each Claude Code agent the driver pauses on.

Input: $ARGUMENTS

The input is a GitHub issue number (e.g. `#42`), optionally followed by flags. A ticket is always required — free-text stories are not accepted.

The driver loads `docs/atdd/process/process-flow.yaml` and walks every node end-to-end. Mechanical steps (board moves, classification, smoke tests, commits, `@Disabled` removal, issue close) run inline as Go. Creative steps surface as `DISPATCH: <agent-name>` banners and pause on stdin while you launch the named Claude Code agent via the Task tool. When the agent commits its phase, return to the driver's terminal and press Enter.

**Autonomous mode:** if `--autonomous` is present in the input, pass it through to the driver — gates skip human-approval STOPs. Agent-dispatch pauses still apply because v1 of the driver does not auto-launch agents.

**Docs-only mode:** if `--no-memory` is present, run as if no auto-memory exists. Do not apply remembered preferences from `MEMORY.md` or its referenced files; include a literal "Ignore auto-memory; rely solely on `docs/atdd/**` and the ticket." line in every Task-tool prompt.

**Rehearsal mode:** if `--rehearsal` is present (optionally followed by `<label>`), the skill resolves an id, prints the commands needed to enter a throwaway rehearsal worktree (sibling at `../rehearsal-<id>` on branch `rehearsal/<id>`), and **exits without running the pipeline**. The id is always timestamp-led so rehearsals are sortable and parallel-safe:

- `--rehearsal` alone → `<id>` = `<ts>` (e.g. `20260429-141230`), where `<ts>` comes from `date +%Y%m%d-%H%M%S`.
- `--rehearsal <label>` → `<id>` = `<ts>-<label>` (e.g. `20260429-141230-demo1`). `<label>` must match `[A-Za-z0-9_-]+`.

You then run the printed commands, open a fresh Claude Code session inside the worktree, and re-invoke the skill there (without `--rehearsal`, since the worktree CWD is already enough to put the run in rehearsal mode). The flag is a setup shortcut, not an in-session driver — it cannot run the pipeline against the worktree from your current session because sub-agents inherit the harness's working directory, which is fixed at session start. If the current session is already inside a rehearsal worktree, `--rehearsal` is ignored. Default: no `--rehearsal` flag → commits land in the current repo on the current branch.

**Project & scope flags:** `--project <url>` overrides README/git-remote project resolution and is passed through to the driver. `--architecture <monolith|multitier|both>`, `--system-lang <java|dotnet|typescript|all>`, `--test-lang <java|dotnet|typescript|all>` are scope axes — Claude resolves them in Pre-flight and propagates them into every Task-tool prompt; the driver itself is scope-agnostic.

## Pre-flight

### Run Mode Confirmation

Before any other work, handle the `--rehearsal` fast-path, then detect and confirm the run mode.

#### Rehearsal flag fast-path

Detect whether the current session is already inside a rehearsal worktree by checking BOTH:
- repo root basename matches `rehearsal-*` (run `basename "$(git rev-parse --show-toplevel)"`)
- current branch matches `rehearsal/*` (run `git rev-parse --abbrev-ref HEAD`)

If either matches → `in_rehearsal = true`. Otherwise `in_rehearsal = false`.

Now look for `--rehearsal` in `$ARGUMENTS` and resolve the rehearsal `<id>`:
1. Always generate the timestamp prefix: `<ts>` = output of `date +%Y%m%d-%H%M%S` (e.g. `20260429-141230`).
2. If `--rehearsal` is followed by a token that does NOT start with `--`, treat that token as `<label>`. Validate it against `[A-Za-z0-9_-]+`; if it fails validation, STOP with an error explaining usage: `--rehearsal [<label>]` (e.g. `--rehearsal demo1`). Set `<id>` = `<ts>-<label>`.
3. Otherwise (no token, or the next token is another flag): set `<id>` = `<ts>`.

Then act:
- If `--rehearsal` is present AND `in_rehearsal = false` → **print the setup commands plus a read-only preview of the Run Mode and Scope as they will resolve inside the worktree, then EXIT immediately**. Do nothing else (no status validation, no story, nothing). The preview is advisory — actual confirmation still happens in the fresh session, where Claude's CWD is the worktree. Showing it here lets the user spot a wrong default before bothering to spin up the worktree.

Resolve the preview values exactly as the inner session would resolve them at its own gates:
- **Memory**: `OFF` if `--no-memory` is in `$ARGUMENTS`, else `ON`.
- **Rehearsal**: always `ON` (the inner session will be inside the worktree).
- **Architecture / System Lang / Test Lang**: honour any `--architecture` / `--system-lang` / `--test-lang` flags from `$ARGUMENTS`; otherwise use the rehearsal defaults (`Architecture=multitier`, `System Lang=java`, `Test Lang=typescript`). For each axis, "other options" lists the remaining values for that axis (always spelled out fully, never abbreviated).
- **Worktree path**: `<parent>/rehearsal-<id>`, where `<parent>` is the parent directory of the current repo root.

Render the block flush-left in your reply (no leading whitespace, no indentation, no list markers around the code fence) so the commands paste cleanly into a shell. Keep the three setup commands on three separate lines — short lines won't wrap in the terminal, and the user can still select all three and paste in one go (newlines act as Enter). Do NOT join them with `&&` into a single long line; the wrap is worse than the multi-line. Use this exact shape:

```
Rehearsal mode requested. Select all three lines and paste once:

./scripts/atdd-rehearsal-start.sh <id>
cd ../rehearsal-<id>
claude

Then in the new Claude Code session, re-invoke the skill (without --rehearsal):

/atdd:atdd-implement-ticket #<N> <other-flags>

Preview of how the inner session will resolve mode and scope (confirmation still happens there):

Run mode for issue #<N>:
  Memory:    <ON|OFF>   (default: ON  — applies MEMORY.md preferences; --no-memory to disable)
  Rehearsal: ON          (commits land in <worktree-path> on branch rehearsal/<id>; scripts/atdd-rehearsal-end.sh <id> to discard)

Scope for issue #<N>:
  Architecture: <value>   (other options: <full|tokens|comma|separated>)
  System Lang:  <value>   (other options: <full|tokens|comma|separated>)
  Test Lang:    <value>   (other options: <full|tokens|comma|separated>)

Nothing has been done in this session. The fresh session is required so Claude Code's working directory becomes the rehearsal worktree — otherwise sub-agents would commit into your real repo.
```

- If `--rehearsal` is present AND `in_rehearsal = true` → ignore the flag (you're already in a rehearsal) and proceed.
- If `--rehearsal` is absent → proceed.

#### Confirmation block

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

### Scope Confirmation

After Run Mode is confirmed, determine and confirm the **scope** — which architecture(s), system implementation language(s), and test language(s) the pipeline will touch. Defaults are **rehearsal-aware**: single-implementation in rehearsal mode (fast iteration on one stack), full fan-out outside rehearsal (production runs touch all parallel implementations).

**Defaults**:
- Rehearsal mode (`Rehearsal: ON`): `Architecture=multitier`, `System Lang=java`, `Test Lang=typescript`.
- Outside rehearsal (`Rehearsal: OFF`): `Architecture=both`, `System Lang=all`, `Test Lang=all`.

**Flag overrides** — present in `$ARGUMENTS` to override the default for that axis (and skip the per-axis prompt for it):
- `--architecture <monolith|multitier|both>`
- `--system-lang <java|dotnet|typescript|all>`
- `--test-lang <java|dotnet|typescript|all>`

In autonomous mode, skip the confirmation prompt entirely and use the mode-appropriate defaults (still honouring any explicit flags). Print the resolved scope and proceed.

Otherwise, print the resolved values and ask for confirmation. Use this exact shape:

```
Scope for issue #<N>:
  Architecture: <value>   (other options: <full|tokens|comma|separated>)
  System Lang:  <value>   (other options: <full|tokens|comma|separated>)
  Test Lang:    <value>   (other options: <full|tokens|comma|separated>)

Proceed with this scope? (yes / change / all)
```

Always spell options out fully (`monolith|multitier|both`, never `m|mt|both`).

- `yes` → proceed with the displayed scope.
- `change` → ask the user for each axis individually, then re-display the block and re-ask.
- `all` → set every axis to the broadest option (`Architecture=both`, `System Lang=all`, `Test Lang=all`) and proceed.

Only proceed past this gate after explicit confirmation (or in autonomous mode).

## Run the driver

After both pre-flight gates pass, invoke the driver in the user's terminal:

```
gh optivem atdd implement-ticket --issue <N> [--project <url>] [--autonomous]
```

The driver handles status validation (move card to In Progress), resume detection (`@Disabled` markers), classification, the AT cycle and CT sub-process, the structural cycle, the per-scenario loop, the smoke test, and release (`@Disabled` removal + commit + close). Orchestration prose previously in this skill (the AT cycle decision tree, CT sub-process, scenario loop, STOP behaviour, resume detection table, phase-to-agent mapping) lives in `docs/atdd/process/process-flow.yaml` and is enforced by the binary; do not re-derive it here.

While the driver runs, watch its stdout for two banner shapes:

1. **`DISPATCH: <agent-name>`** — the driver has paused at a user-task node and wants you to launch a Claude Code agent. Read the banner's `Phase`, `Phase doc`, and `<agent-name>`, then dispatch the agent via the Task tool with this prompt template:

   ```
   Issue: #<N>
   Phase: <description from banner>
   Phase doc: <phase_doc from banner>

   Scope: Architecture=<value>, System Lang=<value>, Test Lang=<value>
   Restrict all file edits, residual-reference greps, compile checks, and sample-suite runs to in-scope paths only. Out-of-scope implementations are NOT to be modified or tested in this run.
   ```

   If `--no-memory` is set, append:

   ```
   Ignore auto-memory; rely solely on `docs/atdd/**` and the ticket.
   ```

   Wait for the agent to commit its phase (the COMMIT lands on HEAD with the canonical `<ticket-title> | <PHASE>` message). Then return to the driver's terminal and press Enter to continue. If the agent reports it cannot proceed, type `abort` at the driver's prompt and surface the blocker to the user — **even in autonomous mode**.

2. **`Can I commit? [y/N]:`** or any other interactive prompt — the driver is asking the user a deterministic yes/no/value question. In autonomous mode, answer the documented default; otherwise relay the question to the user and pass their answer back to the driver. Do NOT auto-answer "yes" without confirmation; the academy's "ask before every commit" gate is firm policy.

### Phase Task Tracking

Maintain a `TaskCreate` list of the phases the driver walks so the user can see progress at a glance. The banner prints the phase name and doc; create a task per phase as it begins, mark it `in_progress`, and tick it `completed` the moment the agent commits and the driver advances. Skipped phases (driver routed past them via a gate) get the `completed` status with a note that they were skipped. Never leave a previous phase `in_progress` while starting the next one.

### Escalation

If the driver exits non-zero, surface the error to the user and stop. Do not auto-retry. If a Task-tool agent reports it cannot proceed, type `abort` at the driver's prompt — the binary halts cleanly and the user can resume from the same node after fixing the blocker.
