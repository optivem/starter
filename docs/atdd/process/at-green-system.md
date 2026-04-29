# AT - GREEN - SYSTEM

## AT - GREEN - SYSTEM - WRITE (STOP)

**Notes / Assumptions:**
- The agent has access to both backend and frontend code and works across the full stack — no silos like a human team. A single COMMIT therefore covers all implementation changes; the workflow does not split backend and frontend into separate commits.
- When fixing backend or frontend code, do NOT change tests, DSL, or drivers — change only the system implementation.

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
