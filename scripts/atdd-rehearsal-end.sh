#!/usr/bin/env bash
#
# Reports what would be cleaned up for a rehearsal — and prints the
# exact commands you can copy-paste to actually delete it.
#
# Usage:
#   scripts/atdd-rehearsal-end.sh <name>              # one rehearsal, print only
#   scripts/atdd-rehearsal-end.sh <name> --execute    # one rehearsal, actually delete
#   scripts/atdd-rehearsal-end.sh --all               # every rehearsal, print only
#   scripts/atdd-rehearsal-end.sh --all --execute     # every rehearsal, actually delete
#
# Default behavior is print-only: cleanup is destructive (force-removing
# worktrees, force-deleting branches along with any commits on them), so
# deletion requires the explicit --execute flag.
#

set -euo pipefail

REPO_ROOT="$(git rev-parse --show-toplevel)"

# Print status + cleanup commands for a single rehearsal name.
# Sets globals `had_anything` (1 if anything to clean up), appends
# to `all_cleanup_cmds` (commands for batch mode), and appends
# to `all_worktree_paths` (for cwd-safety check before --execute).
print_rehearsal() {
    local name="$1"
    local worktree_dir="$REPO_ROOT/../rehearsal-$name"
    local branch="rehearsal/$name"

    local worktree_exists=0
    local branch_exists=0

    if [ -d "$worktree_dir" ]; then
        worktree_exists=1
        # Canonicalize: collapse the ".." and resolve symlinks so the printed
        # path is short enough not to line-wrap on copy-paste, and matches
        # exactly what git has registered for the worktree.
        worktree_dir="$(cd "$worktree_dir" && pwd -P)"
    fi
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
        all_worktree_paths+=("$worktree_dir")
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

# Refuse to delete a worktree we're standing inside — git would fail
# anyway, but its error is cryptic. Give a clear message instead.
check_not_inside_target_worktree() {
    local cwd_real
    cwd_real="$(pwd -P)"
    for wt in "${all_worktree_paths[@]}"; do
        local wt_real
        # Resolve the worktree path (which contains "..") to a canonical form.
        if wt_real="$(cd "$wt" 2>/dev/null && pwd -P)"; then
            case "$cwd_real/" in
                "$wt_real/"*)
                    echo "ERROR: you are currently inside a worktree that --execute would delete:"
                    echo "  cwd:      $cwd_real"
                    echo "  worktree: $wt_real"
                    echo
                    echo "Run --execute from outside that worktree (e.g. cd to the main repo first)."
                    exit 1
                    ;;
            esac
        fi
    done
}

# Run the collected cleanup commands. Fails fast on first error.
run_cleanup() {
    echo "----"
    echo "Executing cleanup..."
    echo
    for cmd in "${all_cleanup_cmds[@]}"; do
        echo "  > $cmd"
        eval "$cmd"
    done
    echo "  > git worktree prune"
    git worktree prune
    echo
    echo "Cleanup complete."
}

# --- Argument parsing ---

EXECUTE=0
POSITIONAL=()
for arg in "$@"; do
    case "$arg" in
        --execute|--yes)
            EXECUTE=1
            ;;
        -h|--help)
            sed -n '2,12p' "$0" | sed 's/^# \?//'
            exit 0
            ;;
        *)
            POSITIONAL+=("$arg")
            ;;
    esac
done

if [ "${#POSITIONAL[@]}" -ne 1 ] || [ -z "${POSITIONAL[0]:-}" ]; then
    echo "Usage:"
    echo "  $0 <name> [--execute]     # one rehearsal"
    echo "  $0 --all  [--execute]     # every rehearsal"
    echo
    echo "Without --execute, prints the cleanup commands but does not run them."
    echo
    echo "Active rehearsals:"
    git worktree list | grep -E "/rehearsal-" || echo "  (none)"
    exit 1
fi

ARG="${POSITIONAL[0]}"

had_anything=0
all_cleanup_cmds=()
all_worktree_paths=()

if [ "$ARG" = "--all" ]; then
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
        if [ "$EXECUTE" -eq 1 ]; then
            check_not_inside_target_worktree
            run_cleanup
        else
            echo "----"
            echo "All-in-one cleanup block (copy-paste to delete every rehearsal above):"
            echo
            for cmd in "${all_cleanup_cmds[@]}"; do
                echo "  $cmd"
            done
            echo "  git worktree prune"
            echo
            echo "Nothing has been deleted. Re-run with --execute to actually delete."
        fi
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
    if [ "$EXECUTE" -eq 1 ]; then
        check_not_inside_target_worktree
        run_cleanup
    else
        echo "  git worktree prune    # safe no-op cleanup of stale worktree metadata"
        echo
        echo "Nothing has been deleted. Re-run with --execute to actually delete."
    fi
fi
