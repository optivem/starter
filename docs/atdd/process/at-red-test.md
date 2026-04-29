# AT - RED - TEST

## AT - RED - TEST - WRITE (STOP)

**Goal:** every test in the scenario set compiles and fails only with runtime failure, then is marked as known-failing for the next phase.

The unit of work is the **ticket** — all scenarios for the ticket are written together as a batch. There is no per-scenario inner loop in this phase.

1. Write the acceptance tests for **all scenarios in the ticket**, following these rules:
   - Write acceptance tests only — do not implement anything.
   - Each Gherkin scenario maps directly to one test method — one-to-one, no interpretation needed. All scenarios are written as real test methods; no `// TODO:` placeholders.
   - Only specify the minimum data needed — inputs directly relevant to what is being tested, and assertions directly relevant to the expected outcome. Do not add extra fields, extra assertions, or noise. If a field is not relevant to the scenario being tested, omit it entirely and let the DSL use its default value.
   - If the DSL needs to be extended with new methods, call them directly in the test as if they exist — do not add them to the DSL interface yet. Compile errors are expected and intentional.
   - **Scenario ordering within the test class:**
     1. Legacy Coverage scenarios (from the `## Legacy Coverage` section of the ticket, if any)
     2. New feature scenarios that use only existing DSL
     3. New feature scenarios that need new DSL
   - After writing each test, verify it matches the acceptance criteria exactly — Given maps to Given, When maps to When, Then maps to Then. Every precondition stated in the scenario must appear in the test. If anything is unclear, ask before proceeding.
2. STOP. Present the tests to the user for review (the user may revise DSL usage). Do NOT continue.

## AT - RED - TEST - COMMIT

1. **Attempt to compile** the tests.
2. If compilation fails (i.e. the tests reference DSL methods that do not yet exist):
   a. Change the DSL interfaces to add the missing methods.
   b. Implement DSL **prototypes** for the new methods — throw a `"TODO: DSL"` not-implemented exception in each (see `language-equivalents.md`). Do not implement DSL behavior here.
   c. **STOP.** Present the DSL changes and prototype implementations to the user for approval. Do NOT continue until approved.
3. Run the tests and verify they fail with a **runtime** error (not a compile error):
   ```
   gh optivem test system --suite <acceptance-api> --test <TestMethodName>
   gh optivem test system --suite <acceptance-ui> --test <TestMethodName>
   ```
4. Mark the tests as disabled with reason `"AT - RED - TEST"` (see `language-equivalents.md` for syntax). Disable **only the change-driven scenarios** (categories 2 and 3 from the WRITE-phase ordering). Legacy-coverage scenarios (category 1) are test-last — they should pass on first run and must NOT be disabled. If a legacy-coverage test fails on first run, STOP and ask the user before continuing — that's a real bug, not an expected RED.
5. COMMIT with message `<Ticket> | AT - RED - TEST`.
6. STOP. Do not proceed further. Phase progression is controlled by the orchestrator, not by this agent.
