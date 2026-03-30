"""GitHub issue tracking for verification progress."""

from __future__ import annotations

from scaffold.log import log, ok, warn
from scaffold.shell import run


class IssueTracker:
    """Creates and updates a GitHub issue to track verification progress."""

    def __init__(self, repo: str, scenario_name: str, steps: list[str], dry_run: bool = False) -> None:
        self.repo = repo
        self.scenario_name = scenario_name
        self.steps = steps
        self.completed: set[int] = set()
        self.issues_found: list[str] = []
        self.fixes_applied: list[str] = []
        self.issue_number: int | None = None
        self.dry_run = dry_run

    def create(self) -> None:
        """Create the tracking issue on the repo."""
        body = self._build_body()
        if self.dry_run:
            log(f"[DRY RUN] Would create tracking issue: Setup: {self.scenario_name}")
            return

        result = run(
            f'gh issue create --repo {self.repo} --title "Setup: {self.scenario_name}" --body "{body}"',
            capture=True,
        )
        # Parse issue URL to get number
        url = result.stdout.strip()
        if url:
            self.issue_number = int(url.rstrip("/").rsplit("/", 1)[-1])
            ok(f"Created tracking issue #{self.issue_number}")
        else:
            warn("Could not parse issue number from gh output")

    def complete_step(self, index: int) -> None:
        """Mark a step as completed and update the issue."""
        self.completed.add(index)
        self._update()

    def add_issue(self, step_name: str, message: str) -> None:
        """Record an issue found during verification."""
        self.issues_found.append(f"[{step_name}] {message}")
        self._update()

    def add_fix(self, step_name: str, message: str) -> None:
        """Record a fix applied during verification."""
        self.fixes_applied.append(f"[{step_name}] {message}")
        self._update()

    def fail_step(self, step_name: str, error: str) -> None:
        """Comment on the issue about a step failure."""
        if self.dry_run or self.issue_number is None:
            return
        body = f"❌ {step_name} failed: {error}"
        run(
            f'gh issue comment {self.issue_number} --repo {self.repo} --body "{body}"',
            check=False,
        )

    def close(self, success: bool) -> None:
        """Close the issue if all steps passed."""
        if self.dry_run or self.issue_number is None:
            return
        if success:
            run(
                f'gh issue close {self.issue_number} --repo {self.repo} --comment "All steps passed."',
                check=False,
            )
            ok(f"Closed tracking issue #{self.issue_number}")

    def _update(self) -> None:
        """Update the issue body with current progress."""
        if self.dry_run or self.issue_number is None:
            return
        body = self._build_body()
        run(
            f'gh issue edit {self.issue_number} --repo {self.repo} --body "{body}"',
            check=False,
        )

    def _build_body(self) -> str:
        """Build the issue body markdown."""
        lines = ["## Setup Progress", ""]
        for i, step in enumerate(self.steps):
            check = "x" if i in self.completed else " "
            lines.append(f"- [{check}] {step}")

        lines.extend(["", "## Issues Found", ""])
        if self.issues_found:
            for j, issue in enumerate(self.issues_found, 1):
                lines.append(f"{j}. {issue}")
        else:
            lines.append("_None yet_")

        lines.extend(["", "## Fixes Applied", ""])
        if self.fixes_applied:
            for j, fix in enumerate(self.fixes_applied, 1):
                lines.append(f"{j}. {fix}")
        else:
            lines.append("_None yet_")

        return "\\n".join(lines)
