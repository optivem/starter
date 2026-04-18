# TypeScript — System Test Alignment Plan

Reference report: `reports/20260417-1841-compare-tests-both.md`
Reference implementation: **Java** (align TypeScript to Java unless otherwise noted).

Remaining items below. Items 2–6, 8, 9 were executed in commit `e95ee5a`.

---

## 1. Architecture — Scenario DSL Port: delete dead `then-failure-and.ts`

Target file:
- `system-test/typescript/src/testkit/dsl/port/then/steps/then-failure-and.ts` — **delete**.

Rationale:
- The file exports an interface `ThenFailureAnd` that is not imported anywhere in the codebase (checked via grep in the report).
- TypeScript's actual `ThenFailure.and()` returns `this` (see `src/testkit/dsl/core/scenario/then/then-place-order.ts`), so no `ThenFailureAnd` navigation type is used.
- Java reference does not have an equivalent interface; .NET has `IThenFailureAnd.cs` only because of its async Success/Failure split (listed as a known .NET-only divergence — see report "Exceptions").

Do not add a matching `then-success-and.ts`. TypeScript's current one-step `.and()` pattern is closer to Java's `ThenStep<TThen>.and()` and is preferred.

VJ: how about in .NET?

## 7. Architecture — UseCaseDsl: rename entry-point `useCase` → `app`

Targets:
- `system-test/typescript/src/testkit/dsl/core/usecase/UseCaseDsl.ts`: keep the class name `UseCaseDsl` but rename the test-facing fixture/property name to `app` (mirror Java `app.shop()...`, `app.erp()...`).
- Fixture file exposing the UseCaseDsl to Playwright tests (likely `tests/legacy/mod07/e2e/fixtures.ts`): rename the exposed fixture key from `useCase` to `app`.
- Legacy mod07 specs that destructure `{ useCase }` (at minimum):
  - `tests/legacy/mod07/e2e/place-order-positive-test.spec.ts`
  - `tests/legacy/mod07/e2e/place-order-negative-test.spec.ts`
  - `tests/legacy/mod07/smoke/external/erp-smoke-test.spec.ts`
  - `tests/legacy/mod07/smoke/external/tax-smoke-test.spec.ts`
  - `tests/legacy/mod07/smoke/system/shop-smoke-test.spec.ts`

  Rename `{ useCase }` → `{ app }` and `useCase.shop()...` → `app.shop()...`. Do a repo-wide grep for `{ useCase }` to catch any other consumers.

Java reference: `.../systemtest/legacy/mod07/e2e/PlaceOrderPositiveTest.java` uses `app.erp().returnsProduct()...execute()`.

VJ: Approved
VJ: also creat e ticket regaridng naming app vs usecase

---

## Follow-up tickets (from VJ comments on executed items)

- Item 2 (done, SystemError relocate): VJ suggested creating a ticket to rename `SystemError` → `ShopError`.
- Item 9 (done, mod11 Tax contract trio): VJ asked whether the "base test class via `register...` function" pattern used here could also be applied elsewhere where base test classes were originally intended.

---

## Local verification & commit

From `system-test/typescript/`:

1. Run the latest suite:
   ```
   ./Run-SystemTests.ps1 -Architecture monolith
   ```
2. Run the legacy suite:
   ```
   ./Run-SystemTests.ps1 -Architecture monolith -Legacy
   ```
3. Fix any failures before proceeding. Do not substitute raw `npx playwright test` or `npm test` — `Run-SystemTests.ps1` is the only supported entry point because it manages containers and configuration.
