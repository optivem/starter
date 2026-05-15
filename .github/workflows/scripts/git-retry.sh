#!/usr/bin/env bash
# GENERATED — DO NOT EDIT.
# Source: optivem/actions/shared/git-retry.sh @ 6d1e9eadae45ff47577fa0e32fb2b169e3b9360f
# Sync via: bash optivem/actions/scripts/sync-shared.sh
# git-retry.sh — retry wrapper for `git` invocations that hit a remote
# (push, fetch — and only those). Local-only `git` operations (status, tag
# creation, config, rev-parse, merge-base) don't go through here.
#
# Source this file from any action.yml composite step or workflow that
# shells out to `git push` / `git fetch`, then call the matching wrapper:
#
#   source "$GITHUB_ACTION_PATH/../shared/git-retry.sh"
#   git_push_retry origin "$TAG"
#   git_fetch_retry origin "$BASE_BRANCH" --quiet
#
# The wrapper buffers each attempt's stdout and stderr. On success, stdout
# is written to the function's stdout (preserving `$(...)` capture
# semantics) and stderr is forwarded to the caller's stderr. On transient
# failure (HTTP 5xx from the git host, network/DNS/TLS blips, connection
# resets, EOF), the call is retried up to 4 times with 5s → 15s → 45s
# backoff between attempts. On hard failure (auth, permission, remote
# rejection, repository-not-found), the wrapper returns the attempt's
# output and preserves the original non-zero exit code so callers that use
# exit code for flow control (e.g. concurrent-push-race recovery) keep
# working unchanged.
#
# `--quiet` suppresses git's progress meter on stdout but does not silence
# stderr error lines; the wrapper buffers stderr regardless, so transient
# detection still works under `--quiet`.
#
# Set `GIT_RETRY_DISABLE=1` to bypass the retry loop.

# shellcheck source=./retry-core.sh
source "$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)/retry-core.sh"

_GIT_RETRY_ATTEMPTS=4
_GIT_RETRY_DELAYS=(5 15 45)

# shellcheck disable=SC2034  # referenced via grep -E
_GIT_RETRY_RETRYABLE='Could not resolve host|Connection reset by peer|RPC failed.*HTTP 5[0-9][0-9]|Operation timed out|unable to access|\bEOF\b|TLS handshake|tls:.*handshake|temporary failure in name resolution|no such host|HTTP 502|HTTP 503|HTTP 504|server certificate verification failed'
# shellcheck disable=SC2034
_GIT_RETRY_HARD_FAIL='Permission denied|HTTP 401|HTTP 403|! \[remote rejected\]|pre-receive hook declined|repository .* not found|fatal: protocol|fatal: bad refspec'

git_push_retry() {
    if [[ "${GIT_RETRY_DISABLE:-0}" == "1" ]]; then
        git push "$@"
        return $?
    fi
    _RETRY_CORE_ATTEMPTS="$_GIT_RETRY_ATTEMPTS"
    _RETRY_CORE_DELAYS=("${_GIT_RETRY_DELAYS[@]}")
    retry_with_policy "$_GIT_RETRY_RETRYABLE" "$_GIT_RETRY_HARD_FAIL" git-push -- git push "$@"
}

git_fetch_retry() {
    if [[ "${GIT_RETRY_DISABLE:-0}" == "1" ]]; then
        git fetch "$@"
        return $?
    fi
    _RETRY_CORE_ATTEMPTS="$_GIT_RETRY_ATTEMPTS"
    _RETRY_CORE_DELAYS=("${_GIT_RETRY_DELAYS[@]}")
    retry_with_policy "$_GIT_RETRY_RETRYABLE" "$_GIT_RETRY_HARD_FAIL" git-fetch -- git fetch "$@"
}
