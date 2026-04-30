# CT - RED - TEST

## Purpose

Express the contract between the system and the real external system as executable tests. The contract tests are the *contract*: they must PASS against the real Test Instance and FAIL against the dockerized stub before the cycle is allowed to proceed.

## What it produces

- Commit `<Ticket> | CT - RED - TEST` containing the new contract tests and any DSL prototype additions needed to make them compile
- Tests in state: contract tests disabled with reason `"CT - RED - TEST"`

## Conventions

- Suite selection (real vs stub): see [ct-cycle-conventions.md](ct-cycle-conventions.md).
- Commit message format: see [ct-cycle-conventions.md](ct-cycle-conventions.md).
- Onboarding pre-condition (Driver + Test Instance must exist): see [ct-cycle-conventions.md](ct-cycle-conventions.md).
- Commit gate ("Can I commit?"): see [shared-commit-confirmation.md](shared-commit-confirmation.md).
- Phase progression and STOP semantics: see [shared-phase-progression.md](shared-phase-progression.md).
- `@Disabled` / skip syntax and "TODO: DSL" exception strings per language: see [language-equivalents.md](../code/language-equivalents.md).

## Example

A contract test calling a not-yet-implemented DSL method. Compile errors are expected and intentional in WRITE; the prototype is filled in at COMMIT.

```java
@Test
void promotion_endpoint_returns_default_no_promotion_state() {
    erp.promotion()
        .shouldHaveActive(false)
        .shouldHaveDiscount(1.0);
}
```

## CT - RED - TEST - WRITE

1. Write External System Contract Tests against the existing DSL surface.
   - If new DSL methods are needed, call them directly as if they exist — compile errors are expected.
2. Verify the tests PASS against the Real External System (Test Instance):
   ```bash
   gh optivem test system --suite <suite-contract-real> --test <TestMethodName>
   ```
   If they do not pass, that is a real contract problem — ask the user for support and STOP. Do NOT continue.
3. Verify the tests FAIL against the dockerized Stub External System:
   ```bash
   gh optivem test system --suite <suite-contract-stub> --test <TestMethodName>
   ```
4. Mark the tests as disabled with reason `"CT - RED - TEST"` (see [language-equivalents.md](../code/language-equivalents.md)).

## CT - RED - TEST - REVIEW (STOP)

STOP. Present the contract tests, the real-instance pass output, and the stub fail output to the user and ask for approval. Do NOT continue.

**Review checklist:**

- Each test maps one-to-one to a contract behavior — no extra fields, no extra assertions.
- Tests verifiably PASS against `<suite-contract-real>`.
- Tests verifiably FAIL against `<suite-contract-stub>`.
- Tests are disabled with reason `"CT - RED - TEST"`.

## CT - RED - TEST - COMMIT

1. If there were compile-time errors in WRITE:
   a. Extend the DSL interfaces with the new methods.
   b. Implement the new methods by throwing a `"TODO: DSL"` not-implemented exception (see [language-equivalents.md](../code/language-equivalents.md)).
   c. Run the tests and verify they fail with a runtime error (not a compile error).
2. COMMIT with message `<Ticket> | CT - RED - TEST`.

## Anti-patterns

- Skipping the real-instance verification "because the tests look right" — without `<suite-contract-real>` passing, you have no evidence the contract is real.
- Marking tests disabled before the real-vs-stub verification has run — that hides the contract from review.
- Implementing real DSL behavior here — that belongs in CT - RED - DSL. This phase only adds `"TODO: DSL"` prototypes when needed to make tests compile.
- Adding fields or assertions that are not part of the contract being expressed — keep each test minimal.
