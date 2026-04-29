# Scheduled Routine Candidates

Captured from a /schedule planning conversation on 2026-04-29 after the
stderr-swallow sweep routine was being set up. Lists tasks that fit the
Claude Code remote-routine pattern (recurring or one-shot, asynchronous,
low false-positive rate) and would carry their weight.

Selection criteria:
- High signal, low noise — we'd act on the output, not delete it unread.
- Doesn't duplicate Dependabot, SonarCloud, or a native GitHub feature.
- Self-contained — the remote agent can complete it without local state
  beyond what the repo and `gh` API expose.

## Tier 1 — would create today

### 1. Workflow-warnings digest

- **Cadence:** weekly, Mondays 09:00 Europe/Belgrade (07:00 UTC, cron `0 7 * * 1`).
- **Reuses:** existing `workflow-warnings-auditor` agent.
- **Action:** run the auditor, compare warning count and category set to
  the most recent prior `Workflow warnings digest` issue in
  `optivem/shop`. If it grew, open a new digest issue listing each
  warning by workflow:run:line. If unchanged or shrunk, exit silently.
- **Why it earns its slot:** workflow warnings rot until an unrelated
  failure forces someone to scroll through logs. A weekly nudge keeps
  the count from compounding.

### 2. `@Disabled` / `it.skip` / `xit` accumulation sweep

- **Cadence:** weekly, Mondays 09:00 Europe/Belgrade.
- **Action:** count `@Disabled` (Java), `it.skip(`/`xit(`/`describe.skip(`
  (TypeScript), and `[Fact(Skip = ...)]`/`[Theory(Skip = ...)]` (.NET)
  across `system-test/{java,dotnet,typescript}` and the corresponding
  test trees. Compare to the count from the most recent prior issue with
  the title prefix `Disabled-test sweep`. If the count grew, post a new
  issue listing each new disable by file:line with the surrounding
  attribute/comment.
- **Why it earns its slot:** ATDD discipline depends on disables not
  silently piling up. A growing count is a leading indicator of test
  rot the team should triage.

### 3. Cross-language workflow consistency check

- **Cadence:** weekly, Tuesdays 09:00 Europe/Belgrade (07:00 UTC, cron `0 7 * * 2`)
  — offset from #1/#2 so issues don't land in the same Monday batch.
- **Reuses:** existing `workflow-comparator` agent.
- **Action:** run the comparator across Java/.NET/TypeScript GitHub
  Actions workflows. Open an issue if drift is detected, listing each
  divergent workflow trio with the diff hunks.
- **Why it earns its slot:** the CLAUDE.md rule "fix all affected
  languages, not just the one that failed" depends on three-way parity
  that drifts every time someone fixes one language and forgets the
  others.

### Stderr-swallow sweep (already being created in this session)

- **Cadence:** weekly, Mondays 09:00 Europe/Belgrade.
- See `feedback_never_swallow_stderr.md` and the routine being created
  via /schedule for the full prompt.

## Tier 2 — useful but situational

### 4. Cross-language system-test parity

- **Cadence:** first Monday of each month, 09:00 Europe/Belgrade
  (cron `0 7 1-7 * 1` — fires only on the first Monday).
- **Reuses:** `test-comparator` and `repo-comparator` agents.
- **Action:** flag drift between language implementations of the same
  acceptance test (DSL, drivers, channels, scenario shapes).
- **Why monthly not weekly:** test parity rots more slowly than workflow
  parity; weekly would be noise.

### 5. VERSION-vs-released drift

- **Cadence:** weekly, Mondays 09:00 Europe/Belgrade.
- **Action:** for each `system/{multitier,monolith}/*/VERSION` and
  `system/multitier/{backend,frontend}-*/VERSION`, compare to the
  latest published GitHub release tag for that flavour. Flag any
  VERSION ahead of release by >1 minor (stuck release pipeline) or
  behind release (un-bumped after publish).
- **Why it earns its slot:** the meta-prerelease/release pipeline
  depends on VERSION discipline; silent drift breaks future releases.

### 6. Cleanup-orphans dry-run audit

- **Cadence:** weekly, Mondays 09:00 Europe/Belgrade (or daily 06:00 if
  test-load is high).
- **Action:** run `cleanup-orphans.sh --all` in dry-run mode against
  `optivem/gh-optivem`. If the orphan count exceeds a threshold
  (initial: 20 SonarCloud projects, 5 repos, 10 Docker artefacts),
  open an issue.
- **Why it earns its slot:** catches the cleanup workflow itself
  silently failing — exactly the incident pattern that triggered the
  stderr-swallow rule.

## Tier 3 — one-shot follow-ups (`run_once_at`)

### 7. Promotion-state migration cleanup

- **Fire once:** approximately 2026-06-10 (~6 weeks after the migration
  plan was authored).
- **Action:** re-read `plans/20260429-070355-migrate-promotion-state-out-of-git-tags.md`,
  check whether the migration is complete (look for `gh-rc-check-*.sh`
  references in workflows, absence of legacy promotion-state tags).
  If complete, open a PR removing dead promotion-state code. Otherwise
  post a status comment on the plan.
- **Why it earns its slot:** migration follow-ups are the canonical
  "leave a sticky note" use case for one-shot routines.

### 8. `gh-rc-check-*` scripts adoption verification

- **Fire once:** approximately 2026-05-27 (~4 weeks).
- **Action:** verify the new RC-check scripts
  (`gh-rc-check-qa-approved.sh`, `gh-rc-find-latest-qa-approved.sh`,
  `gh-rc-find-latest-qa-deployed.sh`) are referenced by at least one
  workflow. Open an issue listing any unreferenced (zombie) scripts.
- **Pairs with:** #7. If #7 confirms the migration is done, this can
  be deleted instead of fired.

## Tier 4 — skip unless they ring a bell

- **Stale-branch sweep** — usually noise; the issues would be ignored.
- **Sonar warnings digest** — Sonar's own dashboards cover this.
- **Dependency drift digest** — Dependabot covers it; a digest is
  redundant.
- **Pre-commit-hook drift across academy repos** — niche; only useful
  if the hook is meant to spread to other repos.
- **GitHub Actions minute consumption tracker** — only matters near
  the org-level minute cap.

## Recommended starter set

Once `/web-setup` is done, create in this order:

1. The stderr-swallow sweep (already in progress).
2. #1 Workflow-warnings digest.
3. #2 `@Disabled` accumulation sweep.

That gives a coherent "weekly state-of-the-shop" view on Monday morning
without piling on too many routines at once. Add Tier 2 as appetite
allows.

## Cost note

Each routine fire consumes Claude tokens billed against the user's
Claude Code plan / API key. Cadence matters: weekly is the sweet spot
for slow-rot checks; daily is overkill for everything in this list
except possibly #6 (cleanup-orphans) under heavy test load.
