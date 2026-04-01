# eShop Tests (.NET)

[![acceptance-stage](https://github.com/optivem/eshop-tests-dotnet/actions/workflows/acceptance-stage.yml/badge.svg)](https://github.com/optivem/eshop-tests-dotnet/actions/workflows/acceptance-stage.yml)

## Prerequisites

- .NET 8 SDK
- Docker Desktop
- PowerShell 7+

Ensure you have .NET 8 SDK installed:

```powershell
dotnet --version
```

Check that you have PowerShell 7+:

```powershell
$PSVersionTable.PSVersion
```

## Run Everything

```powershell
.\Run-SystemTests.ps1
```

This will:

1. Clone or pull the [monolith repository](https://github.com/optivem/eshop) (Frontend, Backend, Docker setup)
2. Build the Backend and Frontend
3. Start Docker containers (Frontend, Backend, PostgreSQL, & Simulated External APIs)
4. Wait for all services to be healthy
5. Run all System Tests (Smoke, Acceptance, Contract, E2E with xUnit + Playwright)

You can open these URLs in your browser:

- **Frontend UI**: [http://localhost:3001](http://localhost:3001)
- **Backend API**: [http://localhost:8081/api](http://localhost:8081/api)
- **ERP API**: [http://localhost:9001/erp/health](http://localhost:9001/erp/health)
- **Tax API**: [http://localhost:9001/tax/health](http://localhost:9001/tax/health)
- **PostgreSQL**: localhost:5401 (database: `eshop`, user: `eshop_user`, password: `eshop_password`)

## Separate Commands

### Run with Local Build (Default)

Builds locally and runs all system tests:

```powershell
.\Run-SystemTests.ps1
# or explicitly:
.\Run-SystemTests.ps1 local
```

### Run with Pipeline Images

Uses pre-built Docker images from registry:

```powershell
.\Run-SystemTests.ps1 pipeline
```

### Other Options

- `-Rebuild` - Force rebuild before running
- `-Restart` - Restart Docker containers
- `-Suite <id>` - Run a specific test suite (e.g. `smoke-stub`, `acceptance-api`, `e2e-ui`)
- `-Test <method>` - Run a single test method within the suite (e.g. `CanPlaceOrder`)

## Pre-commit Checks

Before each commit, you can run format, lint, and compilation checks.

**One-time setup:**

```powershell
.\Install-PreCommitHook.ps1
```

**Bypass** (when needed): `git commit --no-verify`

**Run checks manually:** `.\Run-PreCommitCheck.ps1`

## License

[![MIT License](https://img.shields.io/badge/license-MIT-lightgrey.svg)](https://opensource.org/licenses/MIT)

This project is released under the [MIT License](https://opensource.org/licenses/MIT).

## Contributors

- [Valentina Jemuović](https://github.com/valentinajemuovic)
- [Jelena Cupać](https://github.com/jcupac)
