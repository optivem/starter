# Migration Plan: Starter TypeScript — Jest to Playwright

## Context

The `starter` repo's TypeScript tests use Jest + manual browser lifecycle. The `eshop-tests` repo uses Playwright + `@optivem/optivem-testing` with automatic fixture-based lifecycle. The goal is to make starter match eshop-tests as closely as possible. This is the first step of a larger restructuring (monorepo conversion comes later).

## Scope

- Replace Jest with Playwright as test runner
- Switch from CommonJS to ES modules
- Add `@optivem/optivem-testing` (bindChannels, bindTestEach, ChannelListReporter)
- Convert all test files to use Playwright fixtures + forChannels pattern
- Keep internal src/ code (drivers, DSL) structurally unchanged

## Key Architectural Gap

The starter has no `UseCaseDsl` class. In eshop-tests, `UseCaseDsl` wraps `Configuration` + `UseCaseContext` and lazily creates DSL objects (ShopDsl, ErpDsl, etc.) with a `close()` method. The fixture chain is: `withApp()` creates `UseCaseDsl` -> `withScenario()` creates `ScenarioDsl(useCaseDsl)`.

The starter has `UseCaseContext` + `AppContext` which together serve a similar purpose, but `createScenario()` in `test-setup.ts` assembles them directly into `ScenarioDsl(app, useCaseContext)`. There is no single wrapping class.

So we collapse eshop-tests' `withApp()`+`withScenario()` two-layer pattern into a single `withApp()` fixture that provides `{ scenario }` directly. The two-layer split will be added when the monorepo restructuring introduces a proper `UseCaseDsl` class.

The starter's `ShopUiDriver` takes a `Browser` parameter (unlike eshop-tests where it's internal). The `withApp()` fixture creates/closes the browser internally, reading channel from `process.env.CHANNEL` (set by `forChannels()`).

## Steps

### Step 1: Config files

- [ ] **Delete** `jest.config.js`
- [ ] **Replace** `package.json`:
  - Add `"type": "module"`
  - Remove: `jest`, `ts-jest`, `@types/jest`
  - Add: `@playwright/test` (^1.41.0), `@optivem/optivem-testing` (1.1.4)
  - Keep: `playwright` (^1.55.0), `typescript`
  - Replace scripts with: `playwright test`, `playwright test --project=acceptance-test`, etc.
- [ ] **Replace** `tsconfig.json`: ES2022, Node16, moduleResolution node16, types: ["node", "@playwright/test"]
- [ ] **Create** `playwright.config.ts` (copy verbatim from eshop-tests)
- [ ] **Create** `channel-list-reporter.ts` (copy verbatim from eshop-tests)
- [ ] **Create** `setup-config.ts`: just `process.env.TZ = 'UTC';`

### Step 2: Rename test/ to tests/

- [ ] Rename `test/` directory to `tests/` to match eshop-tests convention and playwright.config.ts testDir

### Step 3: ESM conversion of src/ files

- [ ] Add `.js` extensions to all relative imports in every file under `src/` and `config/`
- [ ] Fix `config/configuration-loader.ts`: replace `__dirname` with `import.meta.url` pattern:
  ```typescript
  import { fileURLToPath } from 'node:url';
  import { dirname, join } from 'node:path';
  const __filename = fileURLToPath(import.meta.url);
  const __dirname = dirname(__filename);
  ```

### Step 4: Create Playwright infrastructure

- [ ] **Create** `src/playwright/withApp.ts` (adapted from eshop-tests):
  ```typescript
  import { test as base } from '@playwright/test';
  import { chromium } from 'playwright';
  import { createScenario, type Channel, type ExternalSystemMode } from '../test-setup.js';
  import type { ScenarioDsl } from '../dsl/scenario/scenario-dsl.js';

  export function withApp() {
      return base.extend<{ scenario: ScenarioDsl }>({
          scenario: async ({}, use) => {
              const channel = (process.env.CHANNEL?.toLowerCase() || 'api') as Channel;
              const mode = (process.env.EXTERNAL_SYSTEM_MODE?.toLowerCase() || 'real') as ExternalSystemMode;
              let browser;
              if (channel === 'ui') {
                  browser = await chromium.launch();
              }
              const scenario = createScenario({ channel, externalSystemMode: mode, browser });
              await use(scenario);
              await scenario.close();
              if (browser) await browser.close();
          },
      });
  }
  ```

### Step 5: Create fixture files

- [ ] `tests/latest/smoke/fixtures.ts`
- [ ] `tests/latest/acceptance/base/fixtures.ts` (forces `EXTERNAL_SYSTEM_MODE=stub`)
- [ ] `tests/latest/e2e/base/fixtures.ts`
- [ ] `tests/latest/contract/base/fixtures.ts`
- [ ] One fixture per legacy module that needs it (mod04-mod11 smoke/e2e/acceptance/contract)

All follow this template:
```typescript
import { bindChannels, bindTestEach } from '@optivem/optivem-testing';
import { withApp } from '<relative-path>/src/playwright/withApp.js';

const _test = withApp();
const test = Object.assign(_test, { each: bindTestEach(_test) });
const { forChannels } = bindChannels(test);
export { test, forChannels };
export { expect } from '@playwright/test';
```

### Step 6: Convert latest test files

Convert all tests under `tests/latest/` (smoke, acceptance, e2e, contract). ~30 spec files. Conversion rules:

| Jest (before) | Playwright (after) |
|---|---|
| `import { chromium, Browser } from 'playwright'` | Remove |
| `import { createScenario } from '...'` | `import { test, forChannels } from './fixtures.js'` |
| `describe('Name', () => { ... })` | `forChannels('ui', 'api')(() => { ... })` or remove |
| `let browser; beforeAll/afterAll` | Remove (fixture handles it) |
| `it('name', async () => { ... })` | `test('name', async ({ scenario }) => { ... })` |
| `it.each(cases)('name', async (data) => { ... })` | `test.each(cases)('name', async ({ scenario, ...data }) => { ... })` |
| `const scenario = createScenario({...})` | Remove (use `{ scenario }` from fixture) |
| `try { ... } finally { await scenario.close(); }` | Just the test body (fixture handles cleanup) |
| `if (channel !== 'api') return;` | `forChannels('api')(() => { ... })` |

### Step 7: Convert legacy test files

Convert all tests under `tests/legacy/` (mod02-mod11). ~40 spec files.

- [ ] **Delete `tests/smoke-tests/`** — superseded by `tests/latest/smoke/` and `tests/legacy/mod02/smoke/`; no equivalent in eshop-tests
- [ ] **mod02**: Raw HTTP tests, use `test` from `@playwright/test` directly (like eshop-tests mod02)
- [ ] **mod03-mod09**: Need their own `fixtures.ts` per test category (smoke, e2e)
- [ ] **mod10**: Acceptance tests with fixtures forcing STUB
- [ ] **mod11**: Contract + e2e tests

### Step 8: Cleanup

- [ ] Delete `jest.config.js` (if not already done in Step 1)
- [ ] `npm install` fresh (delete node_modules + package-lock.json first)
- [ ] Add `playwright-report/`, `test-results/` to `.gitignore`

## Verification

- [ ] `npx playwright test --list` should list all tests
- [ ] Run `./Run-SystemTests.ps1` with latest config to verify latest tests pass
- [ ] Run `./Run-SystemTests.ps1` with legacy config to verify legacy tests pass

## Rules

- **Complete the entire migration before running tests.** Do not run tests partway through — finish all steps first, then verify.
- **Run both latest and legacy tests** using `./Run-SystemTests.ps1` after the migration is complete.
- **NEVER commit without explicit user approval.** After tests pass, ask the user for permission to commit. Do not commit automatically.
