# Multitier - Setup

## Starter Template

This repository (`starter`) is the template. It contains workflows, system code, and system tests for Java, .NET, and TypeScript across monolith and multitier architectures.

## Usage

1. Copy the following from the starter template into your repository (CLI).

   Set your language variables:
   ```bash
   STARTER_PATH="<path-to-starter-repo>"
   BACKEND_LANG="<your-backend-language>"   # java, dotnet, or typescript
   FRONTEND_LANG="<your-frontend-language>" # react
   TEST_LANG="<your-test-language>"         # java, dotnet, or typescript (same as BACKEND_LANG unless QA uses a different language)
   ```

   Copy workflows, system code, system tests, and VERSION file — **only for your chosen languages**:
   ```bash
   mkdir -p .github/workflows system/multitier system-test

   # Commit stage workflows (one per component)
   cp "$STARTER_PATH/.github/workflows/multitier-backend-${BACKEND_LANG}-commit-stage.yml" .github/workflows/
   cp "$STARTER_PATH/.github/workflows/multitier-frontend-${FRONTEND_LANG}-commit-stage.yml" .github/workflows/

   # Acceptance, QA, QA signoff, and production stage workflows (based on test language)
   cp "$STARTER_PATH/.github/workflows/multitier-system-${TEST_LANG}-acceptance-stage.yml" .github/workflows/
   cp "$STARTER_PATH/.github/workflows/multitier-system-${TEST_LANG}-qa-stage.yml" .github/workflows/
   cp "$STARTER_PATH/.github/workflows/multitier-system-${TEST_LANG}-qa-signoff.yml" .github/workflows/
   cp "$STARTER_PATH/.github/workflows/multitier-system-${TEST_LANG}-prod-stage.yml" .github/workflows/

   # Verify workflow (only when backend and test languages are the same)
   if [ "$BACKEND_LANG" = "$TEST_LANG" ]; then
     cp "$STARTER_PATH/.github/workflows/multitier-${BACKEND_LANG}-verify.yml" .github/workflows/
   fi

   # System code (backend + frontend)
   cp -r "$STARTER_PATH/system/multitier/backend-${BACKEND_LANG}" system/multitier/
   cp -r "$STARTER_PATH/system/multitier/frontend-${FRONTEND_LANG}" system/multitier/

   # System tests
   cp -r "$STARTER_PATH/system-test/${TEST_LANG}" system-test/
   cp -f "$STARTER_PATH/VERSION" . 2>/dev/null || true
   ```

   - Also copy the top part of `README.md` (the status badges section) from the template — update the badge URLs to use your languages (e.g. `multitier-backend-java-commit-stage`, `multitier-frontend-react-commit-stage`).
   - *If your chosen language is not on the list, no worries, just choose any of the templates because the Pipeline Architecture is the same — you can do language replacement afterwards.*
2. Replace `optivem/starter` with `<your_repo_owner>/<your_repo_name>` in the whole project (CLI).
   > **macOS note:** The `sed -i` commands below use Linux syntax. On macOS, use `sed -i ''` instead of `sed -i` (add an empty string argument after `-i`).
   ```bash
   grep -rl "optivem/starter" . --include="*.yml" --include="*.yaml" --include="*.md" --include="*.gradle" --include="*.gradle.kts" --include="*.csproj" --include="*.sln" --include="*.cshtml" --include="*.json" --include="Dockerfile" | xargs sed -i 's|optivem/starter|<owner>/<repo>|g'
   ```
   Also replace the underscore variant (used by SonarCloud config) and standalone `optivem` org reference (CLI):
   ```bash
   grep -rl "optivem_starter" . --include="*.yml" --include="*.yaml" --include="*.gradle" --include="*.gradle.kts" --include="*.csproj" --include="*.sln" | xargs sed -i 's|optivem_starter|<owner>_<repo>|g'
   grep -rl "sonar.organization.*optivem\|/o:\"optivem\"\|/o:.*optivem" . --include="*.yml" --include="*.yaml" --include="*.gradle" --include="*.gradle.kts" --include="*.csproj" | xargs sed -i -e "s|sonar.organization=optivem|sonar.organization=<owner>|g" -e "s|'sonar.organization', 'optivem'|'sonar.organization', '<owner>'|g" -e 's|/o:"optivem"|/o:"<owner>"|g' -e "s|/o:optivem|/o:<owner>|g"
   ```
   > **Warning:** After running the SonarCloud org replacement, verify that `optivem/actions` references in `.github/workflows/*.yml` are still intact. If any were changed to `<owner>/actions`, revert them — those must remain as `optivem/actions`.

   This covers `.yml` files (including `docker-compose.yml` and workflow files), `.md` files, `.gradle`/`.gradle.kts` files (including SonarCloud config), and .NET files (`.csproj`, `.sln`, `.cshtml`, `.json`, `Dockerfile`):
   - In the README file, so that the status badges point to your workflows (not the template workflows)
   - In `system-test/docker-compose.yml`, to reference your Docker Image (not the template image)
   - In SonarCloud config (`sonar.projectKey` and `sonar.organization`), so analysis runs under your organization
4. In the Docker Compose file, ensure that everything is lowercase in the image url.
5. Add credentials and variables to your repository (CLI):
   ```bash
   gh variable set DOCKERHUB_USERNAME --body "<your-dockerhub-username>" --repo <owner>/<repo>
   gh secret set DOCKERHUB_TOKEN --body "<your-dockerhub-token>" --repo <owner>/<repo>
   gh variable set SYSTEM_URL --body "http://localhost:8080" --repo <owner>/<repo>
   ```
6. Commit and push (CLI):
   ```bash
   git add -A && git commit -m "Apply pipeline template" && git push
   ```
7. Trigger the commit stage workflows and wait for them to finish (CLI):
   ```bash
   gh workflow run "multitier-backend-${BACKEND_LANG}-commit-stage.yml" --repo <owner>/<repo>
   gh workflow run "multitier-frontend-${FRONTEND_LANG}-commit-stage.yml" --repo <owner>/<repo>
   gh run watch --repo <owner>/<repo>
   ```
8. Trigger the acceptance stage and wait for it to finish (CLI):
   ```bash
   gh workflow run "multitier-system-${TEST_LANG}-acceptance-stage.yml" --repo <owner>/<repo>
   gh run watch --repo <owner>/<repo>
   ```

## Namespace Replacement

1. Find template namespace references:
   - Java: `com.optivem.starter`
   - .NET: `Optivem.Starter`
   - TypeScript: `@optivem/starter-system-test`
   - Also search for any other references like "accelerator" and "Accelerator"
   - For TypeScript, also update `author`, `license`, `description` in `package.json`
   - For .NET, also check `.cshtml` (Razor views) and `Dockerfile` — these contain namespace and project references
   - For TypeScript, also check `Dockerfile` — it may contain project references
2. Replace all references with your corresponding namespace and info.
   - Also update the README title to your system name and language.
3. Commit and push (CLI):
   ```bash
   git add -A && git commit -m "Replace template namespaces" && git push
   ```
4. Verify that the commit stage and acceptance stage workflows still pass (CLI):
   ```bash
   gh run watch --repo <owner>/<repo>
   ```

## Checklist

1. Template has been applied to your repository (only your chosen languages' files)
2. All references to the template repository name have been replaced with your own
3. Namespace customization is complete
4. Root README file contains correct links to GitHub Actions
5. Docker Compose file (in System Test) has the correct image urls
6. Backend and frontend commit stage workflows pass
7. Acceptance stage workflow passes
