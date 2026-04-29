# AT - GREEN - SYSTEM

## AT - GREEN - SYSTEM - WRITE (STOP)

**Rules for this phase:**
- Backend and frontend are implemented in a single COMMIT — the agent has full-stack access and there is no per-layer commit split.
- When fixing backend or frontend code, change only the system implementation — never tests, DSL, or drivers.

1. Enable the tests marked disabled with reason `"AT - RED - SYSTEM DRIVER"`. (This is the only "remove disabled annotation" step in this phase — there is no second one at the end.)
2. Implement the backend:
   a. Implement the backend changes.
   b. Run acceptance tests for the API channel:
      ```
      gh optivem test system --rebuild --suite <acceptance-api> --test <TestMethodName>
      ```
   c. If tests fail, fix the backend until the tests pass.
   d. If you have challenges making the tests pass, ask the user.
3. Implement the frontend:
   a. Implement the frontend changes.
   b. Run acceptance tests for the UI channel:
      ```
      gh optivem test system --rebuild --suite <acceptance-ui> --test <TestMethodName>
      ```
   c. If tests fail, fix the frontend until the tests pass.
   d. If you have challenges making the tests pass, ask the user.
4. By now, all acceptance tests should be passing.
5. STOP. Present the implementation to the user and ask for approval. Do NOT continue.

## AT - GREEN - SYSTEM - COMMIT

1. COMMIT all changes (backend, frontend, and the test re-enabling from AT - GREEN - SYSTEM - WRITE step 1) with message `<Ticket> | AT - GREEN - SYSTEM`.
2. If a GitHub issue was provided as input, tick the checkbox for each acceptance criterion completed by this ticket.
3. If all acceptance criteria in the issue are now ticked, and the issue belongs to a GitHub project, move the issue to the **DONE** status in that project.


# TODO: VJ: Need to add insutrctions regarding handling legacy code...
