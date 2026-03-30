# Starter

A catalog of project templates organized by two independent dimensions: **system** (the application) and **system-test** (the test harness). Each template is self-contained and copy-paste-ready.

## System Templates

Pick based on your architecture and language:

| Architecture | Language | Directory | Framework | Port |
|---|---|---|---|---|
| Monolith | Java | `system/monolith/java/` | Spring Boot + Thymeleaf (SSR) | 8080 |
| Monolith | .NET | `system/monolith/dotnet/` | ASP.NET Core Razor Pages | 8080 |
| Monolith | TypeScript | `system/monolith/typescript/` | Next.js (SSR) | 3000 |
| Multitier | Java (backend) | `system/multitier/backend-java/` | Spring Boot API | 8081 |
| Multitier | .NET (backend) | `system/multitier/backend-dotnet/` | ASP.NET Core API | 8081 |
| Multitier | TypeScript (backend) | `system/multitier/backend-typescript/` | NestJS API | 8081 |
| Multitier | TypeScript (frontend) | `system/multitier/frontend-react/` | React + Nginx | 8080 |

## System-Test Templates

Pick based on your preferred test language (independent of system language):

| Language | Directory | Framework |
|---|---|---|
| Java | `system-test/java/` | JUnit 5 + Playwright |
| .NET | `system-test/dotnet/` | xUnit + Playwright |
| TypeScript | `system-test/typescript/` | Jest + Playwright |

Each system-test includes `docker-compose.single.yml` and `docker-compose.multi.yml` — rename the appropriate one to `docker-compose.yml`.

## CI/CD Pipelines

The `.github/workflows/` directory contains runnable pipelines for all 6 matched-language combinations (system + system-test):

| Combination | Commit Stage | Acceptance | QA | Prod |
|---|---|---|---|---|
| Monolith Java | [![monolith-java-commit-stage](https://github.com/optivem/starter/actions/workflows/monolith-java-commit-stage.yml/badge.svg)](https://github.com/optivem/starter/actions/workflows/monolith-java-commit-stage.yml) | [![monolith-java-acceptance-stage](https://github.com/optivem/starter/actions/workflows/monolith-java-acceptance-stage.yml/badge.svg)](https://github.com/optivem/starter/actions/workflows/monolith-java-acceptance-stage.yml) | [![monolith-java-qa-stage](https://github.com/optivem/starter/actions/workflows/monolith-java-qa-stage.yml/badge.svg)](https://github.com/optivem/starter/actions/workflows/monolith-java-qa-stage.yml) | [![monolith-java-prod-stage](https://github.com/optivem/starter/actions/workflows/monolith-java-prod-stage.yml/badge.svg)](https://github.com/optivem/starter/actions/workflows/monolith-java-prod-stage.yml) |
| Monolith .NET | [![monolith-dotnet-commit-stage](https://github.com/optivem/starter/actions/workflows/monolith-dotnet-commit-stage.yml/badge.svg)](https://github.com/optivem/starter/actions/workflows/monolith-dotnet-commit-stage.yml) | [![monolith-dotnet-acceptance-stage](https://github.com/optivem/starter/actions/workflows/monolith-dotnet-acceptance-stage.yml/badge.svg)](https://github.com/optivem/starter/actions/workflows/monolith-dotnet-acceptance-stage.yml) | [![monolith-dotnet-qa-stage](https://github.com/optivem/starter/actions/workflows/monolith-dotnet-qa-stage.yml/badge.svg)](https://github.com/optivem/starter/actions/workflows/monolith-dotnet-qa-stage.yml) | [![monolith-dotnet-prod-stage](https://github.com/optivem/starter/actions/workflows/monolith-dotnet-prod-stage.yml/badge.svg)](https://github.com/optivem/starter/actions/workflows/monolith-dotnet-prod-stage.yml) |
| Monolith TypeScript | [![monolith-typescript-commit-stage](https://github.com/optivem/starter/actions/workflows/monolith-typescript-commit-stage.yml/badge.svg)](https://github.com/optivem/starter/actions/workflows/monolith-typescript-commit-stage.yml) | [![monolith-typescript-acceptance-stage](https://github.com/optivem/starter/actions/workflows/monolith-typescript-acceptance-stage.yml/badge.svg)](https://github.com/optivem/starter/actions/workflows/monolith-typescript-acceptance-stage.yml) | [![monolith-typescript-qa-stage](https://github.com/optivem/starter/actions/workflows/monolith-typescript-qa-stage.yml/badge.svg)](https://github.com/optivem/starter/actions/workflows/monolith-typescript-qa-stage.yml) | [![monolith-typescript-prod-stage](https://github.com/optivem/starter/actions/workflows/monolith-typescript-prod-stage.yml/badge.svg)](https://github.com/optivem/starter/actions/workflows/monolith-typescript-prod-stage.yml) |
| Multitier Java | [![multitier-backend-java-commit-stage](https://github.com/optivem/starter/actions/workflows/multitier-backend-java-commit-stage.yml/badge.svg)](https://github.com/optivem/starter/actions/workflows/multitier-backend-java-commit-stage.yml) | [![multitier-system-java-acceptance-stage](https://github.com/optivem/starter/actions/workflows/multitier-system-java-acceptance-stage.yml/badge.svg)](https://github.com/optivem/starter/actions/workflows/multitier-system-java-acceptance-stage.yml) | [![multitier-system-java-qa-stage](https://github.com/optivem/starter/actions/workflows/multitier-system-java-qa-stage.yml/badge.svg)](https://github.com/optivem/starter/actions/workflows/multitier-system-java-qa-stage.yml) | [![multitier-system-java-prod-stage](https://github.com/optivem/starter/actions/workflows/multitier-system-java-prod-stage.yml/badge.svg)](https://github.com/optivem/starter/actions/workflows/multitier-system-java-prod-stage.yml) |
| Multitier .NET | [![multitier-backend-dotnet-commit-stage](https://github.com/optivem/starter/actions/workflows/multitier-backend-dotnet-commit-stage.yml/badge.svg)](https://github.com/optivem/starter/actions/workflows/multitier-backend-dotnet-commit-stage.yml) | [![multitier-system-dotnet-acceptance-stage](https://github.com/optivem/starter/actions/workflows/multitier-system-dotnet-acceptance-stage.yml/badge.svg)](https://github.com/optivem/starter/actions/workflows/multitier-system-dotnet-acceptance-stage.yml) | [![multitier-system-dotnet-qa-stage](https://github.com/optivem/starter/actions/workflows/multitier-system-dotnet-qa-stage.yml/badge.svg)](https://github.com/optivem/starter/actions/workflows/multitier-system-dotnet-qa-stage.yml) | [![multitier-system-dotnet-prod-stage](https://github.com/optivem/starter/actions/workflows/multitier-system-dotnet-prod-stage.yml/badge.svg)](https://github.com/optivem/starter/actions/workflows/multitier-system-dotnet-prod-stage.yml) |
| Multitier TypeScript | [![multitier-backend-typescript-commit-stage](https://github.com/optivem/starter/actions/workflows/multitier-backend-typescript-commit-stage.yml/badge.svg)](https://github.com/optivem/starter/actions/workflows/multitier-backend-typescript-commit-stage.yml) | [![multitier-system-typescript-acceptance-stage](https://github.com/optivem/starter/actions/workflows/multitier-system-typescript-acceptance-stage.yml/badge.svg)](https://github.com/optivem/starter/actions/workflows/multitier-system-typescript-acceptance-stage.yml) | [![multitier-system-typescript-qa-stage](https://github.com/optivem/starter/actions/workflows/multitier-system-typescript-qa-stage.yml/badge.svg)](https://github.com/optivem/starter/actions/workflows/multitier-system-typescript-qa-stage.yml) | [![multitier-system-typescript-prod-stage](https://github.com/optivem/starter/actions/workflows/multitier-system-typescript-prod-stage.yml/badge.svg)](https://github.com/optivem/starter/actions/workflows/multitier-system-typescript-prod-stage.yml) |

Multitier frontend (shared): [![multitier-frontend-react-commit-stage](https://github.com/optivem/starter/actions/workflows/multitier-frontend-react-commit-stage.yml/badge.svg)](https://github.com/optivem/starter/actions/workflows/multitier-frontend-react-commit-stage.yml)

QA Signoff: [![monolith-java](https://github.com/optivem/starter/actions/workflows/monolith-java-qa-signoff.yml/badge.svg)](https://github.com/optivem/starter/actions/workflows/monolith-java-qa-signoff.yml) [![monolith-dotnet](https://github.com/optivem/starter/actions/workflows/monolith-dotnet-qa-signoff.yml/badge.svg)](https://github.com/optivem/starter/actions/workflows/monolith-dotnet-qa-signoff.yml) [![monolith-typescript](https://github.com/optivem/starter/actions/workflows/monolith-typescript-qa-signoff.yml/badge.svg)](https://github.com/optivem/starter/actions/workflows/monolith-typescript-qa-signoff.yml) [![multitier-java](https://github.com/optivem/starter/actions/workflows/multitier-system-java-qa-signoff.yml/badge.svg)](https://github.com/optivem/starter/actions/workflows/multitier-system-java-qa-signoff.yml) [![multitier-dotnet](https://github.com/optivem/starter/actions/workflows/multitier-system-dotnet-qa-signoff.yml/badge.svg)](https://github.com/optivem/starter/actions/workflows/multitier-system-dotnet-qa-signoff.yml) [![multitier-typescript](https://github.com/optivem/starter/actions/workflows/multitier-system-typescript-qa-signoff.yml/badge.svg)](https://github.com/optivem/starter/actions/workflows/multitier-system-typescript-qa-signoff.yml)

- **Commit stages** trigger automatically on push via path filters
- **Acceptance/QA/Prod stages** are workflow_dispatch (manual trigger)
