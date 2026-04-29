# AT - RED - DSL

## AT - RED - DSL - WRITE (STOP)

1. Enable the tests marked disabled with reason `"AT - RED - TEST"`.
2. Implement the DSL for real — replace each "TODO: DSL" prototype with actual logic.
3. Update the Driver interfaces as needed.
4. Check whether any interface changes (see `glossary.md`) affect external-system drivers. Set a flag: **External System Driver Interface Changed = yes/no**.
5. Check whether any interface changes affect system drivers. Set a flag: **System Driver Interface Changed = yes/no**.
6. STOP. Present the DSL implementation, Driver interface changes, and both flags to the user and ask for approval. Do NOT continue.

## AT - RED - DSL - COMMIT

1. **If any Driver interface changed** (either flag is `yes`):
   a. Implement Driver **prototypes** for the new/changed Driver methods — throw a `"TODO: Driver"` not-implemented exception in each (see `language-equivalents.md`).
2. Run the tests and verify they fail with a runtime error:
   ```
   gh optivem test system --suite <acceptance-api> --test <TestMethodName>
   gh optivem test system --suite <acceptance-ui> --test <TestMethodName>
   ```
3. Mark the tests as disabled with reason `"AT - RED - DSL"` (see `language-equivalents.md` for syntax).
4. Ensure that there are no test files (accidentally) in the list of changed files.
5. COMMIT with message `<Ticket> | AT - RED - DSL`.
6. STOP. Outputs of this phase: the **External System Driver Interface Changed** flag and the **System Driver Interface Changed** flag — both consumed by the orchestrator to decide whether to enter the Contract Test sub-process and/or AT - RED - SYSTEM DRIVER. Phase progression is controlled by the orchestrator, not by this agent.
