# Commit Confirmation Rule

A shared, low-level rule that every committing agent in the ATDD pipeline must follow. Imported directly by leaf agents (`atdd-test`, `atdd-dsl`, `atdd-driver`, `atdd-backend`, `atdd-frontend`, `atdd-task`, `atdd-chore`, `atdd-release`, and any future committing agent).

This rule is intentionally separate from `cycles.md`: that file decides *which* phases run; this file decides *how* the commit step inside any phase is gated. Leaf agents only need this gate, not the routing tables.

## Rule

**No agent may run `git commit` or `gh issue close` without first asking the user "Can I commit?" and receiving an explicit "yes" reply in the same turn.**

This rule applies universally to every COMMIT step in every cycle (AT, CT, System API Task, System UI Task, External API Task, Chore, Legacy Coverage, External System Onboarding, Release).

## Scope: not every GitHub mutation

The rule covers only `git commit` and `gh issue close`. **Other GitHub state mutations — `gh issue edit` to tick checklist items, project-board status moves (e.g. to IN ACCEPTANCE), label changes — are not gated by this rule.** Those are routine post-commit bookkeeping and proceed without re-asking; the agent just does them and informs the user afterwards.

In particular, the IN ACCEPTANCE procedure in [`shared-ticket-status-in-acceptance.md`](shared-ticket-status-in-acceptance.md) — tick checklist + move issue to IN ACCEPTANCE — runs immediately after an already-approved final ticket commit. Asking again at that point would just nag the user; the COMMIT was the gate.

## Procedure

A COMMIT step proceeds as:

1. Stage the changes; show the user the exact commit message and a summary of files touched.
2. Ask: **"Can I commit?"**
3. Wait for an explicit affirmative ("yes", "go ahead", "approved"). Silence or anything ambiguous = **no**.
4. Only after explicit approval, run `git commit`.

## Relationship to STOP

The STOP at the end of a WRITE phase is **not** a substitute for this confirmation. The WRITE-STOP approves the *content*; the commit-confirmation approves the *act of committing*. Both are required. If the user has just approved a WRITE-STOP, still ask "Can I commit?" before running `git commit`.

## Bypass

This rule cannot be bypassed by `--no-verify`, `--amend`, scripts, or wrapping the commit inside another command. Blanket approvals require an explicit user statement and the agent must still re-confirm at every commit boundary it can name in advance.
