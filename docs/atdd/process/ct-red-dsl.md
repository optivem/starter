# CT - RED - DSL

## Purpose

Replace the `"TODO: DSL"` prototypes left behind by CT - RED - TEST with real DSL logic for the external system, and surface whether the work changes any External System Driver interfaces.

## What it produces

- Commit `<Ticket> | CT - RED - DSL` containing the real DSL implementation, any updated Driver interfaces, and `"TODO: Driver"` prototypes for new/changed Driver methods
- Flag set: `external_system_driver_interface_changed = yes|no`
- Tests in state: contract tests disabled with reason `"CT - RED - DSL"`
- GitHub issue comment summarising DSL interface changes (if an issue number was provided as input)

## Conventions

- Suite selection (real vs stub): see [ct-cycle-conventions.md](ct-cycle-conventions.md). This phase exercises the stub side only.
- Commit message format: see [ct-cycle-conventions.md](ct-cycle-conventions.md).
- Commit gate ("Can I commit?"): see [shared-commit-confirmation.md](shared-commit-confirmation.md).
- Phase progression and STOP semantics: see [shared-phase-progression.md](shared-phase-progression.md).
- `"TODO: Driver"` exception string and `@Disabled` syntax per language: see [language-equivalents.md](../code/language-equivalents.md).
- Definitions of DSL Interface and External System Driver: see [glossary.md](glossary.md).

## Example

Replace the `"TODO: DSL"` prototype with real DSL logic. Driver methods stay as `"TODO: Driver"` prototypes — they get implemented in CT - RED - EXTERNAL DRIVER.

```diff
 public PromotionResult promotion() {
-    throw new UnsupportedOperationException("TODO: DSL");
+    PromotionResponse response = erpDriver.getPromotion();
+    return new PromotionResult(response.isActive(), response.getDiscount());
 }
```

## CT - RED - DSL - WRITE

1. Enable the tests marked disabled with reason `"CT - RED - TEST"`.
2. Implement the DSL for real — replace each `"TODO: DSL"` prototype with actual logic.
3. Update the External System Driver interfaces as needed.
4. Determine whether any interface changes affect files under an `external/` package and set `external_system_driver_interface_changed = yes|no`.

## CT - RED - DSL - REVIEW (STOP)

STOP. Present the DSL implementation, Driver interface changes, and the `external_system_driver_interface_changed` flag to the user and ask for approval. Do NOT continue.

**Review checklist:**

- DSL methods now contain real logic — no `"TODO: DSL"` strings remain.
- Driver interface changes (if any) are confined to files under `external/`.
- The `external_system_driver_interface_changed` flag accurately reflects whether any Driver interface changed.

## CT - RED - DSL - COMMIT

1. For any new or changed External System Driver methods, add `"TODO: Driver"` prototypes that throw a not-implemented exception (see [language-equivalents.md](../code/language-equivalents.md)).
2. Run the contract tests and verify they fail with a runtime error:
   ```bash
   gh optivem test system --suite <suite-contract-stub> --test <TestMethodName>
   ```
3. Mark the tests as disabled with reason `"CT - RED - DSL"` (see [language-equivalents.md](../code/language-equivalents.md)).
4. COMMIT with message `<Ticket> | CT - RED - DSL`.
5. If a GitHub issue number was provided as input, post a comment on the issue summarising the DSL interface changes (new methods added, interfaces updated).

## Anti-patterns

- Implementing External System Drivers here — Driver bodies belong in CT - RED - EXTERNAL DRIVER. Only Driver *prototypes* (`"TODO: Driver"`) are added in this phase.
- Forgetting to post the GitHub-issue comment when an issue number was provided — it's the audit trail of the DSL interface change.
- Leaving `"TODO: DSL"` strings behind in the committed code — every prototype must be replaced with real logic.
- Editing files outside `external/` to "fix" failing contract tests — the contract is between the system and the external boundary, not internal code.
