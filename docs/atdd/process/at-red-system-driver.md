# AT - RED - SYSTEM DRIVER

## Purpose

Replace the System-Driver "TODO: Driver" prototypes from AT - RED - DSL with real Driver logic. This phase touches **System Drivers only** (under `shop/`); external-system Drivers are handled by the Contract Test sub-process. Tests stay red — they only go green once the system implementation lands in AT - GREEN - SYSTEM.

## What it produces

- Commit `<Ticket> | AT - RED - SYSTEM DRIVER` containing real System Driver implementations under `shop/`.
- Tests in state: change-driven scenarios disabled with reason `"AT - RED - SYSTEM DRIVER"`; legacy-coverage scenarios still enabled and passing.

## Conventions

- File scope: only files under `driver-port/` and `driver-adapter/` paths under `shop/` (e.g. `shop/api`, `shop/ui`). Do NOT touch `external/` — that is the Contract Test sub-process.
- Do NOT read or search backend/frontend source code. Model new Driver methods on existing Driver methods in the same file.
- Suite selection (`<acceptance-api>` / `<acceptance-ui>`) and commit-message format: see [at-cycle-conventions.md](at-cycle-conventions.md).
- `@Disabled` / skip syntax per language: see [language-equivalents.md](../code/language-equivalents.md).
- Definition of System Driver vs External System Driver: see [glossary.md](glossary.md).
- Commit confirmation gate: see [shared-commit-confirmation.md](shared-commit-confirmation.md).
- STOP semantics at REVIEW: see [shared-phase-progression.md](shared-phase-progression.md).

## Example

Before — System Driver prototype committed in AT - RED - DSL:

```java
@Override
public RegisterCustomerResponse register(RegisterCustomerRequest request) {
    throw new UnsupportedOperationException("TODO: Driver");
}
```

After — real System Driver wiring the request through the system's HTTP/UI surface (modelled on the sibling `update(...)` method already in this file):

```java
@Override
public RegisterCustomerResponse register(RegisterCustomerRequest request) {
    var response = httpClient.post("/customers", request);
    return response.as(RegisterCustomerResponse.class);
}
```

## AT - RED - SYSTEM DRIVER - WRITE

1. Enable the tests marked disabled with reason `"AT - RED - DSL"`.
2. Implement the System Drivers — replace each "TODO: Driver" prototype with actual logic. Stay within `driver-port/` and `driver-adapter/` under `shop/`. Model new methods on existing Driver methods in the same file.
3. Run the tests and verify they fail with a runtime error:
   ```bash
   gh optivem test system --suite <acceptance-api> --test <TestMethodName>
   gh optivem test system --suite <acceptance-ui> --test <TestMethodName>
   ```

## AT - RED - SYSTEM DRIVER - REVIEW (STOP)

STOP. Present the Driver implementation to the user and ask for approval. Do NOT continue.

**Review checklist:**
- All edits live under `driver-port/` and `driver-adapter/` paths under `shop/`. Nothing under `external/`.
- "TODO: Driver" is gone for every System Driver method affected by this ticket.
- New methods follow the shape of existing methods in the same file — no novel patterns invented from backend/frontend source.
- No test, DSL, system, or external-driver edits.

## AT - RED - SYSTEM DRIVER - COMMIT

1. Mark the tests as disabled with reason `"AT - RED - SYSTEM DRIVER"` (see [language-equivalents.md](../code/language-equivalents.md)).
2. Ensure no test files are (accidentally) in the list of changed files.
3. COMMIT with message `<Ticket> | AT - RED - SYSTEM DRIVER`.

## Anti-patterns

- **Editing files under `external/`.** External-system Drivers belong to the Contract Test sub-process (CT - RED - EXTERNAL DRIVER). If a change is needed there, exit this phase and route through CT.
- **Reading backend/frontend source to figure out behaviour.** The Driver speaks to the system's existing surface; behaviour is modelled on sibling Driver methods, not derived from production code. Reading production code in this phase risks coupling test infrastructure to implementation details.
- **Modifying tests or DSL.** Tests are disabled/enabled here, nothing more; DSL is frozen. If the Driver cannot be implemented without DSL or test changes, the previous phase was incomplete — go back, do not patch around it.
- **Leaving "TODO: Driver" behind.** Any remaining System-Driver prototype means the phase is not done.
