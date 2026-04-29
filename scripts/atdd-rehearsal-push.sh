#!/usr/bin/env bash
#
# Pushes a rehearsal branch to origin so it's visible on GitHub / accessible
# from other machines.
#
# Usage: scripts/atdd-rehearsal-push.sh <name>
#
# Example: scripts/atdd-rehearsal-push.sh demo1
#   pushes the local branch `rehearsal/demo1` to `origin/rehearsal/demo1`
#   and sets upstream tracking on first push.
#
# This script is non-destructive: it only adds remote state. It does not
# force-push, and it does not delete anything. To clean up afterwards
# (locally and on the remote), see the commands printed at the end.
#

set -euo pipefail

if [ $# -ne 1 ] || [ -z "${1:-}" ]; then
    echo "Usage: $0 <name>"
    echo "Example: $0 demo1"
    echo
    echo "Active rehearsal branches:"
    git for-each-ref --format='  %(refname:short)' 'refs/heads/rehearsal/*' \
        | sed 's|^  rehearsal/|  |' \
        || echo "  (none)"
    exit 1
fi

NAME="$1"

if ! [[ "$NAME" =~ ^[A-Za-z0-9_-]+$ ]]; then
    echo "ERROR: name must contain only letters, digits, hyphens, or underscores."
    echo "Got: '$NAME'"
    exit 1
fi

BRANCH="rehearsal/$NAME"

if ! git show-ref --verify --quiet "refs/heads/$BRANCH"; then
    echo "ERROR: branch '$BRANCH' does not exist locally."
    echo
    echo "Active rehearsal branches:"
    git for-each-ref --format='  %(refname:short)' 'refs/heads/rehearsal/*' \
        || echo "  (none)"
    exit 1
fi

echo "Pushing $BRANCH to origin (with upstream tracking)..."
echo
git push -u origin "$BRANCH"

echo
echo "Done. Rehearsal branch is now on the remote."
echo
echo "When you're finished with this rehearsal, clean up with:"
echo
echo "  git push origin --delete $BRANCH    # remove from remote"
echo "  scripts/atdd-rehearsal-end.sh $NAME # then print local cleanup commands"
echo
