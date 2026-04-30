# CT - RED - EXTERNAL DRIVER

## Purpose

Replace the `"TODO: Driver"` prototypes left behind by CT - RED - DSL with real External System Driver logic. The contract tests are still expected to fail at the end of this phase — the dockerized stub does not yet honor the new contract; that's CT - GREEN - STUBS.

## What it produces

- Commit `<Ticket> | CT - RED - EXTERNAL DRIVER` containing real External System Driver implementations
- Tests in state: contract tests disabled with reason `"CT - RED - EXTERNAL DRIVER"`
- GitHub issue comment summarising Driver interface changes (if an issue number was provided as input)

## Conventions

- Scope is strictly limited to files under `external/` (e.g. `driver-port/.../external/...`, `driver-adapter/.../external/...`). Files under `shop/` are off-limits in this phase. See [glossary.md](glossary.md).
- Suite selection (real vs stub): see [ct-cycle-conventions.md](ct-cycle-conventions.md). This phase exercises the stub side only.
- Commit message format: see [ct-cycle-conventions.md](ct-cycle-conventions.md).
- Commit gate ("Can I commit?"): see [shared-commit-confirmation.md](shared-commit-confirmation.md).
- Phase progression and STOP semantics: see [shared-phase-progression.md](shared-phase-progression.md).
- `@Disabled` syntax per language: see [language-equivalents.md](../code/language-equivalents.md).

## Example

Replace the `"TODO: Driver"` prototype with a real HTTP call to the external system. The Driver translates between the DSL's needs and the external API's wire shape.

```diff
 public PromotionResponse getPromotion() {
-    throw new UnsupportedOperationException("TODO: Driver");
+    HttpResponse<String> response = httpClient.send(
+        HttpRequest.newBuilder()
+            .uri(URI.create(baseUrl + "/erp/api/promotion"))
+            .GET()
+            .build(),
+        BodyHandlers.ofString());
+    return objectMapper.readValue(response.body(), PromotionResponse.class);
 }
```

## CT - RED - EXTERNAL DRIVER - WRITE

1. Enable the tests marked disabled with reason `"CT - RED - DSL"`.
2. Implement the External System Drivers — replace each `"TODO: Driver"` prototype with actual logic.
   - Only edit files under `external/` (driver-port and driver-adapter).
   - Do NOT read external-system source code to figure out behavior; rely on the contract tests and the published external API contract.
3. Run the contract tests against the stub and verify they fail with a runtime error (the stub does not yet implement the new contract):
   ```bash
   gh optivem test system --suite <suite-contract-stub> --test <TestMethodName>
   ```

## CT - RED - EXTERNAL DRIVER - REVIEW (STOP)

STOP. Present the Driver implementation to the user and ask for approval. Do NOT continue.

**Review checklist:**

- All changes are confined to files under `external/` — nothing under `shop/` was touched.
- No `"TODO: Driver"` strings remain.
- Tests fail with a runtime error against `<suite-contract-stub>` (still RED — that's expected).

## CT - RED - EXTERNAL DRIVER - COMMIT

1. Mark the tests as disabled with reason `"CT - RED - EXTERNAL DRIVER"` (see [language-equivalents.md](../code/language-equivalents.md)).
2. COMMIT with message `<Ticket> | CT - RED - EXTERNAL DRIVER`.
3. If a GitHub issue number was provided as input, post a comment on the issue summarising the Driver interface changes (new methods added, interfaces updated).

## Anti-patterns

- Editing files under `shop/` — those belong to System Drivers and the AT cycle, not the External System Driver phase.
- Reading external-system source code to figure out behavior — Drivers are written against the *contract* expressed by the contract tests and the published API, not against internal implementation details.
- Expecting the contract tests to pass at the end of this phase — they should still fail. The stub becomes contract-compatible in CT - GREEN - STUBS.
- Skipping the issue comment when an issue number was provided — it's the audit trail of the Driver change.
