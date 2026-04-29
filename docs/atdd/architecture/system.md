# System Code Layout

The `system/` tree holds the production code under test. It is organised by **deployment shape** first, then by **language**.

## Parallel Implementations

The shop has parallel implementations across three languages. CI runs every one of them; a change to the System API or System UI must be applied to every implementation that exposes that layer.

| Deployment shape | Java | .NET | TypeScript |
| --- | --- | --- | --- |
| Monolith (backend + UI in one process) | `system/monolith/java/` | `system/monolith/dotnet/` | `system/monolith/typescript/` |
| Multitier — backend (HTTP API only) | `system/multitier/backend-java/` | `system/multitier/backend-dotnet/` | `system/multitier/backend-typescript/` |
| Multitier — frontend (UI only) | (uses `frontend-react`) | (uses `frontend-react`) | `system/multitier/frontend-react/` |

The multitier frontend is a single React app shared across all three backend languages. The monolith has three distinct UI implementations, one per language.

## Where System API Surface Lives

A System API endpoint is referenced in up to three places per implementation. When the surface changes (rename, signature, status codes), every reference site must be updated.

1. **Backend route** — the controller declares the route:
   - Java (Spring): `@GetMapping`/`@PostMapping`/etc. on the controller method.
   - .NET (ASP.NET Core): `[HttpGet]`/`[HttpPost]`/etc. on the action method, optionally with a class-level `[Route]`.
   - TypeScript multitier (NestJS): `@Get`/`@Post`/etc. on the controller method, optionally with a class-level `@Controller`.
   - TypeScript monolith (Next.js): the route is determined by the **filesystem path** under `src/app/api/<path>/route.ts`. Renaming the URL means moving the directory — editing the file in place is not sufficient.
2. **UI fetch sites** — any page or service that calls the endpoint. In the monolith this is the per-language UI (Spring Thymeleaf templates, ASP.NET Razor pages, Next.js page components). In the multitier setup it is the shared `frontend-react` app's service layer.
3. **Driver adapter constant** — the endpoint URL is encoded as a constant inside the matching resource controller in `system-test/.../driver/adapter/myShop/api/client/controllers/`. Updating the constant is enough; the driver port interface stays untouched. See `driver-adapter.md`.

After editing the source of truth (the backend route), grep the system tree for residual references — UI fetch sites are easy to miss because they live in a different language and folder than the controller.

## Where System UI Surface Lives

System UI surface (page structure, form fields, navigation, copy, selectors) lives in the per-shape, per-language UI code:

- Monolith: `system/monolith/<lang>/` — Thymeleaf templates (Java), Razor pages (.NET), Next.js page components (TypeScript).
- Multitier: `system/multitier/frontend-react/`.

The matching driver adapter is the UI driver under `system-test/.../driver/adapter/myShop/ui/`. See `driver-adapter.md` § Shop UI Driver for the page-object conventions.

## Read-only Areas

`system-test/<lang>/.../Legacy/` is read-only course-reference material. It may reference older API or UI surface; leave it untouched even when a redesign breaks its references — it is not part of the latest test suite and is not run by CI's Acceptance Stage.
