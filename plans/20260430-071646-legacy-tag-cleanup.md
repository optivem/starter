# Legacy Git-Tag Cleanup Plan

One-time cleanup of legacy git tags left over from earlier tagging schemes. Separate from the VERSION layout restructure (`20260430-070642-version-file-layout-restructure.md`) — that plan motivated the audit but the cleanup itself is its own task.

---

## Scope

Two stalled tag families to clean up:

### Family 1: Bare `v*` tags (legacy root-VERSION-as-system tags)

- **Count:** 1,160 tags (`v1.0.0` through `v1.0.19` plus all RC + `-qa-deployed` + `-qa-approved` variants).
- **Last produced:** 2026-04-18, before the prefix-namespace cutover (commits `0aa585bd`, `18fdd83b`).
- **Currently produced by any workflow?** No — confirmed by grepping `.github/workflows/`.
- **GitHub Releases attached:** 20 (one per release tag `v1.0.0`..`v1.0.19`).

### Family 2: `monolith-system-<lang>-v*` (abandoned alternate scheme)

- **Count:** 75 tags across java/dotnet/typescript variants.
- **Last produced:** unknown precise date, stalled at `monolith-system-typescript-v1.0.40`.
- **Currently produced by any workflow?** No — confirmed by grepping `.github/workflows/`.
- **GitHub Releases attached:** 0.

---

## Decisions

### C1. Bare `v*` cleanup — keep release tags + their Releases, delete RC noise

- **Keep** the 20 release tags `v1.0.0` through `v1.0.19` and their attached GitHub Releases as historical record.
- **Delete** the 1,140 RC and `-qa-deployed` / `-qa-approved` variants — pure noise.

### C2. `monolith-system-<lang>-v*` cleanup — delete all 75

No Releases attached, no historical value (alternate scheme that was abandoned in favour of `monolith-<lang>-v*`). Delete entirely.

### C3. Backup before delete

Before any deletion, snapshot the full tag list (tag name + commit SHA) for both families to a backup file under `plans/backups/<timestamp>-deleted-tags.txt`. Commit the backup file to the repo as historical record (small, plain text, useful for forensics). Lets us recreate any tag if we ever need to.

### C4. Dry-run first, then live run

The deletion script supports `--dry-run`. Run dry-run first, capture the list, sanity-check it, then run live.

### C5. Local AND remote deletion

Delete both locally (`git tag -d`) and on the remote (`git push origin :refs/tags/<tag>` or `gh api -X DELETE /repos/.../git/refs/tags/<tag>`). Local-only deletion is meaningless since the remote tags will reappear on next fetch.

### C6. Rate-limit handling

GitHub authenticated REST API limit is 5,000 req/hr. Deleting 1,140 + 75 = 1,215 tags as individual API calls is well within that, but:

- Use the existing `.github/workflows/scripts/gh-retry.sh` wrapper for `gh` calls (handles transient 5xx/network).
- Pace deletes with a small sleep (e.g., `sleep 0.2` between calls) to leave headroom for concurrent CI activity.
- Check remaining rate limit at the start (`gh api /rate_limit`) and bail out early if < 2,000 remaining.
- For mass `git push --delete` of remote tags, prefer batched pushes (50 tags at a time) — git push handles many refs in one round-trip, which is more efficient than per-tag API calls.

### C7. Add legacy-pattern guards to existing cleanup workflow

The existing `.github/workflows/cleanup.yml` runs daily and uses `optivem/actions/cleanup-prereleases@v1`. That action is designed for ongoing prerelease pruning by retention window — it likely doesn't sweep these legacy patterns.

After the one-time cleanup, add a guard: extend `cleanup-prereleases` (in `optivem/actions`) or add a new cleanup step that explicitly errors-or-deletes if any tag matching `^v[0-9]` or `^monolith-system-` reappears. Belt-and-suspenders — no workflow currently produces them, but a guard prevents regression.

### C8. Cleanup runs as a one-off GitHub Action

The cleanup is implemented as a reusable composite action in `optivem/actions`, named to signal its one-off nature: **`optivem/actions/cleanup-legacy-tags-oneoff`**. Following the existing pattern (`cleanup-deployments`, `cleanup-prereleases`, `cleanup-ghcr-orphan-manifests`).

Called from a workflow-dispatch-only shop workflow: **`.github/workflows/cleanup-legacy-tags-oneoff.yml`**. Inputs:

- `dry-run` (bool, default `true`) — print planned deletions, do nothing.
- `confirm` (string, default `''`) — must be set to `DELETE` to actually execute when `dry-run=false` (defence-in-depth against accidental dispatch).

After the one-time cleanup successfully completes, the action and workflow are kept in place (not deleted) — they serve as auditable record and can be re-run safely (idempotent: nothing to delete on second run).

---

## Implementation steps

### Step 1 — Backup

Create `plans/backups/<YYYYMMDD-HHMMSS>-deleted-tags.txt` with:

```
# Bare v* tags (RC/qa variants slated for delete; release tags kept)
<sha> <tagname>
<sha> <tagname>
...
# monolith-system-<lang>-v* tags (all slated for delete)
<sha> <tagname>
...
```

Generated via `git for-each-ref --format='%(objectname) %(refname:short)' refs/tags/v* refs/tags/monolith-system-*`.

Filter out the 20 release tags `v1.0.0`..`v1.0.19` from the backup-and-delete set (those are kept).

### Step 2 — Dry-run

Run a script that prints the planned `git tag -d <tag>` and `git push origin --delete <tag>` operations without executing them. Include rate-limit pre-check. Capture output to a log under `plans/backups/<timestamp>-dryrun.log`.

Sanity-check the count: should be ~1,140 bare-RC-variant tags + 75 monolith-system tags = ~1,215.

### Step 3 — Live deletion

Execute deletes in batches:

- **Local first**: `git tag -d <list>` (fast, no network).
- **Remote in batches of 50**: `git push origin --delete <tag1> <tag2> ... <tag50>` — git push handles many refs in one round-trip. Use `git_push_retry` from `gh-retry.sh` for transient failures.
- **Sleep 1s between batches**.
- **Check rate limit every 10 batches** — abort if < 1,000 requests remaining.

### Step 4 — Verify

- `git tag --list 'v[0-9]*' | wc -l` should equal 20 (just `v1.0.0`..`v1.0.19`).
- `git tag --list 'monolith-system-*' | wc -l` should equal 0.
- `gh release list --limit 50 | grep '^v[0-9]'` should still show the 20 historical Releases intact.
- `git fetch --prune --tags` → confirm no surprise re-creation.

### Step 5 — Add guard to cleanup workflow

Extend `cleanup-prereleases` in `optivem/actions` (or add a new step in `cleanup.yml`) to log-or-delete any tag matching `^v[0-9]` or `^monolith-system-` if encountered. Run the workflow once with `--dry-run` to verify no false positives on the kept `v1.0.0`..`v1.0.19` release tags (the guard must exempt those).

---

## Risks and mitigations

| Risk | Mitigation |
|---|---|
| Accidentally delete one of the 20 kept release tags or detach its Release. | Filter explicitly by suffix pattern (`*-rc.*`, `*-qa-deployed`, `*-qa-approved`) when selecting the bare-`v*` delete set. Do NOT use a broad `^v[0-9]` glob for deletion. Verify in dry-run that the keep-list is intact. |
| Hit the 5,000 req/hr rate limit due to concurrent CI activity. | Pre-flight `gh api /rate_limit` check; abort if < 2,000 remaining. Use batched `git push --delete` (50 refs/call) instead of per-tag API calls where possible. |
| Tag re-appears via local clones still holding old refs and pushing them back. | After cleanup, communicate to anyone with a clone to run `git fetch --prune --tags`. The guard in Step 5 catches this if it happens. |
| Backup file ends up checked in and bloats repo. | Place under `plans/backups/` (add to `.gitignore` if desired) OR commit it deliberately as historical record. Decide before Step 1. |
| Cleanup script has a bug and deletes too much. | Dry-run captures full plan. Backup file enables recreation. Live run aborts on first batch failure. |

---

## Open questions

1. **Run timing.** Run during a quiet period (no concurrent CI), or just go ahead? 1,215 tags is fast (a few minutes). Recommendation: just go ahead, but in dry-run first.

---

## Out of scope

- The VERSION file layout restructure itself — see `20260430-070642-version-file-layout-restructure.md`.
- GHCR container-package cleanup (`shop/monolith-system-*` package names) — those are GHCR image packages, not git tags, and may still be in active use. Audit separately.
- Cleanup of release Releases — the 20 bare `v1.0.x` Releases stay as historical record.
