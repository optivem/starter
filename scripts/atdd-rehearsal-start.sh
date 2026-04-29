#!/usr/bin/env bash
#
# Spins up a throwaway git worktree for rehearsing an ATDD run.
#
# Usage: scripts/atdd-rehearsal-start.sh <name>
#
# Example: scripts/atdd-rehearsal-start.sh demo1
#   creates a sibling directory `../rehearsal-demo1`
#   on a fresh branch `rehearsal/demo1` based on the current HEAD.
#
# Multiple rehearsals can coexist — pick a unique <name> for each one.
# Run `git worktree list` to see what's currently checked out.
# When you're done with a rehearsal, run:
#   scripts/atdd-rehearsal-end.sh <name>
#
# Note: only committed state from your current branch is carried into
# the rehearsal. Uncommitted changes in your working tree are NOT copied.
#

set -euo pipefail

if [ $# -ne 1 ] || [ -z "${1:-}" ]; then
    echo "Usage: $0 <name>"
    echo "Example: $0 demo1"
    exit 1
fi

NAME="$1"

if ! [[ "$NAME" =~ ^[A-Za-z0-9_-]+$ ]]; then
    echo "ERROR: name must contain only letters, digits, hyphens, or underscores."
    echo "Got: '$NAME'"
    exit 1
fi

REPO_ROOT="$(git rev-parse --show-toplevel)"
WORKTREE_DIR="$REPO_ROOT/../rehearsal-$NAME"
BRANCH="rehearsal/$NAME"

if [ -e "$WORKTREE_DIR" ]; then
    echo "ERROR: $WORKTREE_DIR already exists."
    echo "Either pick a different name, or run:"
    echo "  scripts/atdd-rehearsal-end.sh $NAME"
    exit 1
fi

if git show-ref --verify --quiet "refs/heads/$BRANCH"; then
    echo "ERROR: branch '$BRANCH' already exists."
    echo "Either pick a different name, or run:"
    echo "  scripts/atdd-rehearsal-end.sh $NAME"
    exit 1
fi

git worktree add "$WORKTREE_DIR" -b "$BRANCH" HEAD

# Resolve to absolute path for the user
ABS_WORKTREE_DIR="$(cd "$WORKTREE_DIR" && pwd)"

echo
echo "Rehearsal worktree ready."
echo "  Name:   $NAME"
echo "  Path:   $ABS_WORKTREE_DIR"
echo "  Branch: $BRANCH (based on current HEAD)"
echo
echo "Next steps — run these in a NEW terminal:"
echo
echo "  cd \"$ABS_WORKTREE_DIR\""
echo "  claude"
echo
echo "Then inside that fresh Claude Code session, run the ATDD slash command, e.g.:"
echo
echo "  /atdd:atdd-implement-ticket #<issue-number>"
echo
echo "(A new session is required so Claude Code's working directory is the worktree;"
echo " otherwise sub-agents would commit into your real repo, not the rehearsal.)"
echo
echo "When this rehearsal is no longer needed, from any shop checkout run:"
echo
echo "  scripts/atdd-rehearsal-end.sh $NAME"
echo
echo "To see all active rehearsals:"
echo
echo "  git worktree list"
