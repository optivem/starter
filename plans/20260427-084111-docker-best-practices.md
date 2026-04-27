# Plan — Docker Best Practices Cleanup

**Date:** 2026-04-27
**Source:** Review of all 7 Dockerfiles, 7 .dockerignore files, and 24 docker-compose files in the shop repo.
**Scope:** Apply Docker best practices that are currently missing. Items ordered by priority (security/correctness first, then performance, then hygiene). Each item is independent and commit-safe on its own.

---

## 7. Pin image digests in pipeline compose files — DEFERRED

**Status:** Deferred 2026-04-27. Verified that no Renovate/Dependabot config exists in this repo. Pinning digests without an automated update mechanism would cause silent rot — pinned digests become stale, security patches get missed.

**Prerequisite:** Set up Renovate or Dependabot first (separate task), then revisit this.

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
