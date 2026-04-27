# Plan: consolidate ATDD Claude assets in shop

🤖 **Picked up by agent** — `Valentina_Desk` at `2026-04-27T07:31:40Z`

> **Scope**: Phase 1 only — make shop the canonical source of truth for ATDD Claude assets. Downstream CLI work (`gh optivem atdd install/upgrade` and `init` wiring) lives in `gh-optivem/plans/20260427-091546-atdd-install-cli.md`.

## Problem

`eshop-tests/.claude/` contains an ATDD multi-agent pipeline (~21 files, ~786 lines). We want it to live in **shop** as canonical source — shop is the new SUT replacing eshop, and the doctrine is exercised against shop's live code.

Source content in `eshop-tests/`:

- `.claude/agents/`: backend, frontend, driver, dsl, manager, release, story, test (8 files).
- `.claude/commands/`: implement-ticket, manage-project (2 files).
- `docs/prompts/atdd/`: orchestrator(+diagram), acceptance-tests, contract-tests (4 files).
- `docs/prompts/architecture/`: driver-adapter, driver-port, dsl-core, dsl-port, test (5 files).
- `docs/prompts/code/language-equivalents.md` (1 file).
- `docs/prompts/glossary.md` (1 file — ATDD glossary; placed under `atdd/` in destination).

Total **21 files**. Most is domain-agnostic; references to `eshop` / `eshop-tests` / `Run-SystemTests.ps1` are rewritten to shop equivalents so the install-time substitution (handled by the gh-optivem CLI) becomes straightforward.

## Install layout in shop

```
.claude/
  agents/                  ← managed (atdd- prefix): atdd-backend.md, atdd-frontend.md,
                              atdd-driver.md, atdd-dsl.md, atdd-manager.md,
                              atdd-release.md, atdd-story.md, atdd-test.md
  commands/                ← managed (atdd- prefix): atdd-implement-ticket.md,
                              atdd-manage-project.md
docs/prompts/
  atdd/                    ← managed: orchestrator.md, orchestrator-diagram.md,
                              acceptance-tests.md, contract-tests.md, glossary.md
  architecture/            ← managed: driver-adapter.md, driver-port.md,
                              dsl-core.md, dsl-port.md, test.md
  code/                    ← managed: language-equivalents.md
```

`.claude/agents/` and `.claude/commands/` use a flat layout with `atdd-` filename prefix because Claude Code's documentation does not confirm subdir support for agents/commands (verified 2026-04-27). The `atdd-` prefix marks ownership; shop's pre-existing non-ATDD agents (compare-*, editor, monitor, docs/) stay at the root unchanged.

`docs/prompts/` keeps subdirectory layout — these are doctrine docs, not Claude Code agent/command files, so subdirectories are fine.

**Ownership rule** (enforced by the downstream CLI): every file matching `.claude/agents/atdd-*.md`, `.claude/commands/atdd-*.md`, and every file under `docs/prompts/{atdd,architecture,code}/` is owned by `gh optivem atdd`. Students copy out and rename to customize.

**Frontmatter convention**: each `atdd-<name>.md` agent file has `name: atdd-<name>` in its YAML frontmatter, matching the filename for unambiguous discovery regardless of which (filename vs. frontmatter) Claude Code uses for agent identification.

## Tasks

### Phase 1 — consolidate canonical assets in shop

- [ ] Decide what to do with the `shop/` package convention referenced in the prompts — it's a generic ATDD term (the SUT subfolder, distinct from `external/`), not the repo name. Keep as-is. Document the distinction in `docs/prompts/atdd/orchestrator.md` if not already clear.

## Open questions

1. **Templating scope.** Beyond `eshop`/`eshop-tests` and `Run-SystemTests.ps1`, are there any other domain-specific bits the gh-optivem CLI would need to substitute? Re-grep during Phase 1.5/1.6.

## Non-goals

- Building the gh-optivem CLI subcommands (separate plan).
- Changing the ATDD doctrine itself.
- Updating eshop-tests (archived per memory).
- Building a per-language ATDD variant.
