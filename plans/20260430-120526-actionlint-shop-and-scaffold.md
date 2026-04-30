# Plan — Add `actionlint` to shop CI and to gh-optivem scaffold verification

**Status:** Proposed
**Date:** 2026-04-30
**Trigger:** [gh-optivem run 25158207965 — phase 8 failure](https://github.com/optivem/gh-optivem/actions/runs/25158207965/job/73745520010)
**Related fix run (validated):** [test-app-3143fd2b prod-stage 25159379232](https://github.com/valentinajemuovic/test-app-3143fd2b-7ed5ea281e1522d3/actions/runs/25159379232) — green after manually patching the broken `uses:` reference

---

## Problem

A latent bug in gh-optivem's scaffolder produced a syntactically valid but unrunnable workflow in every scaffolded test-app: `prod-stage.yml` contained `uses: ./.github/workflows/bump-patch-version-monolith-typescript.yml` after the file itself had been renamed to `bump-patch-version.yml`. The mistake survived all of:

- shop CI (no workflow linter runs)
- the gh-optivem unit tests (`replacements_test.go` only asserts string substitution, not cross-file resolution)
- the `Verify local compilation` phase in gh-optivem (compiles source, doesn't validate workflows)
- the smoke phases of `gh-acceptance-stage` (commit/acceptance/QA stages don't reference `bump-patch-version.yml`)

It only surfaced 10+ minutes into phase 8 when GitHub itself parsed the workflow at dispatch time and returned HTTP 422 *workflow was not found*. Same class of error would silently break a student repo at the equivalent point.

**Static detection exists.** [`actionlint`](https://github.com/rhysd/actionlint) resolves every `uses: ./.github/workflows/*.yml` against the local directory and fails fast when the target is missing. It would have caught this exact bug in seconds.

The sibling `actions` repo already uses `actionlint` v1.7.7 in its commit-stage (see `actions/.github/workflows/commit-stage.yml`), so there is an established pattern to copy.

---

## Scope

Two installations, sharing a pinned version:

1. **Shop repo** — gate every PR / push on shop's own workflows being lint-clean. Catches authoring bugs in shop *before* they propagate into a scaffold cycle.
2. **gh-optivem scaffolder** — lint the *scaffolded* test-app immediately after `apply_template.go` runs and before the test-app is pushed. Catches scaffolder bugs (broken `uses:` rewrites, missing files, etc.) at the local-verification phase, not 10 minutes into phase 8.

Out of scope:
- Fixing the original `bump-patch-version-<flavor>` rewrite bug — that's a separate plan (or follow-up commit) tracking the missing replacement rules in `monolithContentReplacements` (line 512) and `multitierContentReplacements` (line 634) of `gh-optivem/internal/steps/apply_template.go`.
- Adding `shellcheck` (the `actions` repo also runs it; could be a follow-up).
- Linting downstream student repos after scaffold — not gh-optivem's job; the scaffolder validates the artifact it produced.

---

## Design choices

### Pinned version
`v1.7.7` — match what the `actions` repo already pins, so a future bump moves both repos together.

### Install method
The `actions` repo uses the upstream installer script:

```bash
bash <(curl -fsSL https://raw.githubusercontent.com/rhysd/actionlint/v1.7.7/scripts/download-actionlint.bash) 1.7.7
```

Reuse verbatim in shop CI. For gh-optivem (which runs locally on the user's machine *and* on the gh-acceptance runner), prefer:
- **In CI:** same installer script as shop, in the gh-acceptance pipeline.
- **Local:** auto-detect `actionlint` on PATH; if absent, print one-line install instructions and fail the step. Don't auto-install on dev machines.

### Failure semantics
- Shop CI: hard fail (PR can't merge with a broken workflow).
- gh-optivem: hard fail (test-app push aborts; scaffolder run terminates).

### Config file
Add `.github/actionlint.yaml` (currently absent in shop) only if needed to silence specific noise — start with no config and tune if false positives appear. The `actions` repo runs without one.

---

## Part 1 — Shop CI

### 1.1 Workflow file
Create `.github/workflows/lint-workflows.yml`:

```yaml
name: lint-workflows

on:
  pull_request:
    paths:
      - '.github/workflows/**'
      - '.github/actionlint.yaml'
  push:
    branches: [main]
    paths:
      - '.github/workflows/**'
      - '.github/actionlint.yaml'

permissions:
  contents: read

concurrency:
  group: lint-workflows-${{ github.ref }}
  cancel-in-progress: true

jobs:
  actionlint:
    runs-on: ubuntu-latest
    timeout-minutes: 5
    steps:
      - uses: actions/checkout@v6

      - name: Install actionlint
        run: |
          set -euo pipefail
          bash <(curl -fsSL https://raw.githubusercontent.com/rhysd/actionlint/v1.7.7/scripts/download-actionlint.bash) 1.7.7
          ./actionlint --version

      - name: Lint workflow files
        run: ./actionlint -color
```

Notes:
- `paths` filter avoids running on every doc / source change.
- `permissions: contents: read` (pin minimum, follow `actions` repo).
- `concurrency` cancels superseded PR runs.
- `timeout-minutes: 5` — actionlint completes in seconds; this catches a stuck install.

### 1.2 Validate locally before merge
Per CLAUDE.md "Pre-Commit Verification": after creating the workflow, run `actionlint` locally against shop's current `.github/workflows/*.yml`. Fix every existing violation it finds before the workflow is committed. If a violation is intentional and unfixable, suppress it via `.github/actionlint.yaml` (commit the config alongside the workflow with a one-line `# why:` comment).

### 1.3 Optional: pre-commit hook
Out of scope for this plan, but: a future `.pre-commit-config.yaml` entry pointing at `actionlint` would catch issues even earlier. Track separately if desired.

---

## Part 2 — gh-optivem scaffold verification

### 2.1 New step in the pipeline

Add a `VerifyScaffoldWorkflows` step that runs **after** `apply_template.go` has written the scaffolded test-app's `.github/workflows/` and **before** the first `git push` to the scaffolded repo. Sequencing:

```
... → ApplyTemplate → VerifyCompilation → VerifyScaffoldWorkflows → Push → VerifyCommitStage → ...
                                          ^^^^^^^^^^^^^^^^^^^^^^^^^ NEW
```

Rationale for placement:
- After `apply_template.go`: the workflows have been written and rewritten.
- After `VerifyCompilation`: keep all local-verification steps grouped under the `phaseVerifyLocal` phase.
- Before any push or stage trigger: a lint failure aborts the run before GitHub or external systems are touched.

### 2.2 Implementation

New function in `gh-optivem/internal/steps/verify.go`:

```go
// VerifyScaffoldWorkflows lints the scaffolded test-app's workflow files
// using actionlint. Catches broken `uses:` references, invalid syntax, and
// other static issues that would otherwise only surface 10+ minutes into
// the verification pipeline at workflow-dispatch time (HTTP 422).
func VerifyScaffoldWorkflows(cfg *config.Config) {
    log.Info("Linting scaffolded workflows with actionlint...")

    if cfg.DryRun {
        log.Info("[DRY RUN] Would run actionlint against scaffolded .github/workflows/")
        return
    }

    if _, err := exec.LookPath("actionlint"); err != nil {
        log.Fatalf("actionlint not found on PATH. Install with: bash <(curl -fsSL https://raw.githubusercontent.com/rhysd/actionlint/v1.7.7/scripts/download-actionlint.bash) 1.7.7")
    }

    workflowDirs := scaffoldWorkflowDirs(cfg)
    for _, dir := range workflowDirs {
        if _, err := os.Stat(dir); errors.Is(err, os.ErrNotExist) {
            continue
        }
        out, err := shell.Run("actionlint -color", false, true, filepath.Dir(dir))
        if err != nil {
            log.Errorf("actionlint failed in %s:\n%s", dir, out)
            os.Exit(1)
        }
    }
    log.Success("Scaffolded workflows pass actionlint")
}

// scaffoldWorkflowDirs returns every .github/workflows directory the
// scaffolder writes — typically one (monolith / multitier-monorepo) or two
// (multitier-multirepo: backend + frontend).
func scaffoldWorkflowDirs(cfg *config.Config) []string {
    dirs := []string{filepath.Join(cfg.RepoDir, ".github", "workflows")}
    if cfg.Arch != "monolith" && cfg.RepoStrategy == "multirepo" {
        dirs = append(dirs,
            filepath.Join(cfg.BackendRepoDir, ".github", "workflows"),
            filepath.Join(cfg.FrontendRepoDir, ".github", "workflows"),
        )
    }
    return dirs
}
```

Wire it up in `main.go` next to the existing local-verification step:

```go
stepDef{name: "Verify local compilation", phase: phaseVerifyLocal, fn: func() { steps.VerifyCompilation(cfg) }},
stepDef{name: "Verify scaffolded workflows", phase: phaseVerifyLocal, fn: func() { steps.VerifyScaffoldWorkflows(cfg) }},  // NEW
```

### 2.3 Make actionlint available on the gh-acceptance runner

`gh-optivem/.github/workflows/_gh-acceptance-pipeline.yml` (the reusable pipeline behind `gh-acceptance-stage` / `gh-acceptance-dry-run`) needs actionlint installed on the runner before `gh-optivem` invokes it. Add an install step before the existing scaffold/run step:

```yaml
- name: Install actionlint
  run: |
    set -euo pipefail
    bash <(curl -fsSL https://raw.githubusercontent.com/rhysd/actionlint/v1.7.7/scripts/download-actionlint.bash) 1.7.7
    sudo mv ./actionlint /usr/local/bin/actionlint
    actionlint --version
```

(Move into `/usr/local/bin` so the scaffolder's `exec.LookPath("actionlint")` resolves it without path gymnastics.)

### 2.4 Local-dev path
Document the actionlint dependency in `gh-optivem/README.md` next to the existing prereqs (Go, gh, docker). One line: install command + version.

---

## Part 3 — Validate the rollout

Before declaring done:

1. **Shop:** open a PR that intentionally adds a broken `uses:` reference in a workflow → confirm `lint-workflows` job fails with a recognisable error → revert.
2. **gh-optivem:** introduce a deliberate filename rewrite skip (e.g., comment out one of the bump-patch-version rename rules) → run `gh-optivem` locally → confirm `Verify scaffolded workflows` step fails before push, with the same error class GitHub returned at dispatch.
3. **gh-optivem CI:** re-run `gh-acceptance-stage` against the current shop tip → confirm phase 8 still reaches QA signoff (i.e., the new step doesn't false-fail on shop's own scaffolded output once the underlying bump-patch-version rewrite bug is also fixed).

---

## Effort

| Part | Files touched | LOC est. |
|---|---|---|
| Shop `lint-workflows.yml` | 1 new | ~25 |
| Fix existing shop violations (if any) | TBD after first lint | ? |
| gh-optivem `VerifyScaffoldWorkflows` | `verify.go`, `main.go` | ~40 |
| gh-optivem pipeline runner install | `_gh-acceptance-pipeline.yml` | ~5 |
| gh-optivem README note | `README.md` | ~3 |

Half-day total assuming shop's existing workflows are clean. If actionlint flags pre-existing issues, those become a separate follow-up plan rather than blocking this one — gate the new shop workflow on a single configurable allow-list until the backlog is cleared.

---

## Risks / open questions

- **Pre-existing shop violations.** Likely some — the shop repo has 80+ workflow files and has never been linted. Mitigation: dry-run actionlint locally first; if the count is small, fix in this plan; if large, ship the workflow with `actionlint.yaml` ignores and a follow-up plan.
- **`uses: optivem/actions/...@v1` external resolution.** actionlint by default tries to resolve `uses:` against GitHub. In CI the runner has network; locally, dev machines may not. Add `-shellcheck=` or `-pyflakes=` flags only if specific noise appears — start without flags.
- **Scaffolder running offline.** If `gh-optivem` is invoked with `--dry-run` or in airgapped scenarios, the lint step short-circuits via the existing `cfg.DryRun` guard.
- **actionlint version drift between shop and `actions`.** Both pin `v1.7.7` today. A future bump must coordinate; track the pin in one place (a `VERSIONS.md` or a Renovate config) if drift becomes a problem.

---

## Out-of-scope follow-ups (for separate plans)

1. Fix the actual `bump-patch-version-<flavor>` rewrite bug — add reverse-order rules at `apply_template.go:512` (monolith) and `:634` (multitier) and a regression test in `replacements_test.go` covering the `uses: ./.github/workflows/bump-patch-version-<flavor>.yml` substitution.
2. Add `shellcheck` to shop CI (mirror `actions` repo).
3. Pre-commit hook for `actionlint` in shop and gh-optivem (developer-side fast-fail).
