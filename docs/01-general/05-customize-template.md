# Customize Template

*Skip this page if your chosen language already matches the template you applied.*

## Monolith Language

1. Open the `monolith` folder.
2. **If your target language is Java, .NET, or TypeScript:**
   - Find the target language template in the starter repo.
   - Delete the `monolith` folder in your repo.
   - Copy from the target template: `monolith` folder, `system-test/docker-compose.yml` (overwrite), `.github/workflows/commit-stage-monolith.yml` (overwrite).
   - In `system-test/docker-compose.yml`, replace `optivem/starter` with `<your_repo_owner>/<your_repo_name>` for the image field.
3. **If your target language is something else:**
   - Rewrite the `monolith` folder in your target language (or use an LLM to assist).
   - Update the `monolith` README.md with build and run instructions.
   - Build and run locally. Note the port (e.g. 2500).
   - In `system-test/docker-compose.yml`, set ports to `8080:YOUR_PORT`.
   - In `.github/workflows/commit-stage-monolith.yml`, replace the steps between 'Checkout Repository' and 'Publish Docker Image' with your language's setup and build commands.
4. Commit and push (CLI):
   ```bash
   git add -A && git commit -m "Customize monolith language" && git push
   ```
5. Verify that the commit stage and acceptance stage workflows pass (CLI):
   ```bash
   gh run watch --repo <owner>/<repo>
   ```

## System Test Language

*Skip this section if your developers and QA automation engineers use the same language.*

1. Open the `system-test` folder.
2. **If your target language is Java, .NET, or TypeScript:**
   - Find the target template in the starter repo.
   - Delete everything in `system-test` **except** `docker-compose.yml`.
   - Copy from the target template: `system-test` folder (except `docker-compose.yml`), `.github/workflows/acceptance-stage.yml`.
3. **If your target language is something else:**
   - Rewrite the `system-test` folder in your target language. Check Playwright language support; switch to Selenium if needed.
   - Update the `system-test` README.md with instructions to run tests.
   - In `.github/workflows/acceptance-stage.yml`, replace the steps after 'Deploy System' with your language setup and E2E test commands.
4. Commit and push (CLI):
   ```bash
   git add -A && git commit -m "Customize system test language" && git push
   ```
5. Trigger `acceptance-stage` and verify it passes (CLI):
   ```bash
   gh workflow run acceptance-stage.yml --repo <owner>/<repo>
   gh run watch --repo <owner>/<repo>
   ```

> **Why different languages?** It is common for development teams and QA teams to use different languages.

## Checklist

1. Monolith language matches your target language
2. System test language matches your target language
3. Commit stage and acceptance stage workflows pass
