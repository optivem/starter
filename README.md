# Starter

A catalog of project templates organized by two independent dimensions: **system** (the application) and **system-test** (the test harness). Each template is self-contained and copy-paste-ready.

## System Templates

Pick based on your architecture and language:

| Architecture | Language | Directory | Framework | Port |
|---|---|---|---|---|
| Single-component | Java | `system/single-component/java/` | Spring Boot + Thymeleaf (SSR) | 8080 |
| Single-component | .NET | `system/single-component/dotnet/` | ASP.NET Core Razor Pages | 8080 |
| Single-component | TypeScript | `system/single-component/typescript/` | Next.js (SSR) | 3000 |
| Multi-component | Java (backend) | `system/multi-component/backend-java/` | Spring Boot API | 8081 |
| Multi-component | .NET (backend) | `system/multi-component/backend-dotnet/` | ASP.NET Core API | 8081 |
| Multi-component | TypeScript (backend) | `system/multi-component/backend-typescript/` | NestJS API | 8081 |
| Multi-component | TypeScript (frontend) | `system/multi-component/frontend-react/` | React + Nginx | 8080 |

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
| Single-component Java | `single-component-java-commit-stage` | `single-component-java-acceptance-stage` | `single-component-java-qa-stage` | `single-component-java-prod-stage` |
| Single-component .NET | `single-component-dotnet-commit-stage` | `single-component-dotnet-acceptance-stage` | `single-component-dotnet-qa-stage` | `single-component-dotnet-prod-stage` |
| Single-component TypeScript | `single-component-typescript-commit-stage` | `single-component-typescript-acceptance-stage` | `single-component-typescript-qa-stage` | `single-component-typescript-prod-stage` |
| Multi-component Java | `multi-component-backend-java-commit-stage` | `multi-component-java-acceptance-stage` | `multi-component-java-qa-stage` | `multi-component-java-prod-stage` |
| Multi-component .NET | `multi-component-backend-dotnet-commit-stage` | `multi-component-dotnet-acceptance-stage` | `multi-component-dotnet-qa-stage` | `multi-component-dotnet-prod-stage` |
| Multi-component TypeScript | `multi-component-backend-typescript-commit-stage` | `multi-component-typescript-acceptance-stage` | `multi-component-typescript-qa-stage` | `multi-component-typescript-prod-stage` |

Plus `multi-component-frontend-react-commit-stage` (shared across all multi-component variants).

- **Commit stages** trigger automatically on push via path filters
- **Acceptance/QA/Prod stages** are workflow_dispatch (manual trigger)
