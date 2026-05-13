# Running System Tests

The `gh optivem` CLI extension orchestrates the docker-compose stacks and runs
the test suites. Install it once with:

```bash
gh extension install optivem/gh-optivem
```

## All languages at once — `test-all.sh`

From the repo root:

```bash
./test-all.sh -a monolith
```

Runs both **latest** and **legacy** suites across all three languages (.NET,
Java, TypeScript) sequentially, prints a per-language / per-phase summary, and
exits non-zero on any failure.

Useful flags:

- `-l dotnet,java` — run only a subset of languages
- `-a multitier` — run against the multitier architecture instead of monolith

This is the preferred entry point for verifying cross-language changes.

## Single language — `gh optivem test run`

Point `GH_OPTIVEM_CONFIG` at the variant yaml at the repo root, then run the
commands without per-flag overrides. Substitute `<language>` ∈ {java, dotnet,
typescript} and `<architecture>` ∈ {monolith, multitier}:

```pwsh
$env:GH_OPTIVEM_CONFIG = "gh-optivem-<architecture>-<language>.yaml"

# Bring up the docker-compose stacks
gh optivem system start

# Prepare the test harness (npm ci / gradle compile / dotnet build, depending on language)
gh optivem test setup

# Run the latest suites
gh optivem test run

# Or a fast smoke (one sample per suite)
gh optivem test run --sample

# Stop when done
gh optivem system stop
```

For the legacy suites, switch the env var to the `-legacy` sibling and re-run setup (legacy has its own setupCommands block):

```pwsh
$env:GH_OPTIVEM_CONFIG = "gh-optivem-<architecture>-<language>-legacy.yaml"
gh optivem test setup
gh optivem test run
```

Use this when iterating on a single language, or for the `--sample`
pre-commit verification described in [CLAUDE.md](../../CLAUDE.md).

Do **not** substitute `./gradlew test`, `mvn test`, `dotnet test`, or `npm
test` — these wrappers manage Docker containers and per-suite environment
variables that the raw toolchain does not.
