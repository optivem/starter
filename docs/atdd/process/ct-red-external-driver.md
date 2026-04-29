# CT - RED - EXTERNAL DRIVER

## CT - RED - EXTERNAL DRIVER - WRITE

1. Enable the tests marked disabled with reason `"CT - RED - DSL"`.
2. Implement the Drivers — replace the "TODO: Driver" stub with actual logic.
   - Only look at files in the `driver-adapter` and `driver-port` directories under `external/`.
3. Run the tests and verify they fail with a runtime error.

## CT - RED - EXTERNAL DRIVER - REVIEW (STOP)

STOP. Present the Driver implementation to the user and ask for approval. Do NOT continue.

## CT - RED - EXTERNAL DRIVER - COMMIT

1. Mark the tests as disabled with reason `"CT - RED - EXTERNAL DRIVER"` (see `language-equivalents.md` for syntax).
2. COMMIT with message `<Scenario> | CT - RED - EXTERNAL DRIVER`.
3. If a GitHub issue number was provided as input, post a comment on the issue summarising the Driver interface changes made (new methods added, interfaces updated).
4. STOP. Do not proceed further. Phase progression is controlled by the orchestrator, not by this agent.
