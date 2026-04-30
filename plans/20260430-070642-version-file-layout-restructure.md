# VERSION File Layout Restructure Plan

Restructure the shop repo's VERSION files so each system has its own version file, decoupling the implicit "root `VERSION` doubles as meta + monolith + multitier system version" pattern that has caused recurring issues and hacky workarounds.

---

## Background

Today the shop repo has a single root `VERSION` that simultaneously plays three roles:

1. The **meta** version (tagged as `meta-v*`).
2. The **monolith** system version for every language (java/dotnet/typescript).
3. The **multitier** system version for every language flavour.

This conflation means a bump for one flavour's release implicitly bumps the version every other flavour's acceptance stage observes — even though those flavours had no change. The historical workarounds for this have been ad hoc.

Multitier components (`backend-java`, `backend-typescript`, `frontend-react`) already have their own per-component VERSION files at `system/multitier/<component>/VERSION`. What is missing is a per-language **system bundle** VERSION for multitier (the synthetic "java backend + react frontend" version) and a per-language system VERSION for monolith.

The git tag namespace already disambiguates these roles:

| Tag pattern | Role | Status |
|---|---|---|
| `meta-v*` | meta version | active |
| `monolith-<lang>-v*` | monolith system tag (per language) | active |
| `multitier-<lang>-v*` | multitier system tag (synthetic bundle) | active |
| `multitier-backend-<lang>-v*`, `multitier-frontend-react-v*` | component tags | active |
| `monolith-system-<lang>-v*` | abandoned alternate scheme | stalled |
| `v1.x.x` | legacy root-VERSION-as-system tags | stalled |

So the tag scheme is already correct. The VERSION file layout just needs to align to it.

---

## Decisions

### D1. Root `VERSION` becomes meta-only

Root `VERSION` no longer plays the system-version role for any flavour. It only feeds the `meta-v*` tag stream.

`meta-v*` is the **shop bundle version** — the umbrella tag meaning "this commit is the shop at version X.Y.Z, bundling all six flavour releases (monolith-{java,dotnet,typescript}, multitier-{java,dotnet,typescript})." Cut by `meta-release-stage` once all six flavour releases for that meta version have succeeded. Bumped on the next dev cycle by `bump-patch-version-meta.yml` — which keeps working unchanged after this restructure.

Bundle structure has two levels:

- **Shop bundle** (`meta-v*`) → bundles 6 flavour releases.
- **Multitier flavour bundle** (`multitier-<lang>-v*`) → bundles backend + frontend component releases for that language.

Monolith flavours (`monolith-<lang>-v*`) are leaf releases — no inner components — so they're not bundles, just flavour releases.

### D2. New per-system VERSION files

Add the following VERSION files:

- `system/monolith/java/VERSION`
- `system/monolith/dotnet/VERSION`
- `system/monolith/typescript/VERSION`
- `system/multitier/java/VERSION`
- `system/multitier/dotnet/VERSION`
- `system/multitier/typescript/VERSION`

Each is the system-bundle version for its flavour. Acceptance, RC tagging, QA signoff, and release for that flavour all read from this file.

### D3. Multitier component VERSIONs unchanged

Component VERSIONs exist only for multitier (monolith has no separate components — monolith *is* one component). The existing multitier component files — `system/multitier/backend-java/VERSION`, `backend-typescript/VERSION`, `backend-dotnet/VERSION`, `frontend-react/VERSION` — keep tracking their own image publishes and remain tagged via `multitier-<component>-v*`. No structural change.

### D4. Bump policy: post-flavour-release, with cascade

When a flavour's prod-release stage completes, it triggers a `bump-patch-version-<arch>-<lang>` workflow:

- **Monolith**: bumps `system/monolith/<lang>/VERSION`. No cascade (monolith has no separate components).
- **Multitier**: bumps `system/multitier/<lang>/VERSION` AND cascade-bumps the underlying component VERSIONs:
  - `multitier-java` → bumps `multitier/backend-java/VERSION` + `multitier/frontend-react/VERSION`
  - `multitier-dotnet` → bumps `multitier/backend-dotnet/VERSION` + `multitier/frontend-react/VERSION` (TBD — confirm during audit)
  - `multitier-typescript` → bumps `multitier/backend-typescript/VERSION` + `multitier/frontend-react/VERSION` (TBD — confirm during audit)

Root `VERSION` (meta) is bumped only on meta-release events, not on flavour releases.

### D5. Tag scheme unchanged

Keep using the existing `<arch>-<lang>-v*` system-tag pattern. No literal `system-` prefix is added — the existing convention already disambiguates.

Bare `v1.x.x` tag creation was retired in the prefix-namespace refactor on 2026-04-18 (commits `0aa585bd`, `18fdd83b`). No workflow currently produces them. Cleanup of the historical bare `v*` and abandoned `monolith-system-<lang>-v*` tag families is tracked separately in `20260430-071646-legacy-tag-cleanup.md`.

### D6. gh-optivem scaffolding aligns to new layout

When gh-optivem materializes a single-system scaffolded repo from the shop:

- Source: `shop/system/<arch>/<lang>/VERSION`
- Destination: `scaffolded-repo/VERSION` (root, since scaffolded repos host one system)

Scaffolded repos keep root `VERSION` as their system version. The asymmetry between shop (multi-system, root = meta) and scaffolded repos (single-system, root = system) is intentional and reflects each repo's actual shape.

### D7. gh-optivem internal versioning unchanged

gh-optivem's own root `VERSION` and component versions are about gh-optivem-the-tool's release lifecycle and are orthogonal to this restructure.

### D8. Initial values seeded from latest published tag

Each new VERSION file is initialized to the version number of the latest published tag for its flavour (the latest non-RC `<arch>-<lang>-v*` tag). For example, if `monolith-java-v1.0.62` is the latest, then `system/monolith/java/VERSION = 1.0.62`. The next post-release bump produces `<arch>-<lang>-v1.0.63` — natural continuation, no collision with existing image or tag artifacts.

Resolved during Step 1 audit; values written in Step 2.

---

## Implementation steps

### Step 7 — Verify — ⏳ Deferred

Per-user direction (Q5): user will run verification manually when ready.

- Run `./compile-all.sh` to confirm nothing broke at build time.
- Trigger one acceptance stage per flavour (monolith-java, monolith-dotnet, monolith-typescript, multitier-java, multitier-dotnet, multitier-typescript) and confirm each reads its own VERSION.
- Cut one RC per flavour and confirm correct tag prefix is produced.
- Trigger one full prod-release-stage end-to-end and confirm the post-release bump targets the right VERSION file(s).

---

## Risks and mitigations

| Risk | Mitigation |
|---|---|
| A workflow currently reads root `VERSION` but is missed during the audit, silently still working but reading the wrong (now-frozen meta) value. | The audit grep must be exhaustive — search for `VERSION` (case-sensitive) across `.github/workflows/`, `scripts/`, `docker/`, `system-test/`, and root-level shell scripts. Cross-check by triggering at least one workflow per stage type after Step 3. |
| Initial VERSION values are set wrong, causing image tags to collide with existing published images. | Answer Open Question 1 before Step 2. Default-recommended approach: seed each new VERSION file from the latest published tag for that flavour (`monolith-java-v1.0.62` → `system/monolith/java/VERSION = 1.0.62`). |
| gh-optivem scaffolding regresses for already-scaffolded downstream repos. | gh-optivem only affects new scaffolds. Already-scaffolded repos are unaffected (they have their own root `VERSION` independent of shop). |
| Cascade bump for multitier creates a divergent component-version trail (component bumped by both its own publish AND by system bundle release). | Decide cascade semantics explicitly. Recommended: cascade bumps the patch on the component VERSION even if the component itself didn't change in that release, accepting that some component versions will be published-but-unchanged. Alternative: cascade only bumps components that actually changed in the release — requires diff detection. |

---

## Open questions (deferred)

See bottom of file — answers needed before Step 2.

---

## Out of scope

- Changing the tag-as-state-machine pattern (covered separately in `ACTIONS_PLAN.md` H0).
- Restructuring the meta-release pipeline.
- Cleaning up historical tags (`monolith-system-<lang>-v*`, bare `v1.x.x` RC/qa variants) — covered separately in `20260430-071646-legacy-tag-cleanup.md`.
- gh-optivem internal versioning changes.
