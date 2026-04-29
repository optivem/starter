#!/usr/bin/env bash
#
# Reports what would be cleaned up for a rehearsal — and prints the
# exact commands you can copy-paste to actually delete it.
#
# Usage:
#   scripts/atdd-rehearsal-end.sh <name>   # one rehearsal
#   scripts/atdd-rehearsal-end.sh --all    # every rehearsal at once
#
# This script intentionally does NOT delete anything. Cleanup is
# destructive (force-removing worktrees, force-deleting branches along
# with any commits on them), so deletion is left to you to run explicitly.
# Copy the commands printed below and run them when ready.
#

set -euo pipefail

REPO_ROOT="$(git rev-parse --show-toplevel)"

# Print status + cleanup commands for a single rehearsal name.
# Sets globals `had_anything` (1 if anything to clean up) and appends
# to `all_cleanup_cmds` (an array of bash commands for batch mode).
print_rehearsal() {
    local name="$1"
    local worktree_dir="$REPO_ROOT/../shop-rehearsal-$name"
    local branch="rehearsal/$name"

    local worktree_exists=0
    local branch_exists=0

    [ -d "$worktree_dir" ] && worktree_exists=1
    git show-ref --verify --quiet "refs/heads/$branch" && branch_exists=1 || true

    if [ "$worktree_exists" -eq 0 ] && [ "$branch_exists" -eq 0 ]; then
        echo "Rehearsal '$name': nothing to clean up."
        echo "  no worktree at $worktree_dir"
        echo "  no branch '$branch'"
        return
    fi

    had_anything=1

    echo "Rehearsal '$name':"
    if [ "$worktree_exists" -eq 1 ]; then
        echo "  WORKTREE exists: $worktree_dir"
    else
        echo "  WORKTREE: (gone)"
    fi
    if [ "$branch_exists" -eq 1 ]; then
        echo "  BRANCH   exists: $branch"
        local commit_count
        if commit_count=$(git rev-list --count "$branch" "^HEAD" 2>/dev/null); then
            if [ "$commit_count" -gt 0 ]; then
                echo "    ($commit_count commit(s) on '$branch' not reachable from current HEAD"
                echo "     — these would be discarded if you delete the branch)"
            fi
        fi
    else
        echo "  BRANCH:   (gone)"
    fi

    echo
    echo "  Commands to delete this rehearsal:"
    if [ "$worktree_exists" -eq 1 ]; then
        local cmd="git worktree remove \"$worktree_dir\" --force"
        echo "    $cmd"
        all_cleanup_cmds+=("$cmd")
    fi
    if [ "$branch_exists" -eq 1 ]; then
        local cmd="git branch -D $branch"
        echo "    $cmd"
        all_cleanup_cmds+=("$cmd")
    fi
    echo
}

# --- Argument parsing ---

if [ $# -ne 1 ] || [ -z "${1:-}" ]; then
    echo "Usage:"
    echo "  $0 <name>     # one rehearsal"
    echo "  $0 --all      # every rehearsal at once"
    echo
    echo "Active rehearsals:"
    git worktree list | grep -E "shop-rehearsal-" || echo "  (none)"
    exit 1
fi

ARG="$1"

had_anything=0
all_cleanup_cmds=()

if [ "$ARG" = "--all" ]; then
    # Enumerate all branches under refs/heads/rehearsal/
    rehearsal_names=()
    while IFS= read -r ref; do
        [ -n "$ref" ] && rehearsal_names+=("${ref#rehearsal/}")
    done < <(git for-each-ref --format='%(refname:short)' 'refs/heads/rehearsal/*')

    if [ "${#rehearsal_names[@]}" -eq 0 ]; then
        echo "No rehearsals found (no branches under 'rehearsal/*')."
        exit 0
    fi

    echo "Found ${#rehearsal_names[@]} rehearsal(s):"
    echo
    for name in "${rehearsal_names[@]}"; do
        print_rehearsal "$name"
    done

    if [ "$had_anything" -eq 1 ]; then
        echo "----"
        echo "All-in-one cleanup block (copy-paste to delete every rehearsal above):"
        echo
        for cmd in "${all_cleanup_cmds[@]}"; do
            echo "  $cmd"
        done
        echo "  git worktree prune"
        echo
        echo "Nothing has been deleted. Re-run with --all after cleanup to confirm."
    fi
    exit 0
fi

# Single-name mode
if ! [[ "$ARG" =~ ^[A-Za-z0-9_-]+$ ]]; then
    echo "ERROR: name must contain only letters, digits, hyphens, or underscores."
    echo "Got: '$ARG'"
    exit 1
fi

print_rehearsal "$ARG"

if [ "$had_anything" -eq 1 ]; then
    echo "  git worktree prune    # safe no-op cleanup of stale worktree metadata"
    echo
    echo "Nothing has been deleted. Re-run this script after cleanup to confirm."
fi
