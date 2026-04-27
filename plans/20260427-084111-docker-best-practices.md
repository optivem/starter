# Plan — Docker Best Practices Cleanup

**Date:** 2026-04-27
**Source:** Review of all 7 Dockerfiles, 7 .dockerignore files, and 24 docker-compose files in the shop repo.
**Scope:** Apply Docker best practices that are currently missing. Items ordered by priority (security/correctness first, then performance, then hygiene). Each item is independent and commit-safe on its own.

---

## 1. Add non-root `USER` to all app Dockerfiles

**Status:** Only `system/monolith/typescript/Dockerfile` runs as non-root. The other 6 run as root.

**Affected files:**
- `system/monolith/java/Dockerfile`
- `system/multitier/backend-java/Dockerfile`
- `system/monolith/dotnet/Dockerfile`
- `system/multitier/backend-dotnet/Dockerfile`
- `system/multitier/backend-typescript/Dockerfile`
- `system/multitier/frontend-react/Dockerfile`

**Actions:**
- Java (both): add `RUN addgroup -S app && adduser -S app -G app`, `USER app`, and `--chown=app:app` on the JAR copy.
- .NET (both): add `USER app` (the official `mcr.microsoft.com/dotnet/aspnet:8.0` image already ships an `app` user).
- backend-typescript: switch final stage to `USER node` (image already has it) — verify `dist/` and `node_modules/` are readable.
- frontend-react: replace `nginx:alpine` with `nginxinc/nginx-unprivileged:alpine`. Update `EXPOSE` to 8080 and pipeline compose port mappings if needed.

**Verification:** Build each image and `docker run --rm <image> id` — should NOT show uid 0.

---

## 2. Add `HEALTHCHECK` to every app Dockerfile

**Status:** Postgres has a healthcheck; no app service does. `depends_on` only waits for container start, not readiness.

**Affected files:** All 7 Dockerfiles.

**Actions:**
- Java: `HEALTHCHECK --interval=10s --timeout=3s --start-period=20s --retries=5 CMD wget -qO- http://localhost:8081/actuator/health || exit 1` (verify Spring Actuator health endpoint exists; if not, expose one).
- .NET: same pattern, hitting `/health` (verify Microsoft.AspNetCore.Diagnostics.HealthChecks is wired; add if missing).
- TypeScript backend: same pattern, hitting whatever existing health endpoint (verify in code).
- TS monolith (Next.js): hit `/api/health` or root path.
- React frontend (nginx): `wget -qO- http://localhost:8080/ || exit 1`.

**Compose follow-up:** Switch all `depends_on: <backend>` lines (without condition) to `depends_on: { <svc>: { condition: service_healthy } }`. Affects all 12 multitier compose files.

**Verification:** `docker compose ps` shows `(healthy)` after startup.

---

## 3. Fix .NET NuGet cache mount

**Status:** `--mount=type=cache,target=/root/.nuget/packages` is silently ignored — `dotnet restore` writes to `~/.nuget/packages` (per `$HOME`), not `/root/.nuget/packages`, unless `NUGET_PACKAGES` is set.

**Affected files:**
- `system/monolith/dotnet/Dockerfile`
- `system/multitier/backend-dotnet/Dockerfile`

**Actions:**
- Add `ENV NUGET_PACKAGES=/root/.nuget/packages` before the `RUN dotnet restore` line in the build stage.
- Add `global.json* Directory.Packages.props*` to the `COPY *.csproj ./` line so future central package management works.

**Verification:** Time a clean build then a re-build of the same image — second build should skip restore.

---

## 4. Dedupe `npm ci` in backend-typescript

**Status:** `system/multitier/backend-typescript/Dockerfile` runs `npm ci` twice (build stage full deps, runtime stage `--omit=dev`). Two cold installs.

**Action:** Restructure to a `deps-prod` stage that runs `npm ci --omit=dev` once in parallel with the build stage. Copy `node_modules` from `deps-prod` into the runtime stage.

**Template:**
```dockerfile
FROM node:22-alpine AS deps-prod
WORKDIR /app
COPY package*.json ./
RUN --mount=type=cache,target=/root/.npm npm ci --omit=dev

FROM node:22-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN --mount=type=cache,target=/root/.npm npm ci
COPY . .
RUN npm run build

FROM node:22-alpine
WORKDIR /app
ENV NODE_ENV=production
COPY --from=deps-prod /app/node_modules ./node_modules
COPY --from=build /app/dist ./dist
COPY --from=build /app/package*.json ./
USER node
EXPOSE 8081
CMD ["node", "dist/main"]
```

**Verification:** Build is faster on a warm cache; final image still runs.

---

## 5. Drop gradlew CRLF workaround, fix at source

**Status:** Both Java Dockerfiles run `sed -i 's/\r$//' gradlew && chmod +x gradlew` to fix Windows line endings — band-aid for misconfigured git attributes.

**Affected files:**
- `system/monolith/java/Dockerfile`
- `system/multitier/backend-java/Dockerfile`
- `system/monolith/java/.gitattributes` (create)
- `system/multitier/backend-java/.gitattributes` (create)

**Actions:**
1. Create `.gitattributes` in each Java module:
   ```
   * text=auto eol=lf
   gradlew text eol=lf
   *.sh    text eol=lf
   *.bat   text eol=crlf
   ```
2. Re-normalize: `git add --renormalize .` then commit.
3. Replace the `sed && chmod` line with just `chmod +x gradlew` (or even drop chmod if file mode is preserved).

**Verification:** Fresh clone on Windows still builds; gradlew has LF endings in the repo.

---

## 6. Bump `external-real-sim` from node:18 to node:22 + bake into Dockerfile

**Status:** All 12 `*-real.yml` compose files use `image: node:18-alpine` then run `sh -c "npm install && npm start"` at container start. Node 18 reached EOL April 2025. The runtime `npm install` adds 10–30s to every `up`.

**Affected files:**
- `system/external-real-sim/Dockerfile` (create)
- All 12 `docker-compose.*.real.yml` files (replace `image: node:18-alpine` + volume + working_dir + command with `build: ../../../system/external-real-sim` for local files, and `image: ghcr.io/...` for pipeline files — match existing pattern).

**Actions:**
1. Create `system/external-real-sim/Dockerfile`:
   ```dockerfile
   # syntax=docker/dockerfile:1.4
   FROM node:22-alpine AS deps
   WORKDIR /app
   COPY package*.json ./
   RUN --mount=type=cache,target=/root/.npm npm ci --omit=dev

   FROM node:22-alpine
   WORKDIR /app
   COPY --from=deps /app/node_modules ./node_modules
   COPY . .
   USER node
   EXPOSE 9000
   CMD ["npm", "start"]
   ```
2. Add `system/external-real-sim/.dockerignore`.
3. Update all 6 `docker-compose.local.*.real.yml` to use `build:` instead of `image: node:18-alpine` + volume mount.
4. Update all 6 `docker-compose.pipeline.*.real.yml` to use a published image (matches existing pattern for backend/frontend).
5. Add a publish workflow for the new image (mirror existing patterns under `.github/workflows/`).

**Verification:** `docker compose up` starts external-real-sim in <5s instead of 15-30s.

---

## 7. Pin image digests in pipeline compose files

**Status:** All compose files use mutable tags (`postgres:16-alpine`, `wiremock/wiremock:3.10.0`, `node:18-alpine`). For reproducibility in CI, pin digests.

**Affected files:** 12 `docker-compose.pipeline.*.yml` files.

**Actions:**
- For each external image used in pipeline compose, look up the current digest (`docker pull <image> && docker inspect <image> --format '{{index .RepoDigests 0}}'`) and pin: `image: postgres:16-alpine@sha256:...`.
- Skip local compose files — keep them on tags for dev convenience.
- Add Renovate/Dependabot config to keep digests fresh (check if already exists in `.github/`).

**Verification:** `docker compose -f docker-compose.pipeline.X.yml pull` returns stable digests.

---

## 8. Tighten `.dockerignore` files

**Status:** Each `.dockerignore` lists `README.md` and `Run-Sonar.ps1` individually. Build context could be smaller and more uniform.

**Affected files:** All 7 `.dockerignore` files.

**Actions per file:**
- Replace `README.md` (and `HELP.md` for Java) with broader `*.md` + `!path/needed.md` if any markdown is consumed at build time (none observed).
- Add: `Dockerfile.*`, `docker-compose*.yml`, `.github/`, `.gitignore`, `.editorconfig`, `*.sln` (.NET only), `tests/`, `test/` (TS — verify not needed for build first).
- Verify build still works after each addition (`docker build .` should not error on missing files).

**Verification:** `docker build` shows smaller "transferring context" size.

---

## 9. Frontend nginx — drop shell wrapper in CMD

**Status:** `system/multitier/frontend-react/Dockerfile` line 32 uses `CMD ["/bin/sh", "-c", "envsubst ... && nginx -g 'daemon off;'"]`. The official `nginx:alpine` (and `nginxinc/nginx-unprivileged:alpine`) image already supports `/etc/nginx/templates/*.template` natively via its entrypoint script.

**Action:** Remove the shell wrapper. Just `COPY nginx.conf /etc/nginx/templates/default.conf.template` and let the image's entrypoint handle envsubst. Default `CMD` becomes unnecessary.

**Verification:** Container starts, `$BACKEND_API_URL` is substituted.

---

## 10. Postgres password env var fallback in pipeline compose

**Status:** All compose files hardcode `POSTGRES_PASSWORD=app`. Fine for local; for pipeline.real running in CI, allow override.

**Affected files:** 12 `docker-compose.pipeline.*.yml` files.

**Action:** Change `POSTGRES_PASSWORD=app` to `POSTGRES_PASSWORD=${POSTGRES_PASSWORD:-app}` in pipeline compose files. Same for backend `POSTGRES_DB_PASSWORD`. Local compose stays as-is (dev convenience).

**Verification:** `POSTGRES_PASSWORD=secret docker compose -f ...pipeline... up` overrides; without override falls back to `app`.

---

## Order of execution

Execute in numerical order. Each item is committable on its own — commit after each item lands and tests pass.

After all items complete, delete this file. If `plans/` is empty, delete the directory.
