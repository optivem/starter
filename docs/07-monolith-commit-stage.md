# Monolith - Commit Stage

For a working example, see the starter templates:

- [Greeter - Java](https://github.com/optivem/greeter-java)
- [Greeter - .NET](https://github.com/optivem/greeter-dotnet)
- [Greeter - TypeScript](https://github.com/optivem/greeter-typescript)

## Verify the Template

1. Open the `monolith` folder. In any file, add a comment.
2. Commit and push (CLI):
   ```bash
   git add -A && git commit -m "Add comment to verify commit stage" && git push
   ```
3. Verify that `commit-stage-monolith` passes (CLI):
   ```bash
   gh run watch --repo <owner>/<repo>
   ```
4. Verify the Docker image artifact exists in Packages (CLI):
   ```bash
   gh api users/<owner>/packages?package_type=container --jq '.[].name'
   ```
   You should see a `monolith` package.

## Change & Push Code

1. Make some change in the source code, commit and push, verify that the Commit Stage passes (CLI):
   ```bash
   git add -A && git commit -m "Test commit stage" && git push
   gh run watch --repo <owner>/<repo>
   ```
2. Make another change which causes a compile time error, commit and push, verify that the Commit Stage fails (CLI):
   ```bash
   git add -A && git commit -m "Introduce compile error" && git push
   gh run watch --repo <owner>/<repo>
   ```
3. Revert the commit and verify the Commit Stage passes (CLI):
   ```bash
   git revert HEAD --no-edit && git push
   gh run watch --repo <owner>/<repo>
   ```

## Checklist

1. `commit-stage-monolith` triggers automatically on push
2. Workflow passes with valid code
3. Workflow fails with broken code
4. Docker image artifact is published to Packages
