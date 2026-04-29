# CT - RED - DSL

## CT - RED - DSL - WRITE

1. Enable the tests marked disabled with reason `"CT - RED - TEST"`.
2. Implement the DSL for real — replace the "TODO: DSL" stub with actual logic.
3. Update the Driver interfaces as needed.
4. Check whether any interface changes affect files under an `external/` package. Set a flag: **External System Driver Interface Changed = yes/no**. (No recursive triggering — this flag is used only for the CT cycle's own skip logic.)

## CT - RED - DSL - REVIEW (STOP)

STOP. Present the DSL implementation, Driver interface changes, and the flag to the user and ask for approval. Do NOT continue.

## CT - RED - DSL - COMMIT

1. Implement the Drivers by throwing a "TODO: Driver" not-implemented exception (see `language-equivalents.md`).
2. Run the tests and verify they fail with a runtime error:
   ```
   gh optivem test system --suite <suite-contract-stub> --test <TestMethodName>
   ```
3. Mark the tests as disabled with reason `"CT - RED - DSL"` (see `language-equivalents.md` for syntax).
4. COMMIT with message `<Scenario> | CT - RED - DSL`.
5. If a GitHub issue number was provided as input, post a comment on the issue summarising the DSL interface changes made (new methods added, interfaces updated).
6. Automatically proceed to CT - RED - EXTERNAL DRIVER - WRITE (STOP).
