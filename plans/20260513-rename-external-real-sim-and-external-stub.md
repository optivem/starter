# Rename `external-real-sim` → `simulators`, `external-stub` → `stubs`

**Status:** Plan draft. Not started.
**Created:** 2026-05-13.
**Cross-repo change** — touches both `optivem/shop` and `optivem/gh-optivem`. Same coordination pattern as the 2026-05-05 `external-systems/` reorg plan.

---

## Goal

Rename the two external-system directories under `external-systems/` for clarity:

- `external-systems/external-real-sim/` → `external-systems/simulators/`
- `external-systems/external-stub/` → `external-systems/stubs/`

And rename the corresponding docker-compose services/containers to match:

- service `external-real` → `external-system-simulators`
- service `external-stub` → `external-system-stubs`

The `external-systems/` parent directory stays. The `label: real|stub` discriminator in `systems.yaml`, the compose filenames `docker-compose.*.real.yml` / `*.stub.yml`, and the `EXTERNAL_SYSTEM_MODE=real|stub` env value all stay — those describe a *mode* (running against real-fidelity sims vs deterministic stubs), not a directory.

### Why

- `external-real-sim` is awkward ("real? sim? both?"). `simulators` is the plain word.
- `external-stub` becomes `stubs` to align with the `gh-optivem.yaml` config key `external_systems.stubs.path` that already exists.
- Service names get the `external-system-` prefix so a bare `stubs:` service doesn't read as "stubs of what?" — they remain self-describing inside compose files.
- The Go struct fields `ExternalSystems.Stubs` / `.Simulators` are *already* named correctly. This rename brings disk paths and docker-compose service names in line with the existing config concept.

### Target layout (shop)

```
shop/
  external-systems/
    simulators/                      # was external-real-sim
      Dockerfile
      mock-server.js
      package.json
      package-lock.json
    stubs/                           # was external-stub
      mappings/
  system/
    monolith/                        # unchanged
    multitier/                       # unchanged
```

---

## Cross-repo audit

Searched `optivem/actions`, `gh-optivem`, `hub`, `github-utils`, and `eshop` for references to the old names.

| Repo | Has references? | Impact |
|---|---|---|
| `optivem/actions` | None | Clean. |
| `hub` | None relevant (one false-positive course-module label `09-architecture-external-stubs`) | None. |
| `github-utils` | None | Clean. |
| `eshop` / `eshop-tests` | None | Clean. |
| **`shop`** | **Yes — ~64 files** | See Step 2. |
| **`gh-optivem`** | **Yes — ~18 files, including silent-failure paths** | See Step 1. |

### Silent-failure risk (same shape as the 2026-05-05 plan)

`gh-optivem`'s `copyExternals` uses `os.Stat` + skip-if-missing — if the source path no longer matches, the scaffolded repo silently ships without the externals directories. The replacement-table-driven path rewrites in `apply_template.go` are also silent: a stale `../../../external-systems/external-real-sim` in compose files won't error at scaffold time, only at `docker compose up`.

Mitigation: ship gh-optivem and shop in the same coordinated merge (option A below), so the version of gh-optivem that ships with the new constants is always paired with a shop that has the new directory names. **Don't** introduce a "tries new then old" fallback in `copyExternals` — that's the kind of compat shim CLAUDE.md tells us to avoid.

---

## Step 1 — `gh-optivem` changes

### Code

| File | Change |
|---|---|
| `internal/steps/apply_template.go` | `dirExternalRealSim` → `dirSimulators` (value `"simulators"`). `dirExternalStub` → `dirStubs` (value `"stubs"`). `externalSimDirs` → `externalDirs`. Doc comment on `copyExternals` updated. |
| `internal/steps/names.go` | Field `ShopExternalRealSimDir` → `ShopSimulatorsDir` (value `"external-systems/simulators"`). Field `ShopExternalStubDir` → `ShopStubsDir` (value `"external-systems/stubs"`). (These fields are currently declared but **not read** anywhere else per the 2026-05-05 audit; the rename is for accuracy.) |
| `internal/configinit/prompt.go` | `defaultStubsPath = "external-systems/stubs"`. `defaultSimulatorsPath = "external-systems/simulators"`. |
| `internal/config/config.go` (lines 597, 600) | Default `StubsPath` / `SimulatorsPath` values updated to new paths. |
| `main.go` (lines 128–129) | Usage examples updated. |
| `scripts/manual-test.sh` (lines 125–126) | Default flag values updated. |

### Tests

| File | Change |
|---|---|
| `config_commands_test.go` | All literal `"external-systems/external-stub"` / `"external-systems/external-real-sim"` strings updated (~12 occurrences). |
| `internal/config/yaml_input_test.go` | `Path: "external-stub"` → `"stubs"`, `Path: "external-real-sim"` → `"simulators"` (4 occurrences). |
| `internal/projectconfig/config_test.go` | YAML fixtures + struct literals updated (~13 occurrences). |
| `internal/atdd/runtime/preflight/preflight_test.go` | `makeDir` paths and `ExternalSpec.Path` values updated (~8 occurrences). |
| `internal/steps/optivem_yaml_test.go` | Fixture-builder return values and assertions updated (~6 occurrences). |

### Docs

| File | Change |
|---|---|
| `README.md` | Two `--stubs-path … --simulators-path …` usage blocks updated. |
| `CONTRIBUTING.md` | One usage block updated. |
| `MAPPING.md` | 10 prose references to `external-real-sim/` / `external-stub/` updated. |
| `internal/atdd/runtime/agents/prompts/atdd-driver.md` | One reference (prose `external-systems/external-real-sim`) updated. |
| `internal/atdd/runtime/agents/prompts/atdd-dsl.md` | Same. |
| `internal/atdd/runtime/agents/prompts/atdd-test.md` | Same. |
| `internal/atdd/runtime/agents/prompts/atdd-stubs.md` | One reference + markdown link target (`../../../external-systems/external-real-sim`) updated. |

> ATDD prompts are baked into the binary via `//go:embed prompts/*.md` (per the 2026-05-05 plan's note). Rebuild after edits.

### Validation

- `scripts/test.sh` (per memory: never `go test ./...` without `-p 2` on this machine).
- `scripts/manual-test.sh` sweep.

---

## Step 2 — `shop` changes

### Directory moves

```
git mv external-systems/external-real-sim external-systems/simulators
git mv external-systems/external-stub     external-systems/stubs
```

### Docker compose (24 files)

Under `docker/<testLang>/<arch>/` for `testLang ∈ {dotnet, java, typescript}` and `arch ∈ {monolith, multitier}` — 4 files per combo (`docker-compose.{local,pipeline}.{real,stub}.yml`).

Three independent substitution rules per file:

| Rule | From | To |
|---|---|---|
| Volume bind / build context (real) | `../../../external-systems/external-real-sim` | `../../../external-systems/simulators` |
| Volume bind / build context (stub) | `../../../external-systems/external-stub` | `../../../external-systems/stubs` |
| Service key + `depends_on:` key (real) | `external-real` | `external-system-simulators` |
| Service key + `depends_on:` key (stub) | `external-stub` | `external-system-stubs` |
| Env URL host (real) | `http://external-real:` | `http://external-system-simulators:` |
| Env URL host (stub) | `http://external-stub:` | `http://external-system-stubs:` |

Note: `EXTERNAL_SYSTEM_MODE=real` / `=stub` env values are unchanged — they tag the mode, not the service.

Apply order matters slightly: do the path substitutions first, then the service-name substitutions. A naive global "external-real → external-system-simulators" would corrupt the path string `external-systems/external-real-sim` if the path rewrite hasn't run yet.

### `systems.yaml` (6 files)

Under `docker/<testLang>/<arch>/systems.yaml`:

- `containerName: external-real` → `containerName: external-system-simulators` (2 occurrences per file under the `real` label's `externalSystems:`).
- `containerName: external-stub` → `containerName: external-system-stubs` (2 occurrences per file under the `stub` label's `externalSystems:`).
- Header prose comment that mentions "vendored real-sim / stub external systems" — update prose to "simulators / stubs".

### `gh-optivem-*.yaml` (12 files)

The 6 active + 6 legacy variants at shop root. Update `external_systems` block:

```yaml
external_systems:
    stubs:
        path: external-systems/stubs           # was external-systems/external-stub
        repo: optivem/shop
    simulators:
        path: external-systems/simulators      # was external-systems/external-real-sim
        repo: optivem/shop
```

### ATDD process docs (3 files)

- `docs/atdd/process/cycles.md` — one prose reference.
- `docs/atdd/process/ct-green-stubs.md` — one prose reference.
- `docs/atdd/process/ct-cycle-conventions.md` — one prose reference.

### Historical plan (leave alone)

- `plans/20260505-move-external-systems-out-of-system.md` is a historical record of the prior reorg; do **not** rewrite its prose. Add a `## Follow-up` cross-link at the bottom pointing to this plan if desired.

### Validation

- `compile-all.sh` + `test-all.sh`.
- For each `(testLang, arch)` combo, bring up the local docker stack against both `real` and `stub` labels; verify health probes pass.
- Run shop's acceptance-stage workflow locally / in a branch CI run.

---

## Step 3 — coordinated merge

Same approach as the 2026-05-05 plan:

1. Land Step 1 (`gh-optivem`) and Step 2 (`shop`) on the **same day**, in two PRs.
2. Bump `gh-optivem` patch version, cut release tag.
3. Run a rehearsal scaffold (`rehearsal-YYYYMMDD-HHMMSS`) from the updated shop using the new `gh-optivem` binary; bring up the scaffolded repo's docker stack; confirm `external-system-simulators` and `external-system-stubs` services come up and health probes pass.
4. Watch CI on shop's acceptance-stage flavors post-merge.

No backwards-compat shim in `copyExternals` (per CLAUDE.md anti-pattern guidance — would mask the silent-failure mode for the next mover).

---

## Out of scope (deliberate non-changes)

- **Compose filename suffixes** `docker-compose.*.real.yml` / `*.stub.yml` — these encode the mode, not the directory name.
- **`label:` field values** `real` / `stub` in `systems.yaml` — same reasoning.
- **`EXTERNAL_SYSTEM_MODE=real|stub` env value** — same reasoning.
- **Go struct fields** `ExternalSystems.Stubs` / `.Simulators` and `ExternalSpec` — already named correctly.
- **Go CLI flags** `--stubs-path` / `--simulators-path` — already named correctly; only default values change.
- **The prior `plans/20260505-...md` document** — historical record, leave intact.

---

## Open questions

None — scoping decisions captured during planning:

1. **Parent dir stays.** `external-systems/` parent kept; only the two leaf names change.
2. **Services renamed too.** With the `external-system-` prefix to keep them self-describing (so a bare `stubs:` service doesn't read as "stubs of what?").
3. **Modes / mode-tagged filenames are not touched.** They describe how the SUT is run, not what's mounted into it.
