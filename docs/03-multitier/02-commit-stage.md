# Multitier - Commit Stage

For a working example, see the [Starter](https://github.com/optivem/starter) template.

## Verify the Commit Stage

The commit stage workflows should have triggered automatically from the push in the [Apply Template](../01-general/04-apply-template.md) step.

1. If they haven't completed yet, wait for them (CLI):
   ```bash
   gh run watch --repo <owner>/<repo>
   ```
2. Verify both commit stage workflows pass (CLI):
   ```bash
   gh run list --repo <owner>/<repo> --limit 5 --json name,status,conclusion
   ```
   You should see `commit-stage-frontend` and `commit-stage-backend` both passing.
3. Verify the Docker image artifacts exist in Packages (CLI):
   ```bash
   gh api users/<owner>/packages?package_type=container --jq '.[].name'
   ```
   You should see `frontend` and `backend` packages.

## Verify SonarCloud Analysis

If you set up SonarCloud during the [Apply Template](../01-general/04-apply-template.md) step, verify the analysis ran:

```bash
SONAR_PROJECT="<project-key-from-build-config>"

curl -s -u "${SONAR_TOKEN}:" \
  "https://sonarcloud.io/api/measures/component?component=${SONAR_PROJECT}&metricKeys=bugs,vulnerabilities,code_smells,coverage,duplicated_lines_density" | jq '.component.measures'
```

You should see metrics for: bugs, vulnerabilities, code smells, coverage, and duplicated lines.

## Checklist

1. `commit-stage-frontend` passes
2. `commit-stage-backend` passes
3. Docker image artifacts are published to Packages (frontend and backend)
4. SonarCloud analysis runs and metrics are visible
