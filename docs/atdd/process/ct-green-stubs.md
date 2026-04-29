# CT - GREEN - STUBS

## CT - GREEN - STUBS - WRITE

1. Enable the tests marked disabled with reason `"CT - RED - EXTERNAL DRIVER"`.
2. Implement the External System Stubs.
3. Run the External System Contract Tests:
   ```
   gh optivem test system --rebuild --suite <suite-contract-stub> --test <TestMethodName>
   ```
4. Verify that the tests pass. If they fail, ask the user. STOP. Do NOT continue.

## CT - GREEN - STUBS - REVIEW (STOP)

STOP. Present the stub implementation to the user and ask for approval. Do NOT continue.

## CT - GREEN - STUBS - COMMIT

1. Remove the disabled annotation (reason `"CT - RED - EXTERNAL DRIVER"`) from the tests.
2. Run the tests and verify they pass.
3. COMMIT with message `<Scenario> | CT - GREEN - STUBS`.
4. STOP. Do not proceed further. Phase progression is controlled by the orchestrator, not by this agent.
