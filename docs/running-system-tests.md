# Running System Tests

## All languages at once — `Run-AllSystemTests.ps1`

From the repo root:

```powershell
./Run-AllSystemTests.ps1 -Architecture monolith
```

Runs both **latest** and **legacy** suites across all three languages (.NET, Java, TypeScript) sequentially, prints a per-language / per-phase summary, and exits non-zero on any failure.

Useful flags:

- `-Languages dotnet,java` — run only a subset of languages
- `-Rebuild` — rebuild containers before running tests
- `-Mode pipeline` — use pipeline config instead of local

This is the preferred entry point for verifying cross-language changes.

## Single language — `Run-SystemTests.ps1`

From `system-test/<language>/`:

```powershell
./Run-SystemTests.ps1 -Architecture monolith            # latest suites
./Run-SystemTests.ps1 -Architecture monolith -Legacy    # legacy suites
./Run-SystemTests.ps1 -Architecture monolith -Sample    # one sample per suite (fast smoke)
```

Use this when iterating on a single language, or for the `-Sample` pre-commit verification described in [CLAUDE.md](../CLAUDE.md).

Do **not** substitute `./gradlew test`, `mvn test`, `dotnet test`, or `npm test` — these wrappers manage Docker containers and config selection that the raw toolchain does not.
