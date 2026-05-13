# System Test (Java)

## Prerequisites

- gh CLI with the optivem extension: `gh extension install optivem/gh-optivem`
- Docker Desktop (running)
- JDK 21+

## Running Tests

All commands are run from the repo root. Point `GH_OPTIVEM_CONFIG` at the variant yaml at the repo root once per shell, then run the verbs without per-flag overrides:

```pwsh
$env:GH_OPTIVEM_CONFIG = "gh-optivem-monolith-java.yaml"
```

Bring up the system stack (real + stub) for the chosen architecture:

```bash
gh optivem system start
```

Prepare the test harness (gradle compile of test sources):

```bash
gh optivem test setup
```

Run all latest test suites:

```bash
gh optivem test run
```

Run legacy test suites — switch the env var first, then re-run setup (legacy has its own setupCommands block):

```pwsh
$env:GH_OPTIVEM_CONFIG = "gh-optivem-monolith-java-legacy.yaml"
gh optivem test setup
gh optivem test run
```

Run only sample tests (one per suite, fast smoke):

```bash
gh optivem test run --sample
```

Run a specific suite by ID:

```bash
gh optivem test run --suite acceptance-api
```

Rebuild container images before bringing the system up:

```bash
gh optivem system build
```

Stop the system when done:

```bash
gh optivem system stop
```

Substitute `gh-optivem-multitier-java.yaml` for the multitier architecture.

## Available Suite IDs

| ID | Description |
|----|-------------|
| `smoke-stub` | Smoke tests (stub) |
| `smoke-real` | Smoke tests (real) |
| `acceptance-api` | Acceptance tests - API channel |
| `acceptance-ui` | Acceptance tests - UI channel |
| `acceptance-isolated-api` | Isolated acceptance tests - API channel |
| `acceptance-isolated-ui` | Isolated acceptance tests - UI channel |
| `contract-stub` | Contract tests (stub) |
| `contract-stub-isolated` | Isolated contract tests (stub) |
| `contract-real` | Contract tests (real) |
| `e2e-api` | E2E tests (real) - API channel |
| `e2e-ui` | E2E tests (real) - UI channel |
