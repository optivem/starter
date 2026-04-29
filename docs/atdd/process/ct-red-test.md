# CT - RED - TEST

## CT - RED - TEST - WRITE (STOP)

1. Write External System Contract Tests.
   - If new DSL methods are needed, call them directly as if they exist — compile errors are expected.
2. Verify that they pass when executed against the Real External System (Test Instance):
   ```
   gh optivem test system --suite <suite-contract-real> --test <TestMethodName>
   ```
   If they don't pass, ask the user for support. STOP. Do NOT continue.
3. Verify that they fail when executed against the Stub External System:
   ```
   gh optivem test system --suite <suite-contract-stub> --test <TestMethodName>
   ```
4. Mark the tests as disabled with reason `"CT - RED - TEST"` (see `language-equivalents.md` for syntax).
5. STOP. Present the contract tests to the user and ask for approval. Do NOT continue.

## CT - RED - TEST - COMMIT

1. If there were compile-time errors in CT - RED - TEST - WRITE:
   a. Extend the DSL interfaces with the new methods.
   b. Implement the new methods by throwing a "TODO: DSL" not-implemented exception (see `language-equivalents.md`).
   c. Run the tests and verify they fail with a runtime error.
2. COMMIT with message `<Scenario> | CT - RED - TEST`.
3. STOP. Do not proceed further. Phase progression is controlled by the orchestrator, not by this agent.
