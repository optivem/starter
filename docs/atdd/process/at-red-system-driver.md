# AT - RED - SYSTEM DRIVER

## AT - RED - SYSTEM DRIVER - WRITE (STOP)

**Notes:**
- Implement **System Drivers only** — only look at files in the `driver-adapter` and `driver-port` directories under `shop/` (e.g. `shop/api`, `shop/ui`).
- Do NOT implement drivers under `external/` — those are handled by the Contract Test sub-process.
- Do NOT read or search backend/frontend source code. Model new methods on existing driver methods in the same file.

1. Enable the tests marked disabled with reason `"AT - RED - DSL"`.
2. Implement the System Drivers — replace each "TODO: Driver" prototype with actual logic.
3. Run the tests and verify they fail with a runtime error:
   ```
   gh optivem test system --suite <acceptance-api> --test <TestMethodName>
   gh optivem test system --suite <acceptance-ui> --test <TestMethodName>
   ```
4. STOP. Present the Driver implementation to the user and ask for approval. Do NOT continue.

## AT - RED - SYSTEM DRIVER - COMMIT

1. Mark the tests as disabled with reason `"AT - RED - SYSTEM DRIVER"` (see `language-equivalents.md` for syntax).
2. Ensure no test files (accidentally) are in the list of changed files.
3. COMMIT with message `<Ticket> | AT - RED - SYSTEM DRIVER`.
4. STOP. Do not proceed further. Phase progression is controlled by the orchestrator, not by this agent.
