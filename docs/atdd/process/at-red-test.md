# AT - RED - TEST

## Purpose

Turn the ticket's change-driven AC into compiling, runtime-failing acceptance tests in a single batch. This is the entry point of the AT Cycle: it locks in test intent before any DSL, Driver, or system code is written.

## What it produces

- Commit `<Ticket> | AT - RED - TEST` containing the new test class(es) and any DSL interface additions plus DSL "TODO: DSL" prototypes if compilation required them.
- Tests in state: change-driven scenarios disabled with reason `"AT - RED - TEST"`; legacy-coverage scenarios enabled and passing.

## Conventions

- Unit of work: the **ticket**. All scenarios for the ticket are written together as a batch — there is no per-scenario inner loop.
- Suite selection (`<acceptance-api>` / `<acceptance-ui>`) and commit-message format: see [at-cycle-conventions.md](at-cycle-conventions.md).
- `@Disabled` / skip syntax per language: see [language-equivalents.md](../code/language-equivalents.md).
- "TODO: DSL" prototype syntax per language: see [language-equivalents.md](../code/language-equivalents.md).
- Commit confirmation gate: see [shared-commit-confirmation.md](shared-commit-confirmation.md).
- STOP semantics at REVIEW: see [shared-phase-progression.md](shared-phase-progression.md).
- Test layout context: see [test.md](../architecture/test.md) and [dsl-core.md](../architecture/dsl-core.md).

## Example

A scenario that needs a not-yet-existing DSL method is written as if the method already exists. Compile errors are intentional and resolved at COMMIT by adding interface methods plus prototypes.

```java
@Test
@Disabled("AT - RED - TEST")
void registerCustomer_succeeds() {
    customer().withEmail("a@b.test")  // existing DSL
              .register()              // does not exist yet — compile error here
              .shouldSucceed();
}
```

The matching DSL prototype added during COMMIT (Java shown — see [language-equivalents.md](../code/language-equivalents.md) for other languages):

```java
@Override
public ThenSuccess register() {
    throw new UnsupportedOperationException("TODO: DSL");
}
```

## AT - RED - TEST - WRITE

1. Write the acceptance tests for **all scenarios in the ticket**, following these rules:
   - Write acceptance tests only — do not implement anything.
   - Each Gherkin scenario maps directly to one test method — one-to-one, no interpretation. All scenarios are real test methods; no `// TODO:` placeholders.
   - Specify only the minimum data needed — inputs directly relevant to what is being tested, and assertions directly relevant to the expected outcome. Omit any field not relevant to the scenario and let the DSL use its default.
   - If the DSL needs new methods, call them directly in the test as if they exist — do not add them to the DSL interface yet. Compile errors are expected and intentional.
   - **Scenario ordering within the test class:**
     1. Legacy Coverage scenarios (from the `## Legacy Coverage` section of the ticket, if any)
     2. New feature scenarios that use only existing DSL
     3. New feature scenarios that need new DSL
   - After writing each test, verify it matches the AC exactly — Given maps to Given, When maps to When, Then maps to Then. Every precondition stated in the scenario must appear in the test. If anything is unclear, ask before proceeding.

## AT - RED - TEST - REVIEW (STOP)

STOP. Present the tests to the user for review (the user may revise DSL usage). Do NOT continue.

**Review checklist:**
- One test method per scenario; the mapping is one-to-one.
- Test ordering matches the rule above (legacy-coverage first, then existing-DSL, then needs-new-DSL).
- No noise: no extra fields, no extra assertions, no speculative setup.
- New DSL calls (if any) are used directly without being declared in the interface yet.

## AT - RED - TEST - COMMIT

1. **Attempt to compile** the tests.
2. If compilation fails (the tests reference DSL methods that do not yet exist):
   a. Change the DSL interfaces to add the missing methods.
   b. Implement DSL **prototypes** for the new methods — throw a `"TODO: DSL"` not-implemented exception in each (see [language-equivalents.md](../code/language-equivalents.md)). Do not implement DSL behavior here.
   c. **STOP.** Present the DSL changes and prototype implementations to the user for approval. Do NOT continue until approved.
3. Run the tests and verify they fail with a **runtime** error (not a compile error):
   ```bash
   gh optivem test system --suite <acceptance-api> --test <TestMethodName>
   gh optivem test system --suite <acceptance-ui> --test <TestMethodName>
   ```
4. Mark the tests as disabled with reason `"AT - RED - TEST"` (see [language-equivalents.md](../code/language-equivalents.md)). Disable **only the change-driven scenarios** (categories 2 and 3 in the ordering above). Legacy-coverage scenarios (category 1) are test-last — they should pass on first run and must NOT be disabled. If a legacy-coverage test fails on first run, STOP and ask the user — that is a real bug, not an expected RED.
5. COMMIT with message `<Ticket> | AT - RED - TEST`.

## Anti-patterns

- **Implementing too much in WRITE.** WRITE produces test code only. DSL prototypes are added at COMMIT *after* the compile attempt fails — not preemptively while writing tests.
- **Disabling a legacy-coverage scenario.** Legacy coverage is test-last; it must pass on first run. A failing legacy-coverage test signals a real bug — surface it, do not paper over it with `@Disabled`.
- **Adding "noise" assertions or fields.** Anything not directly tied to Given/When/Then for the scenario is noise. Trust the DSL defaults.
- **Hand-coding DSL bodies in this phase.** Real DSL logic belongs to AT - RED - DSL. Here, prototypes throw `"TODO: DSL"` and nothing more.
