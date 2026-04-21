# 20260421-1127 — .NET System Test Alignment Plan (both)

🤖 **Picked up by agent** — `Valentina_Desk` at `2026-04-21T10:03:44Z`

Reference report: [20260421-1127-compare-tests-both.md](../reports/20260421-1127-compare-tests-both.md)

Timestamp: 20260421-1127
Mode: both
Reference implementation: **Java** (align .NET to Java unless explicitly stated otherwise).

All action items below are consistent with the **Known Language-Specific Divergences (Exceptions)** list in the `compare-tests` agent spec. Items covered by that list (e.g. `VoidValue.cs`, `ResultTaskExtensions.cs`, Then Success/Failure split, `IThenFailureAnd.cs`, `IThenSuccessAnd.cs`) are intentionally **not** included here.

Ordering: architectural mismatches → architecture layers (clients → drivers → channels → use-case DSL → scenario DSL → common) → tests (acceptance → contract → e2e → smoke).

---

## 1. Architecture — Use Case DSL

## 2. Architecture — Scenario DSL

---

## Local verification & commit

1. From `system-test/dotnet/`:
   - Run `Run-SystemTests -Architecture monolith` (latest suite). Do not substitute raw `dotnet test`.
   - Run `Run-SystemTests -Architecture monolith -Legacy` (legacy suite).
2. Fix any failures introduced by the renames (most likely: broken `using` directives, missing interface references).
3. Commit the .NET alignment changes as a single logical commit with a message such as:
   `dotnet: rename Base*Command → Base*UseCase, GherkinDefaults → ScenarioDefaults, I{X} → IWhen{X}, add Dsl.Core SystemResults`
