# Rename `optivem/starter` → `optivem/shop`

## Context

The repo `optivem/starter` has dual purpose: it's the example Shop application students study in courses, and the scaffolding source that `gh-optivem` clones and parameterizes. "Shop" reflects the identity of what's in the repo. Display names also unify: "eShop Starter" / "Shop Starter" → just "Shop".

This rename touches ~100 files across 8 local repos, 16 archived repos on GitHub, GHCR container images, and the VS Code workspace.

---

## Phase 1: Update all local references (before GitHub rename)

Bulk find-and-replace across all repos:
- `optivem/starter` → `optivem/shop` (URLs, repo refs, image paths)
- `optivem_starter` → `optivem_shop` (SonarCloud underscore variant)
- `ghcr.io/optivem/starter/` → `ghcr.io/optivem/shop/` (container images)
- Display names: `eShop Starter` / `Shop Starter` → `Shop`
- Filename rename: `02-shop-starter.md` → `02-shop.md`

### 1.1 — `starter/` repo (~15 files)
- `README.md` — ~48 badge/link URLs
- `system-test/java/docker-compose.pipeline.*.yml` (4 files) — GHCR image refs
- `system-test/dotnet/docker-compose.pipeline.*.yml` (4 files) — GHCR image refs
- `system-test/typescript/docker-compose.pipeline.*.yml` (4 files) — GHCR image refs
- `docs/monitor-process.md` — CLI examples
- `plans/MIGRATION-CLOUD-DEPLOY.md` — references
- `.claude/agents/*.md` — agent instructions referencing starter

### 1.2 — `gh-optivem/` repo (~8 files)
- `internal/config/config.go:555-562` — `cloneStarter()` clones `"optivem/starter"` → change to `"optivem/shop"`
- `internal/steps/replacements.go:22,27,155-158` — hardcoded `"optivem/starter"` strings → `"optivem/shop"`
- `internal/steps/apply_template.go` — comments referencing starter
- `internal/templates/templates.go` — comments referencing starter
- `MAPPING.md` — prose references
- `docs/gh-monitoring-process.md` — CLI examples
- `archived/scaffold/*.py` — legacy Python code references (config.py, templates.py, files.py, replacements.py)
- **Rebuild Go binary** after editing: `cd gh-optivem && go build ./...`

### 1.3 — `courses/` repo (~72 files)
- `docs/rules/00-shared.md:153-159` — canonical name: change "Shop Starter" → "Shop"
- `docs/rules/01-pipeline.md`, `docs/rules/02-atdd.md` — starter references
- `02-atdd/accelerator/course/01-getting-started/02-shop-starter.md` — rename file to `02-shop.md`, update title/content
- `02-atdd/accelerator/course/01-getting-started/03-quick-start.md:11` — "Shop Starter system" → "Shop system"
- `02-atdd/accelerator/course/01-getting-started/04-sandbox-project.md:183` — display text
- ~69 course markdown files with deep links like `https://github.com/optivem/starter/blob/main/...` → `https://github.com/optivem/shop/blob/main/...`
- `pending/*.md` — "eShop Starter project" → "Shop"

### 1.4 — `sandbox/` repo (~3 files)
- `config/projects.json:4` — `"name": "eShop Starter"` → `"name": "Shop"`
- `docs/index.html` — 5 occurrences: "eShop Starter" → "Shop", URLs updated
- `.github/ISSUE_TEMPLATE/review-request.yml:11` — dropdown "eShop Starter" → "Shop"

### 1.5 — `github-utils/` repo (~2 files)
- `README.md` — example commands
- `scripts/delete-packages.sh` — example in comments

### 1.6 — `claude/` repo
- `.claude/commands/update-plan.md` — references to starter

### 1.7 — Commit and push all repos using `/commit` skill

---

## Phase 2: Rename the GitHub repo

```bash
gh repo rename shop --repo optivem/starter --yes
```

Then update local remote and push pending starter commits:
```bash
cd <starter-dir>
git remote set-url origin https://github.com/optivem/shop.git
git push
```

---

## Phase 3: Update 16 archived repos

For each archived repo, unarchive → clone to temp dir → update README (`optivem/starter` → `optivem/shop`) → push → delete temp clone → re-archive.

**Repos to update:**
1. greeter, greeter-java, greeter-dotnet, greeter-typescript
2. greeter-multi-lang, greeter-multi-comp, greeter-multi-repo
3. greeter-multi-repo-backend, greeter-multi-repo-frontend
4. eshop, eshop-tests, eshop-tests-java, eshop-tests-dotnet, eshop-tests-typescript
5. eshop-system-test-java, eshop-monolith-java

---

## Phase 4: GHCR container images

- Trigger CI on `optivem/shop` to build new images under `ghcr.io/optivem/shop/*`
- Old images at `ghcr.io/optivem/starter/*` can be cleaned up later with `delete-packages.sh`

---

## Phase 5: Local directory + workspace rename

1. Close VS Code workspace
2. Rename directory: `starter/` → `shop/`
3. Update `academy.code-workspace`: `"name": "starter"` + `"path": "starter"` → `"shop"`
4. Reopen workspace

---

## Verification

1. Visit `https://github.com/optivem/starter` — should redirect to `optivem/shop`
2. Check CI passes: `gh run list --repo optivem/shop --limit 3`
3. Verify GHCR images: check for `shop/` packages
4. Spot-check course deep links
5. Run `gh-optivem` dry-run scaffold to confirm it clones from `optivem/shop`
6. Verify archived repos show updated READMEs
