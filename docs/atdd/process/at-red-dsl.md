# AT - RED - DSL

## Purpose

Replace the "TODO: DSL" prototypes from AT - RED - TEST with real DSL logic, and lock in which Driver interfaces (external and/or system) need to change as a consequence. Tests stay red — they will only go green once Drivers and the system implementation catch up.

## What it produces

- Commit `<Ticket> | AT - RED - DSL` containing real DSL implementations, any Driver interface changes, and Driver "TODO: Driver" prototypes for any Driver interface that changed.
- Flag set: `External System Driver Interface Changed = yes|no`.
- Flag set: `System Driver Interface Changed = yes|no`.
- Tests in state: change-driven scenarios disabled with reason `"AT - RED - DSL"`; legacy-coverage scenarios still enabled and passing.

## Conventions

- Suite selection (`<acceptance-api>` / `<acceptance-ui>`) and commit-message format: see [at-cycle-conventions.md](at-cycle-conventions.md).
- `@Disabled` / skip syntax and "TODO: Driver" prototype syntax per language: see [language-equivalents.md](../code/language-equivalents.md).
- Definition of an "interface change" (DSL Interface, External System Driver, System Driver): see [glossary.md](glossary.md).
- Commit confirmation gate: see [shared-commit-confirmation.md](shared-commit-confirmation.md).
- STOP semantics at REVIEW: see [shared-phase-progression.md](shared-phase-progression.md).
- DSL layout context: see [dsl-core.md](../architecture/dsl-core.md).

## Example

Before — DSL prototype committed in AT - RED - TEST:

```java
@Override
public ThenSuccess register() {
    throw new UnsupportedOperationException("TODO: DSL");
}
```

After — real DSL logic delegating to the Driver port:

```java
@Override
public ThenSuccess register() {
    var request = new RegisterCustomerRequest(email, name);
    var response = customerDriver.register(request);
    return new ThenSuccess(response);
}
```

If `customerDriver.register(...)` is a new method on the System Customer Driver port, the System Driver interface has changed — set the flag accordingly and add a Driver "TODO: Driver" prototype during COMMIT.

## AT - RED - DSL - WRITE

1. Enable the tests marked disabled with reason `"AT - RED - TEST"`.
2. Implement the DSL for real — replace each "TODO: DSL" prototype with actual logic.
3. Update the Driver interfaces as needed to support the new DSL behavior.
4. Check whether any interface changes (see [glossary.md](glossary.md)) affect external-system Drivers. Set the flag: **External System Driver Interface Changed = yes/no**.
5. Check whether any interface changes affect system Drivers. Set the flag: **System Driver Interface Changed = yes/no**.

## AT - RED - DSL - REVIEW (STOP)

STOP. Present the DSL implementation, Driver interface changes, and both flags to the user and ask for approval. Do NOT continue.

**Review checklist:**
- "TODO: DSL" prototypes are gone — every change-driven DSL method has real logic.
- Driver interface changes are minimal: only what the new DSL actually calls.
- Both flags reflect reality: an external-driver port change means `External System Driver Interface Changed = yes`; a system-driver port change means `System Driver Interface Changed = yes`.
- No system implementation, no test edits, no Driver bodies — only DSL code, Driver interfaces, and flag values.

## AT - RED - DSL - COMMIT

1. **If any Driver interface changed** (either flag is `yes`):
   a. Implement Driver **prototypes** for the new/changed Driver methods — throw a `"TODO: Driver"` not-implemented exception in each (see [language-equivalents.md](../code/language-equivalents.md)).
2. Run the tests and verify they fail with a runtime error:
   ```bash
   gh optivem test system --suite <acceptance-api> --test <TestMethodName>
   gh optivem test system --suite <acceptance-ui> --test <TestMethodName>
   ```
3. Mark the tests as disabled with reason `"AT - RED - DSL"` (see [language-equivalents.md](../code/language-equivalents.md)).
4. Ensure that no test files are (accidentally) in the list of changed files.
5. COMMIT with message `<Ticket> | AT - RED - DSL`.

## Anti-patterns

- **Implementing Driver bodies in this phase.** Drivers are prototyped here (`"TODO: Driver"`); real Driver code belongs to CT - RED - EXTERNAL DRIVER and/or AT - RED - SYSTEM DRIVER.
- **Forgetting to set both flags.** Both `External System Driver Interface Changed` and `System Driver Interface Changed` must be set explicitly — an unset flag is a bug. They gate downstream phases.
- **Leaving "TODO: DSL" behind.** If any DSL method still throws `"TODO: DSL"` after this phase, the phase is not done.
- **Touching test files.** Re-enabling tests at WRITE and disabling them again at COMMIT is the only test-file activity here. Anything else (changing assertions, adding scenarios) means you're in the wrong phase.
