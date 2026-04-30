# Switch bump-patch-version signal from ghcr-image to git-tag

## Decision

Use `git-tag` as the release-decision signal for every `bump-patch-versions` invocation. The git tag is the authoritative "this version was released" record; GHCR images remain a downstream artifact of the same release event (docker tagging is **not** deleted).

For multitier — where each release publishes two independently-versioned components — per-component git tags are added at prod-stage to mirror the existing per-component docker tagging. Flavor-level tags are unchanged. Monolith needs no new git tags (`flavor == component`). See Scope for tag prefixes.

Rationale:

- **The git tag is the authoritative release ledger.** Every `*-prod-stage.yml` already calls `optivem/actions/publish-tag@v1` and `softprops/action-gh-release@v3` against a flavor tag. Extending this to per-component tags makes git the upstream truth for "was component X v1.2.3 released?" — the question `bump-patch-versions` actually asks. GHCR remains a downstream artifact.
- **Smaller auth surface.** `git ls-remote` runs on `contents: read`, which the workflow already has. No `packages: read`, no OCI two-step bearer exchange against `ghcr.io/token`.
- **Registry-independent.** A git-tag signal survives a registry move; a GHCR-image signal hard-codes the registry into bump logic.
- **Removes asymmetry.** `bump-patch-version-meta.yml` already uses `git-tag`. Per-flavor and per-component bumps using `ghcr-image` is inconsistency without justification.

### Three immutability layers (rationale)

The "git tag vs docker tag" framing is misleading because there are three layers, each authoritative for a different question:

| Layer | Mutable? | Authoritative for |
|---|---|---|
| Git commit SHA | immutable by hash | "what code is this" |
| Git tag (`v1.2.3`) | technically movable, socially "don't" | "this version was released" |
| Image digest (`sha256:…`) | immutable by hash | "this exact byte-for-byte artifact exists" |
| Image tag (`:v1.2.3`) | freely movable | nothing on its own — a lookup label |

For the bump-patch use case, the question is unambiguously "was v1.0.61 released?" — a release-decision question. Git tag wins.

## Scope

### Multitier prod-stage workflows — new per-component git-tag step + step reorder

Each multitier prod-stage already has a per-component **docker** tagging step at the end of the `run` job: `read-base-versions` → `compose-tags` → `tag-docker-images` (see `multitier-typescript-prod-stage.yml:206-229`). It tags `multitier-backend-typescript:v<backend-version>` and `multitier-frontend-react:v<frontend-version>` using each component's own `VERSION` file.

The plan does two things here:

1. **Add the symmetric per-component git-tag step**, publishing `multitier-backend-{lang}-v<backend-version>` and `multitier-frontend-react-v<frontend-version>` using the same component versions read by `read-base-versions`.
2. **Reorder steps so user-visible side effects are last**, per rubric §7.1 (cheapest-to-reverse first; user-visible last). Today's order is `tag-docker-images (flavor)` → `publish-tag (flavor)` → `generate-release-notes` → `softprops/action-gh-release` → `read-base-versions` → `compose-tags` → `tag-docker-images (per-component)`. The user-visible GitHub Release happens *before* the per-component docker tagging — if the per-component step fails, the Release is already public but `multitier-backend-typescript:v1.0.60` is missing.

| Flavor | New git tags |
|---|---|
| `multitier-dotnet-prod-stage.yml` | `multitier-backend-dotnet-v<backend-dotnet VERSION>`, `multitier-frontend-react-v<frontend-react VERSION>` |
| `multitier-java-prod-stage.yml` | `multitier-backend-java-v<backend-java VERSION>`, `multitier-frontend-react-v<frontend-react VERSION>` |
| `multitier-typescript-prod-stage.yml` | `multitier-backend-typescript-v<backend-typescript VERSION>`, `multitier-frontend-react-v<frontend-react VERSION>` |

**New ordering** (cheapest-to-reverse → user-visible last):

1. `tag-docker-images` — flavor-level docker tag (broadcast)
2. `publish-tag` — flavor-level git tag (`multitier-{lang}-v<flavor>`)
3. `read-base-versions` — read each component's `VERSION`
4. `compose-tags` — build per-component docker tag map
5. `tag-docker-images` — per-component docker tags (`multitier-backend-{lang}:v<backend>`, `multitier-frontend-react:v<frontend>`)
6. **NEW** `publish-tag` (loop) — per-component git tags (`multitier-backend-{lang}-v<backend>`, `multitier-frontend-react-v<frontend>`)
7. `generate-release-notes` — compose notes (no external side effect)
8. `softprops/action-gh-release@v3` — **last**, user-visible Release record

If step N (any of 1–6) fails, no user-visible Release exists yet — retries are clean. If 7 or 8 fails, all artifacts and tags are already in place and the Release can be re-attempted directly.

Implementation note for step 6: simplest path is a loop calling `optivem/actions/publish-tag@v1` over the component-version list (reusing `read-base-versions` output). If the same pattern shows up in another caller, promote to a `publish-tags` (plural) action; for now one loop in three workflows is fine.

**Frontend tag idempotency.** A component git tag is owned by the *component's version*, not by any flavor's release event. Flavors that ship the same frontend version converge on the same tag; re-publishing is a no-op (the first publish wins the SHA pointer). Verify `publish-tag@v1` exits cleanly when the target tag already exists; if it doesn't, fix the action there (single place) rather than wrapping every call site.

Monolith prod-stage workflows: same reorder applies (move `softprops/action-gh-release` to last; per-component docker re-tag happens before it). No new git tags — for monolith, flavor *is* component, so `monolith-{lang}-v<version>` already covers both roles.

**Stages other than prod are unaffected.** Commit stage emits per-component `-dev` docker tags only (no git tag — git tags are reserved for release decisions, not build trace); acceptance stage emits flavor-level RC docker + git tags only (RC is a flavor-level QA gate; component identity at RC time isn't a meaningful gate); prod stage is where per-component *release* tagging happens. The rule the rubric proposal in step 6 should encode: **git tags mirror docker *release* tags, not docker build labels**.

### bump-patch-version files — switch signal to `git-tag`

| File | Entries | Tag prefix |
|---|---|---|
| `bump-patch-version-monolith-dotnet.yml` | `VERSION` | `monolith-dotnet-v` |
| `bump-patch-version-monolith-java.yml` | `VERSION` | `monolith-java-v` |
| `bump-patch-version-monolith-typescript.yml` | `VERSION` | `monolith-typescript-v` |
| `bump-patch-version.yml` (multitier root omnibus, 4 entries) | `system/multitier/backend-dotnet/VERSION` | `multitier-backend-dotnet-v` |
| | `system/multitier/backend-java/VERSION` | `multitier-backend-java-v` |
| | `system/multitier/backend-typescript/VERSION` | `multitier-backend-typescript-v` |
| | `system/multitier/frontend-react/VERSION` | `multitier-frontend-react-v` |
| `bump-patch-version-meta.yml` | (already `git-tag`) | no change |

For each file, change `"signal": "ghcr-image"` → `"signal": "git-tag"` and rewrite `"value"` per the table.

### Scaffold-only (never executed in shop)

These templates are copied by gh-optivem into student multirepo repos at `gh optivem init` time, and the `value` is rewritten by `systemPrefixDropReplacements` to `v` (matching what the student's own prod-stage publishes). The shop value is a placeholder; the prefix should follow the same per-component construction principle for consistency.

- `bump-patch-version-multitier-{dotnet,java,typescript}.yml` (3 files, 2 entries each) — placeholder prefixes `multitier-backend-{lang}-v` and `multitier-frontend-react-v`
- `bump-patch-version-multitier-backend-{dotnet,java,typescript}.yml` (3 files) — placeholder prefix `multitier-backend-{lang}-v`
- `bump-patch-version-multitier-frontend-react.yml` (1 file) — placeholder prefix `multitier-frontend-react-v`

### gh-optivem template-fixup logic

`gh-optivem/internal/steps/apply_template.go` — `systemPrefixDropReplacements` (line 521-530) already collapses 2-segment prefixes (`monolith-typescript-v` → `v`, `multitier-java-v` → `v`). Add 3-segment cases for the per-component scaffold templates: `multitier-backend-{lang}-v` → `v`, `multitier-frontend-react-v` → `v`. Re-build and re-publish the gh-optivem CLI tag.

## Implementation order

2. **Update gh-optivem fixup logic** — `systemPrefixDropReplacements` to also collapse 3-segment prefixes (`multitier-backend-{lang}-v` → `v`, `multitier-frontend-react-v` → `v`). Re-publish the CLI tag.
3. **Cut a fresh shop tag** so `gh-acceptance-stage` can pick it up.
4. **Re-run `gh-acceptance-stage` end-to-end.** Confirm prod-stage publishes all expected tags (flavor + 2 components) and bump-patch-version succeeds across all four matrix entries.
5. **Propose a rubric update in `actions/`.** After at least one clean release cycle, draft an addition to `actions/.claude/agents/docs/devops-rubric.md` codifying three rules: (i) "For release-decision probes (e.g. `bump-patch-versions` signal), prefer `git-tag` over `ghcr-image` — git is the authoritative release ledger, GHCR is a downstream artifact"; (ii) "Per-component artifact tagging — apply the `<flavor>-<role>-<lang>-v<version>` construction principle to git tags as well as docker tags, so each independently-versioned component has a first-class git ref"; (iii) "Git tags mirror docker *release* tags, not docker build labels — RC and release stages get symmetric docker+git treatment; commit-stage `-dev` build labels stay docker-only." Likely home is §1 (build-once-promote-many) or as a new sub-rule under §4 (lookup-and-trigger). Tracked as a separate plan in `actions/plans/` (created at this step, not pre-created).

## Verification

- `gh-acceptance-stage` smoke matrix passes (4/4 green) with a fresh shop tag.
- After a multitier release, `gh tag list` shows three new git refs: `multitier-{lang}-v<flavor>`, `multitier-backend-{lang}-v<backend>`, `multitier-frontend-react-v<frontend>`. Docker tags also still present (build-once-promote-many unaffected).
- After a release, the GitHub Release record (Releases UI) appears **after** all docker and git tags exist — confirm by checking workflow run logs that `softprops/action-gh-release` is the last step in the `run` job.
- Manually-dispatched `meta-bump-all` in shop main runs without 403s.
- A no-op re-dispatch of `bump-patch-version` in any test-app exits idempotently (signal exists, no commit).

## Out of scope

- The `optivem/actions/bump-patch-versions@v1` action itself needs no changes. Both signal modes are already supported.
- Cloud-variant prod-stage workflows (`*-prod-stage-cloud.yml`) don't call `bump-patch-version.yml`.
