# CT - GREEN - STUBS

## Purpose

Bring the dockerized External System stub into contract-compatibility with the real Test Instance, so the stub-suite contract tests pass. This is the only GREEN phase in the CT sub-process — it closes the loop opened by CT - RED - TEST.

## What it produces

- Commit `<Ticket> | CT - GREEN - STUBS` containing stub server changes (routes, fixtures) and the re-enabled contract tests
- Tests in state: contract tests enabled and PASSING against `<suite-contract-stub>`

## Conventions

- The dockerized stub follows the json-server pattern — see [`system/external-real-sim`](../../../system/external-real-sim) for the canonical reference (`mock-server.js`, `Dockerfile`).
- Stub data must reflect the real Test Instance's contract — same shapes, same status codes, same error semantics. Drift between stub and real instance breaks the CT cycle.
- Suite selection (real vs stub): see [ct-cycle-conventions.md](ct-cycle-conventions.md). This phase exercises the stub side only.
- Commit message format: see [ct-cycle-conventions.md](ct-cycle-conventions.md).
- Commit gate ("Can I commit?"): see [shared-commit-confirmation.md](shared-commit-confirmation.md).
- Phase progression and STOP semantics: see [shared-phase-progression.md](shared-phase-progression.md).
- `@Disabled` removal syntax per language: see [language-equivalents.md](../code/language-equivalents.md).

## Example

A new stub route added to `mock-server.js` to honor a contract method that previously returned 404.

```javascript
// Promotion endpoint - returns default no-promotion state
server.get('/erp/api/promotion', (req, res) => {
  res.status(200).json({
    promotionActive: false,
    discount: 1.0
  });
});
```

## CT - GREEN - STUBS - WRITE

1. Enable the tests marked disabled with reason `"CT - RED - EXTERNAL DRIVER"`.
2. Implement the dockerized External System stub changes — add or update routes, fixtures, or middleware so the stub honors the new contract.
3. Run the External System Contract Tests against the stub:
   ```bash
   gh optivem test system --rebuild --suite <suite-contract-stub> --test <TestMethodName>
   ```
4. Verify that the tests pass. If they fail, ask the user. STOP. Do NOT continue.

## CT - GREEN - STUBS - REVIEW (STOP)

STOP. Present the stub implementation and the passing test output to the user and ask for approval. Do NOT continue.

**Review checklist:**

- Stub routes/fixtures match the real Test Instance's contract — shapes, status codes, error semantics.
- Tests pass against `<suite-contract-stub>`.
- The `@Disabled` annotation with reason `"CT - RED - EXTERNAL DRIVER"` has been removed.

## CT - GREEN - STUBS - COMMIT

1. Confirm the `@Disabled` annotation (reason `"CT - RED - EXTERNAL DRIVER"`) has been removed from the tests.
2. Run the contract tests one more time and verify they still pass:
   ```bash
   gh optivem test system --suite <suite-contract-stub> --test <TestMethodName>
   ```
3. COMMIT with message `<Ticket> | CT - GREEN - STUBS`.

## Anti-patterns

- Forgetting to remove the `@Disabled` (reason `"CT - RED - EXTERNAL DRIVER"`) — the tests look passing locally but are silently skipped in CI.
- Modifying the real Test Instance instead of the stub — the real instance is owned by the external system, not by this repo.
- Letting stub data drift from the real-instance contract — if the stub returns shapes the real instance would not, the contract tests stop being meaningful. Mirror the real instance.
- Adding "test-only" branches to the stub that the real instance does not honor — same drift problem, harder to spot.
