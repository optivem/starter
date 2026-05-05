# Move `external-real-sim` and `external-stub` out of `system/` into top-level `external-systems/`

🤖 **Picked up by agent** — `Valentina_Desk` at `2026-05-05T09:30:51Z`

**Status:** In progress — open questions resolved, executing Step 1. **Cross-repo change** — touches both `shop` and `gh-optivem`. Validated locally before either PR merges.
**Created:** 2026-05-05.
**Last revised:** 2026-05-05 (after sibling-repo audit; local-validation sequence adopted; open questions resolved).

---

## Goal

Reorganise the shop repo so `system/` contains only the systems we own and ship (the SUTs), and the simulated/stubbed external dependencies live under a sibling `external-systems/` directory. This makes the "what we own / what we fake" boundary explicit.

### Current layout (shop)

```
shop/
  system/
    external-real-sim/      # node + json-server simulator (ERP, Tax, Clock)
    external-stub/          # WireMock mappings + __files
    monolith/               # SUT
    multitier/              # SUT
```

### Target layout (shop)

```
shop/
  external-systems/
    external-real-sim/
    external-stub/
  system/
    monolith/               # unchanged
    multitier/              # unchanged
```

> The scaffolded-repo layout produced by `gh-optivem` is unchanged: externals are flattened to `<repo>/external-real-sim/` and `<repo>/external-stub/` at the root. Only the *source* path inside `shop` changes.

---

## Cross-repo audit

Searched `optivem/actions`, `gh-optivem`, `hub`, and `github-utils` for references to `system/external-` or the directory names.

| Repo | Has references? | Impact |
|---|---|---|
| `optivem/actions` | None | Clean — no changes needed. |
| `hub` | One false positive (`"09-architecture-external-stubs"` is a course-module label, not a path) | None. |
| `github-utils` | None | Clean. |
| **`gh-optivem`** | **Yes — including code that will silently break** | **Coordinated change required.** See next section. |

### `gh-optivem` coupling (critical)

`gh-optivem` is the scaffolder CLI. It reads from `shop/` and emits new repos. It hardcodes both the source path `<shop>/system/external-*` and the docker-compose path-rewrite pattern. After the shop rename, three things break — two of them **silently**.

| Item | File / Line | Failure mode if not updated | Fix |
|---|---|---|---|
| `copyExternals` source path | `internal/steps/apply_template.go:68` — `filepath.Join(shop, "system", dir)` | **Silent.** `os.Stat` returns "not found", function skips the copy. Scaffolded repos ship without `external-real-sim/` and `external-stub/` directories. | Change `"system"` → `"external-systems"` (or thread a constant). |
| Compose path-rewrite (monolith) | `apply_template.go:627-628` — `{shopSystemPrefix + dirExternalRealSim, "../" + dirExternalRealSim}` (where `shopSystemPrefix = "../../../system/"`) | **Silent.** Source pattern no longer matches; compose files inside scaffolded repos retain shop-relative paths like `../../../external-systems/external-real-sim` that don't exist in the scaffolded layout. Stack startup fails with "no such file or directory". | Add `shopExternalSystemsPrefix = "../../../external-systems/"` and use it for these two replacements. |
| Compose path-rewrite (multitier) | `apply_template.go:720-721` | Same as above. | Same. |
| Dead-but-misleading constants | `internal/steps/names.go:126-127` — `ShopExternalRealSimDir: "system/external-real-sim"`, `ShopExternalStubDir: "system/external-stub"` | None at runtime (declared, never read elsewhere — verified via grep). Documents a stale assumption. | Update to `"external-systems/external-real-sim"` and `"external-systems/external-stub"` for accuracy. |
| `MAPPING.md` | Lines 19, 20, 66, 67, 86, 87, 115, 116, 136, 137 — repeated `# from: system/external-*` annotations | None at runtime (documentation). Misleads readers. | Update the prose to reference `external-systems/`. |
| Embedded ATDD prompts (4 files) | `internal/atdd/runtime/agents/prompts/{atdd-driver, atdd-dsl, atdd-stubs, atdd-test}.md` — prose like ``see `system/external-real-sim` `` | None at runtime (informational text in agent prompts). The prompts are baked into the gh-optivem binary via `//go:embed prompts/*.md` and are aggregations of shop docs; they don't auto-sync. | Update prose to `external-systems/external-real-sim`. |

> **Why the silent failures matter.** The scaffolder isn't part of shop's CI; it's exercised by hand or in `optivem/academy/rehearsal-*` worktrees. A silent skip in `copyExternals` would not show up in shop's compile-all or system-test sweeps. A new scaffolded repo would simply lack the two folders, and the failure would surface only when someone tried to bring up its docker stack.

---

## Scope decisions (what changes, what stays)

### Changes — shop repo

- Two directory moves via `git mv`.
- 24 docker-compose YAML files under `docker/<lang>/<topology>/` — substitute `system/external-` → `external-systems/external-` in `context:` and volume-bind paths. Compose-file depth (`../../../` to repo root) is unchanged.
- 3 ATDD process docs under `docs/atdd/process/` — same string substitution; one of them (`ct-green-stubs.md`) has a relative markdown link whose href must also be retargeted.

### Changes — gh-optivem repo

- `internal/steps/apply_template.go` — `copyExternals` source path (1 string), and add a new `shopExternalSystemsPrefix` constant used in two replacement-table entries (4 occurrences across `monolithContentReplacements` and `multitierContentReplacements`).
- `internal/steps/names.go` — 2 constant values (`ShopExternalRealSimDir`, `ShopExternalStubDir`).
- `MAPPING.md` — 10 prose references.
- 4 embedded prompt files — prose references.
- A new `gh-optivem` release after the PR merges (the prompts are `//go:embed`-ed, so the binary must be rebuilt and reinstalled for any consumer that scaffolds new repos against the renamed shop).

### Out of scope — explicitly NOT renamed

These are independent identifiers that share the name; the folder rename does not require touching them.

| Identifier | Where | Why it stays |
|---|---|---|
| `external-stub` (docker-compose service name) | All `docker/**/docker-compose.*.{stub,real}.yml` | Service name is a docker-network identifier, not a filesystem path. |
| `http://external-stub:8080/...` (env vars) | All compose files and `EXTERNAL_BASE_URL` defaults | Same — docker-network hostname. |
| `external-stub` / `external-real` (`containerName` field) | All 6 `docker/<lang>/<topology>/system.json` | Used by `gh optivem` to target containers; tied to the docker service name above, not the folder. |
| `external-real-sim:latest`, `external-stub:latest` (GHCR image tags) | 18 `*-stage-cloud.yml` workflows | Image name is an OCI registry identifier, independent of source path. |
| `external-stub-acceptance`, `external-real-acceptance` (Cloud Run service names) | Cloud workflows | Cloud Run identifier. |
| `deploy-external-stub`, `deploy-external-real` (workflow job names) | Cloud workflows | Job name; renaming would force every dependent `needs:` and `outputs:` reference to change for no clarity gain. |
| `dirExternalRealSim`, `dirExternalStub` (basename consts) | `gh-optivem/internal/steps/apply_template.go:34-35` | Hold *basenames* (`"external-real-sim"`, `"external-stub"`), which are unchanged by the parent-folder rename. |
| Scaffolded-repo layout | `gh-optivem` `copyExternals` destination | `copyExternals` flattens externals to `repoDir/external-real-sim` regardless of the shop source layout — scaffolded repos never had a `system/` parent for these in the first place. |

Keeping these unchanged means **zero workflow file edits in shop**, and no API change for `gh-optivem`'s consumers.

---

## Files to edit

### Shop — directory moves (2)

```
git mv system/external-real-sim external-systems/external-real-sim
git mv system/external-stub     external-systems/external-stub
```

### Shop — Docker compose files (24)

For each language `L ∈ {dotnet, java, typescript}` and each topology `T ∈ {monolith, multitier}`:

| File | Line(s) | Change |
|---|---|---|
| `docker/L/T/docker-compose.local.real.yml` | `context:` | `../../../system/external-real-sim` → `../../../external-systems/external-real-sim` |
| `docker/L/T/docker-compose.local.stub.yml` | volume bind | `../../../system/external-stub:/home/wiremock` → `../../../external-systems/external-stub:/home/wiremock` |
| `docker/L/T/docker-compose.pipeline.real.yml` | `context:` | same as local.real |
| `docker/L/T/docker-compose.pipeline.stub.yml` | volume bind | same as local.stub |

= 6 dirs × 4 files = **24 files**, one substitution each.

### Shop — Documentation (3)

| File | Line | Change |
|---|---|---|
| `docs/atdd/process/cycles.md` | 145 | `system/external-real-sim` → `external-systems/external-real-sim` (prose) |
| `docs/atdd/process/ct-green-stubs.md` | 14 | Markdown link ``[`system/external-real-sim`](../../../system/external-real-sim)`` — update both label text and target href |
| `docs/atdd/process/ct-cycle-conventions.md` | 5 | `system/external-real-sim` → `external-systems/external-real-sim` (prose) |

### gh-optivem — code (2 files)

`internal/steps/apply_template.go`:
- Line 36: add a sibling constant — `shopExternalSystemsPrefix = "../../../external-systems/"`. Keep `shopSystemPrefix` for the SUT replacements at lines 625, 631, 728 — those still target `system/`.
- Line 68 (`copyExternals`): change `filepath.Join(shop, "system", dir)` → `filepath.Join(shop, "external-systems", dir)`.
- Lines 627-628 (monolith replacements): change source patterns from `shopSystemPrefix + dirExternalRealSim` / `shopSystemPrefix + dirExternalStub` to `shopExternalSystemsPrefix + dirExternalRealSim` / `shopExternalSystemsPrefix + dirExternalStub`. Destinations (`"../" + dir...`) are unchanged.
- Lines 720-721 (multitier replacements): same change as 627-628.

`internal/steps/names.go`:
- Line 126: `"system/external-real-sim"` → `"external-systems/external-real-sim"`.
- Line 127: `"system/external-stub"` → `"external-systems/external-stub"`.

### gh-optivem — docs and embedded prompts (5 files)

- `MAPPING.md` — find/replace `system/external-real-sim` → `external-systems/external-real-sim` and `system/external-stub` → `external-systems/external-stub` across the document. Also re-position those two entries within the diagrammed shop tree (currently shown as siblings of `monolith/`, `multitier/` under `system/`; should now appear at the repo root).
- `internal/atdd/runtime/agents/prompts/atdd-driver.md` — prose mention (`system/external-real-sim`).
- `internal/atdd/runtime/agents/prompts/atdd-dsl.md` — prose mention.
- `internal/atdd/runtime/agents/prompts/atdd-stubs.md` — prose mention + relative markdown link href.
- `internal/atdd/runtime/agents/prompts/atdd-test.md` — prose mention.

> Note: these four prompt files appear to be aggregations / re-embeds of shop docs. Whether they're hand-edited or generated isn't clear from the audit; if there's a generator, run it after updating shop. Either way, the embedded copy must end up consistent with shop, since `//go:embed prompts/*.md` bakes whatever's on disk at build time.

---

## Risk / blast radius

| Dimension | Assessment |
|---|---|
| Shop text edits | 27 mechanical substitutions + 1 directory move. No source code or build-script imports affected. |
| Shop CI impact | Cloud-stage workflows pull pre-built `external-real-sim` / `external-stub` images from GHCR; folder rename does not affect them. Docker-stack workflows rebuild from compose, which pick up the new paths. Sample tests will validate this locally before merge. |
| gh-optivem failure modes (silent-skip in `copyExternals`, no-op in compose path-rewrite) | **Mitigated by local validation before either PR merges** — see Step 1 below. The new gh-optivem binary is built locally, run against the locally-renamed shop working tree, and used to scaffold a throwaway repo. Both silent-failure paths are exercised end-to-end before any commit. |
| Merge ordering | **Doesn't matter.** After local validation passes, both PRs can be merged independently in any order; neither breaks anything that wasn't already broken. The merge-window pressure that motivated lockstep-merge / transitional-fallback is gone. |
| Pre-commit hook | Academy convention is `git add -A`. The shop rename produces a large diff (two trees moved + 27 file edits); confirm the hook doesn't false-positive on the `Sync changes` shape. Use a clear commit message — see Step 1 below. |

### Things that could go wrong

1. **Missed compose file in shop** — caught immediately on first `gh optivem run system` (loud failure) and by the local scaffold smoke-test in Step 1.
2. **Stale doc link** in `ct-green-stubs.md` (relative href to non-existent path) — easy to verify with markdown preview after edit.
3. **Stale embedded prompt** in `gh-optivem` — would only mislead readers, not break runtime.
4. **Skipping local validation and merging shop first** — old gh-optivem binary keeps reading `<shop>/system/external-*` and silently skipping, so anyone who scaffolds between the two merges gets a broken repo. Mitigation: don't skip local validation. The whole point of Step 1 is to remove this risk before either PR exists.

---

## Suggested execution order

The headline idea: **make all changes in working trees first, validate end-to-end with a locally-built gh-optivem binary against the locally-renamed shop, only then commit and push.** Both silent-failure modes get exercised before any merge happens, which removes merge-ordering pressure entirely.

### Step 0 — pre-flight

1. Confirm with user: rename target name (`external-systems/` vs alternative).
2. Re-run cross-repo grep right before starting (in case other repos in `optivem/academy/*` have grown new references): `Grep "system/external-(real-sim|stub)"` across every sibling repo.
3. Branch in both repos. No commits yet.

### Step 1 — local edits + local end-to-end validation (no commits)

1. **Edit shop's working tree:**
   - `git mv system/external-real-sim external-systems/external-real-sim`
   - `git mv system/external-stub external-systems/external-stub`
   - Apply the 27 text substitutions (24 compose files + 3 docs, including the relative-href update in `ct-green-stubs.md`).
   - `Grep "system/external-"` excluding `node_modules`, `.git`, `build`, `bin`, `obj` — must return zero hits.
   - `./compile-all.sh` — must pass.

2. **Edit gh-optivem's working tree:**
   - `apply_template.go`: add `shopExternalSystemsPrefix`, update `copyExternals` source path, swap the source patterns in the four replacement-table entries.
   - `names.go`: update the two `Shop*Dir` constant values.
   - `MAPPING.md` + 4 embedded prompt files: prose updates.
   - `go build ./...` — must succeed.
   - Run the gh-optivem test suite.

3. **Local end-to-end smoke test** — this is the step that validates the silent-failure modes:
   - Build the gh-optivem binary locally (the existing `scripts/atdd-rehearsal.sh` flow already does this — `go build` produces `gh-optivem.exe` from the working copy).
   - Run that binary to scaffold a throwaway repo into a temp dir, pointing at the local `shop` working tree as the source.
   - Verify the scaffolded repo has `external-real-sim/` and `external-stub/` at the root (catches the `copyExternals` silent-skip).
   - In the scaffolded repo, run `docker compose -f docker/<lang>/<topology>/docker-compose.local.stub.yml up` for one combo and confirm the stack starts (catches the compose path-rewrite silent no-op). Tear down.
   - Per `feedback_ask_before_local_system_tests`: **ask user before running** any docker stack or `gh optivem run/test/stop system` commands locally.

### Step 2 — commit and push (after Step 1 passes)

1. **Shop PR:** commit with a clear, non-`Sync changes`-shape message — e.g. `refactor: split external systems out of system/ into external-systems/`. Push.
2. **gh-optivem PR:** commit with the matching message. Push.
3. Merge order doesn't matter — both code paths are validated and both repos are internally consistent.

### Step 3 — gh-optivem release

After the gh-optivem PR merges, tag a new release so consumers pick up the new binary.

### Step 4 — watch shop's acceptance-stage CI

The cloud-stage workflows pull pre-built images from GHCR and don't reference source paths; folder rename does not affect them. Watch the next acceptance-stage run for one language to confirm.

---

## Open questions

All resolved (2026-05-05):

1. **Naming** → `external-systems/` (confirmed by user).
2. **GHCR image namespacing** → skip (image names stay as-is; out of scope).
3. **`external-systems/README.md`** → skip (no stub README).
4. **`gh-optivem` release strategy** → resolved by Step 1 local-validation flow.
