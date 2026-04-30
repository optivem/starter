# Move scaffold-only workflows to `.github/workflows/templates/`

## Decision

Relocate the 8 scaffold-only workflow files from `.github/workflows/` to a new `.github/workflows/templates/` subdirectory. GitHub Actions only parses top-level `.github/workflows/*.yml` files as workflows — files in subdirectories are ignored. This structurally guarantees scaffold files cannot be dispatched in shop, removes them from the Actions UI, and removes them from "scan all workflows" lints — without renaming, guard steps, or convention-by-convention.

Rationale:

- **Naming alone can't solve it.** Renaming with prefixes (`_scaffold-*.yml`) or suffixes (`*.template.yml`) does not stop GHA from parsing the file as a workflow. Adding `if:` guards adds noise and depends on memory/discipline.
- **GitHub's official "templates" mechanism doesn't fit.** GitHub starter workflows live at `.github/workflow-templates/` in an organisation's special `.github` repo and are surfaced via the GitHub UI's "New workflow" button. shop's scaffolds are scaffold-tool-driven (gh-optivem), not GitHub-UI-driven. Different mechanism.
- **Subdirectory non-scanning is the clean structural fix.** No naming gymnastics; the file tree itself signals intent (`templates/` = source for tooling, not executable here).
- **Discoverability stays high.** Templates remain next to the active workflows they parallel (single `.github/workflows/` parent), so the relationship is visually obvious.

## Scope

### Files to move (8 scaffold-only)

| Current path | New path |
|---|---|
| `.github/workflows/bump-patch-version-multirepo.yml` | `.github/workflows/templates/bump-patch-version-multirepo.yml` |
| `.github/workflows/bump-patch-version-multitier-dotnet.yml` | `.github/workflows/templates/bump-patch-version-multitier-dotnet.yml` |
| `.github/workflows/bump-patch-version-multitier-java.yml` | `.github/workflows/templates/bump-patch-version-multitier-java.yml` |
| `.github/workflows/bump-patch-version-multitier-typescript.yml` | `.github/workflows/templates/bump-patch-version-multitier-typescript.yml` |
| `.github/workflows/bump-patch-version-multitier-backend-dotnet.yml` | `.github/workflows/templates/bump-patch-version-multitier-backend-dotnet.yml` |
| `.github/workflows/bump-patch-version-multitier-backend-java.yml` | `.github/workflows/templates/bump-patch-version-multitier-backend-java.yml` |
| `.github/workflows/bump-patch-version-multitier-backend-typescript.yml` | `.github/workflows/templates/bump-patch-version-multitier-backend-typescript.yml` |
| `.github/workflows/bump-patch-version-multitier-frontend-react.yml` | `.github/workflows/templates/bump-patch-version-multitier-frontend-react.yml` |

### Files that stay at top level (active workflows, even though their headers say "scaffold")

These three are dual-role: they ARE scaffold sources for monolith student repos, but they are ALSO called by `meta-bump-all.yml` in shop (`uses: ./.github/workflows/bump-patch-version-monolith-{lang}.yml`). Moving them would break shop's own `meta-bump-all.yml`. Update the header comments to clarify the dual role; do not move.

- `bump-patch-version-monolith-dotnet.yml`
- `bump-patch-version-monolith-java.yml`
- `bump-patch-version-monolith-typescript.yml`

### gh-optivem coordination

`gh-optivem/internal/templates/` (specifically `CopyWorkflows`, source: `filepath.Join(shop, ".github", "workflows")`) currently reads scaffold sources from the top-level workflows directory. After this move, gh-optivem must look in the `templates/` subdirectory for the 8 relocated files.

Two implementation options:

1. **Source-path lookup map** — extend `CopyWorkflows` (or add a sibling `CopyTemplates`) that reads from `.github/workflows/templates/` for the 8 known scaffold filenames. Active workflows still copy from the top level. Most explicit.
2. **Fallback search** — `CopyWorkflows` tries top-level first, falls back to `templates/` subdirectory. Less explicit but tolerant of partial migration.

Option 1 preferred — explicit source paths are easier to reason about and less likely to silently pick up the wrong file if a name collision ever arises.

Specific call sites to update in `gh-optivem/internal/steps/apply_template.go`:

| Line | Current source filename | Action |
|---|---|---|
| 173 | `bump-patch-version-{prefixMonolith}{lang}.yml` | Stays at top level (dual-role). No change. |
| 224 | `bump-patch-version-multirepo.yml` | Read from `templates/` |
| 277 | `bump-patch-version-{prefixMonolith}{lang}.yml` | Stays at top level. No change. |
| 309 | `bump-patch-version-{prefixMultitier}{backendLang}.yml` | Read from `templates/` |
| 366 | `bump-patch-version-multirepo.yml` | Read from `templates/` |
| 422 | `bump-patch-version-{prefixMultitierBackend}{backendLang}.yml` | Read from `templates/` |
| 447 | `bump-patch-version-{prefixMultitierFrontend}{frontendLang}.yml` | Read from `templates/` |

## Phase 0 — Verification (do this BEFORE moving any files)

GitHub Actions' "subdirectory not scanned" behavior is widely understood but worth confirming empirically before relying on it. Two-minute check:

1. Add a trivial dispatchable workflow at `.github/workflows/_check/sub-scan-test.yml`:
   ```yaml
   name: sub-scan-test
   on: workflow_dispatch
   jobs:
     noop:
       runs-on: ubuntu-latest
       steps:
         - run: echo "should not be reachable"
   ```
2. Commit, push.
3. Open the GitHub Actions UI: confirm `sub-scan-test` does **not** appear in the workflow list.
4. Try to dispatch via API: `gh workflow run sub-scan-test --repo optivem/shop` — should fail with "no workflow with that name".
5. Delete the test file.

If the test workflow does appear in the UI or is dispatchable, abort this plan and reconsider — the architectural assumption is invalid and a different approach (e.g. `if: github.repository != …` guards) is needed.

## Implementation order

1. **Phase 0 verification** (above) — confirm subdirectory non-scanning. Required gate.
2. **Single shop commit:**
   - `git mv` the 8 scaffold files into `.github/workflows/templates/`.
   - Update header comments in the moved files to reference the new path (search for self-references like "this file" or "in `.github/workflows/`").
   - Update header comments in the three dual-role monolith files to clarify their dual role (scaffold source + active workflow called by `meta-bump-all.yml`).
3. **Single gh-optivem commit:**
   - Update `apply_template.go` so the 6 affected scaffold copy operations read from `.github/workflows/templates/` instead of `.github/workflows/`.
   - Add a unit test or at minimum exercise an end-to-end `gh optivem init` against each scaffold variant (monolith-multirepo, multitier-monorepo, multitier-multirepo) in a throwaway target dir; verify each generated student repo gets the expected `bump-patch-version.yml`.
   - Re-publish the gh-optivem CLI tag (rebuild + tag + release).
4. **Backwards compatibility for in-flight scaffolds:** none required — students who already ran `gh optivem init` with the previous CLI tag have a copy of the scaffold output committed to their repo; they're unaffected by where the source lived. Only the next student running `gh optivem init` will pull the new tag.
5. **Verification** — see below.

## Verification

- After the shop commit: GitHub Actions UI for shop shows **8 fewer** dispatchable workflows. Dispatching `bump-patch-version-multirepo`, `bump-patch-version-multitier-typescript`, etc. via UI or `gh workflow run` fails with "no workflow with that name."
- `meta-bump-all.yml` in shop still dispatches successfully end-to-end (the three dual-role monolith bumpers were not moved).
- After the gh-optivem CLI re-publish: a fresh `gh optivem init` for each of the four scaffold variants — monolith-monorepo, monolith-multirepo, multitier-monorepo, multitier-multirepo — produces a student repo with the expected `bump-patch-version.yml` (or `bump-patch-version-multirepo.yml` for multirepo roots), matching the pre-move scaffold output byte-for-byte (or with only intended diffs).
- A pre-move `gh-acceptance-stage` baseline run vs. a post-move run in a student-multirepo target should produce identical scaffold output.

## Out of scope

- Moving the three dual-role monolith bumpers (`bump-patch-version-monolith-{dotnet,java,typescript}.yml`). They remain at top level because shop's own `meta-bump-all.yml` calls them.
- Renaming any scaffold files. Move only.
- Changing `bump-patch-version.yml` (multitier omnibus) — it's an active workflow called from multitier prod-stages.
- Re-organising other scaffold-style files in shop (e.g. anything in `docker/`, `system/`, `system-test/`). Out of scope for this plan; if a similar concern exists there, file separately.
- Migrating to GitHub's official `workflow-templates/` mechanism in a `.github` org repo. Different surface, different audience (interactive GitHub UI users vs. gh-optivem CLI users).
