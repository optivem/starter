Pick the top ticket from the GitHub project board and run it through the ATDD pipeline driver.

Input: $ARGUMENTS

This skill is a thin wrapper over `gh optivem atdd manage-project`. The driver picks the top item from the Ready column, moves it to In Progress, and walks `docs/atdd/process/process-flow.yaml` end to end — same shape as `/atdd:atdd-implement-ticket` once the issue is resolved.

**Autonomous mode:** if `--autonomous` is present, pass it through to the driver — gates skip human-approval STOPs. Agent-dispatch pauses still apply because v1 of the driver does not auto-launch agents.

**GitHub project:** optionally specify `--project <url>` to identify which GitHub project board to use (e.g. `--project https://github.com/orgs/optivem/projects/3`). Pass it through to the driver verbatim. If not specified, the driver discovers the project via `README.md` first, then `git remote get-url origin`.

## Run the driver

```
gh optivem atdd manage-project [--project <url>] [--autonomous]
```

The driver:
1. Resolves the project URL.
2. Reads the Ready column and picks the top item.
3. Moves it to In Progress.
4. Walks the same flow as `/atdd:atdd-implement-ticket`: service tasks inline, user tasks paused at `DISPATCH: <agent>` banners.

When the driver pauses at a `DISPATCH:` banner, dispatch the named agent via the Task tool using the prompt template documented in `/atdd:atdd-implement-ticket` ("Run the driver" → "DISPATCH"). Wait for the agent's COMMIT, then return to the driver's terminal and press Enter.

This skill does not duplicate the Run Mode Confirmation or Scope Confirmation gates — `/atdd:atdd-implement-ticket` runs them once the issue is known. For board-mode runs, the gates surface in the same session immediately after the driver picks the ticket.

## Rules

- If the Ready column is empty, the driver exits with a "nothing to do" message — surface it and stop.
- If the driver exits non-zero, surface the error and stop. Do not auto-retry — even in autonomous mode.
- If a Task-tool agent reports it cannot proceed, type `abort` at the driver's prompt to halt cleanly.
