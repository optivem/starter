# Mechanical-vs-Creative Inventory of the ATDD Pipeline

> Output of Step 1 in `plans/20260429-211522-script-vs-agent-atdd-orchestration.md`. Read-only analysis: tags every section of every pipeline agent body **and** every doc those agents `@include` as **Mechanical** (rule-based / deterministic) or **Creative** (judgment-required), and tallies token cost per agent invocation and per typical ticket pass.

## Method & caveats

- **Token estimate.** All counts are `wc -w` × 1.33 (rough English-tokens heuristic). Code blocks and tables are denser per-token than prose, so the absolute totals are an upper bound; the *relative* mechanical-vs-creative split is what the plan's "≥30% reduction" gate cares about, and that ratio is robust to the heuristic.
- **Pipeline scope.** Only the 12 ATDD pipeline agents (matching the plan's "Current state" table) — i.e. files directly under `.claude/agents/atdd/`. The 6 `meta/` agents (diagram-*, process-audit, token-usage-audit, architecture-sync) are out of scope.
- **`@include` resolution.** Every agent's loaded context = agent body + transitive `@includes`. Each unique included doc is counted once per invocation that loads it (so shared docs are paid for repeatedly across a ticket pass).
- **"Section" granularity.** For agent bodies, sections = the markdown headings or numbered procedure steps. For included docs, sections = top-level headings.
- **Tagging key.**
  - **M** — *Mechanical*: flow control, gate checks, file lookups, ticket-state mutations, commit boilerplate, classification rules, output-format conventions, "run `gh ...`" commands.
  - **C** — *Creative*: artefact-authoring guidance — what good Gherkin/DSL/driver/production code looks like, design rules, anti-patterns, review criteria.
  - **X** — *Mixed*: section interleaves M and C; a clean split would require restructuring.

---

## Per-agent inventory

### atdd-orchestrator (714 words ≈ 950 tokens) — **100% Mechanical**

No `@includes`. Body sections:

| § | Section | Words | Tag | Notes |
|---|---|---:|:-:|---|
| 1 | Choose ticket source (3 input branches + ask) | ~165 | M | Pure rule on input flags; only "ask user a/b" if neither flag present |
| 2 | Board mode — resolve project (gh/README/git remote fallback) | ~110 | M | Sequence of deterministic lookups |
| 2a | Pick the ticket (top of Ready → bottom of In Progress) | ~30 | M | Two `gh project` mutations |
| 2b | Specific-issue mode — resolve ticket (`gh issue view ... --json ...`) | ~85 | M | Single `gh` call + state guard |
| 3 | Resolve repositories (default-to-`shop` + TODO marker for multitier) | ~110 | M | Hard-coded mapping table |
| 4 | Hand off (issue number + repo lists; classification deferred to dispatcher) | ~90 | M | Output-format rule |
| 5 | Sequential processing rule | ~25 | M | One-line invariant |
| — | Empty-Ready / not-fetchable bail-outs | ~30 | M | Error rules |

**Verdict:** confirms plan's table. The whole agent demotes cleanly to `internal/atdd/runtime/board/`. The single creative-shaped step ("ask a/b if neither flag is supplied") is `bufio.Scanner` on stdin — not an LLM round-trip.

### atdd-dispatcher (663 words ≈ 882 tokens) — **100% Mechanical** (with one ask-user escape)

No `@includes`. Body sections:

| § | Section | Words | Tag | Notes |
|---|---|---:|:-:|---|
| — | Intro: fetch via `gh issue view ... --json projectItems,labels` | ~80 | M | gh call + parse |
| — | Top-level type definitions (story/bug/task) | ~80 | M | Routing dictionary |
| — | Task subtype definitions (3 subtypes) | ~75 | M | Routing dictionary |
| 1 | Rule 1: prefer Projects v2 `Type` field | ~50 | M | Rule |
| 2 | Rule 2: labels for top-level type + subtype | ~115 | M | Rule with canonical token list |
| 3 | Rule 3: body-shape fallback (with stop-and-ask for missing subtype) | ~75 | M | Rule + escape |
| 4 | Rule 4: stop-and-ask on signal conflict | ~50 | M | Rule + escape |
| — | "Don't second-guess" guardrail | ~30 | M | Rule |
| — | Fast path: single canonical label → emit one-line decision | ~95 | M | Pure shortcut |
| — | Output format | ~45 | M | Output convention |

**Verdict:** confirms plan's "mostly mechanical". The fast path (single canonical label) covers most tickets — pure rule, no LLM needed. The "stop and ask" escapes (rule 3 missing subtype, rule 4 conflict) are the only LLM cases — and in both cases the existing prose just hands off to the user. A Go classifier would do the same: stdin prompt, no LLM. The plan's `atdd-classify-fallback` agent only carries weight if the user *wants* an LLM second opinion before asking — currently the agent doesn't do that.

### atdd-story (128 words ≈ 170 tokens) — **~75% Creative**

No `@includes`. 4 numbered steps:

| § | Section | Words | Tag |
|---|---|---:|:-:|
| 1 | Scan existing tests for uncovered behaviours → propose Legacy Coverage | ~25 | C |
| 2 | Produce Gherkin scenarios from acceptance criteria | ~20 | C |
| 3 | Add Legacy Coverage to issue under `## Legacy Coverage` section | ~25 | M |
| 4 | Present + STOP for human approval | ~20 | M |

**Verdict:** confirms plan. Body is small; the work is creative. Gherkin authoring is exactly the "judgment is actually needed" case from the plan motivation §2.

### atdd-bug (317 words ≈ 422 tokens) — **~80% Creative**

No `@includes`. Sections:

| § | Section | Words | Tag |
|---|---|---:|:-:|
| — | Intro: bug differs from story (steps-to-reproduce / actual / expected) | ~50 | C |
| 1 | Extract steps / expected / actual from bug report | ~70 | C |
| 2 | Scan existing tests for uncovered behaviours → propose Legacy Coverage | ~30 | C |
| 3 | One Gherkin scenario per repro path (single by default; "genuinely distinct" judgement) | ~95 | C |
| 4 | Produce Gherkin for Legacy Coverage | ~10 | C |
| 5 | Add Legacy Coverage to issue | ~25 | M |
| 6 | Present + STOP | ~20 | M |

**Verdict:** confirms plan. Heavily creative — "genuinely distinct repro path" is the kind of judgement that doesn't compress to a rule.

### atdd-task (619 words ≈ 823 tokens body) + 8 includes (≈ 6,083 tokens) ⇒ **~6,906 tokens loaded per invocation**

`@includes`: `shared-commit-confirmation`, `shared-phase-progression`, `task-and-chore-cycles`, `architecture/system`, `architecture/driver-port`, `architecture/driver-adapter`, `process/glossary`, `code/language-equivalents`.

| § | Section | Words | Tag | Notes |
|---|---|---:|:-:|---|
| — | Intro + `gh issue view` fetch | ~60 | M | Mechanical handoff |
| — | Subtype mapping (UX/UI / System API / External API → directories) | ~70 | M | Routing |
| — | "Apply Driver Port/Adapter Rules" pointer | ~25 | C | Tells agent to apply creative rules |
| Scope | `Scope:` block parsing rule, restrict edits to in-scope architecture(s) + system languages | ~110 | M | Rule |
| 1 | Identify layer + driver | ~85 | M | Routing rule |
| 2 | Implement system change (frontend/backend/external contract) | ~25 | C | Creative work |
| 3 | Adapt driver impl to absorb change | ~35 | C | Creative work |
| 4 | Driver interface guardrail (STOP and present if interface change unavoidable) | ~115 | M | Gate |
| 5 | Don't run tests/compile yourself; orchestrator runs structural-cycle TEST | ~110 | M | Gate / rule |
| 6 | Report-back format | ~85 | M | Output convention |

**Body verdict:** ~70% mechanical. The creative chunk (steps 2–3) is short — the *hard* creative work is in the included `architecture/system.md`, `driver-port.md`, `driver-adapter.md` (all C: design rules + code patterns).

**Includes verdict:**
- `shared-commit-confirmation` (504t) — **M**
- `shared-phase-progression` (69t) — **M**
- `task-and-chore-cycles` (1,875t) — **~85% M** (commit-message format, structural-cycle TEST procedure with `full|compile|skip` gate, structural-cycle COMMIT procedure, all SYSTEM \<boundary\> REDESIGN scaffolding). Creative bits: "WRITE goal" definitions for system-vs-UI vs chore.
- `architecture/system` (624t) — **C** (where API/UI surface lives in each language × deployment shape — judgement-relevant code-layout knowledge)
- `architecture/driver-port` (388t) — **C** (DTO rules, ID-only `getX(id)`, no direct UI URLs, etc.)
- `architecture/driver-adapter` (559t) — **C** (Real/Stub split, `Ext*` DTOs, `goTo*()` checks, page-object selectors)
- `process/glossary` (1,609t) — **~70% C** (definitions of behavioral / structural / interface change / Legacy Coverage; mostly substance, but the routing-implication paragraphs are mechanical)
- `code/language-equivalents` (455t) — **C** (tables of TODO stub syntax, test-disabling syntax, string field types per language)

### atdd-chore (635 words ≈ 845 tokens body) + 3 includes (≈ 2,448 tokens) ⇒ **~3,293 tokens loaded**

`@includes`: `shared-commit-confirmation`, `shared-phase-progression`, `task-and-chore-cycles`.

| § | Section | Words | Tag |
|---|---|---:|:-:|
| — | Intro + Scope block (same shape as task) | ~150 | M |
| — | Definition: chore = structural change, no new AC | ~75 | C |
| 1 | Extract scope + intent from ticket | ~30 | C |
| 2 | Confirm purely structural; reclassify if observable behaviour change | ~30 | C |
| 3 | Scan existing tests for uncovered behaviours → propose Legacy Coverage | ~25 | C |
| 4-7 | No new AC; produce Gherkin for Legacy Coverage; add to issue; present + STOP | ~85 | M+C |
| CHORE-WRITE | Implement structural change; driver/test guardrails | ~115 | C+M |
| CHORE-REVIEW | STOP. Present + ask | ~25 | M |
| CHORE-TEST + COMMIT | Defers to shared structural-cycle TEST/COMMIT | ~100 | M |

**Verdict:** ~50/50 mixed; the includes are heavily M (`task-and-chore-cycles` carries the gate procedures).

### atdd-test (161 words ≈ 214 tokens body) + 9 includes (≈ 3,326 tokens) ⇒ **~3,540 tokens**

`@includes`: `shared-commit-confirmation`, `shared-phase-progression`, `at-cycle-conventions`, `ct-cycle-conventions`, `at-red-test`, `ct-red-test`, `architecture/test`, `architecture/dsl-core`, `code/language-equivalents`.

**Body:** ~95% M — pure dispatcher ("follow the phase specified in the input; load these phase docs; STOP on review"). The creative pointer is the one-liner "Apply test file rules from `test.md` and DSL Core Rules from `dsl-core.md`".

**Includes (token cost shown; tag column applies to **content**):**

| Include | Tokens | Tag | Mechanical share |
|---|---:|:-:|---:|
| shared-commit-confirmation | 504 | M | 100% |
| shared-phase-progression | 69 | M | 100% |
| at-cycle-conventions | 239 | M | 100% (commit message format, suite selection) |
| ct-cycle-conventions | 477 | M | 100% (suite selection, sub-process trigger commentary) |
| at-red-test | 710 | X | ~50% — WRITE substance (one-to-one Gherkin↔test, ordering rule, DSL extension protocol) is C; COMMIT mechanics (try-compile, STOP if DSL added, run `gh optivem test system`, mark disabled, COMMIT, STOP) is M |
| ct-red-test | 298 | X | ~50% (same shape, smaller) |
| architecture/test | 182 | C | 0% — Positive vs Negative test-class rule + TODO ordering rule |
| architecture/dsl-core | 392 | C | 0% — DSL implementation patterns |
| code/language-equivalents | 455 | C | 0% — per-language syntax tables |

**Verdict:** the *body* of `atdd-test` is ~100% dispatch (mechanical). The *creative content* lives in the includes — and ~58% of the includes by token count is mechanical orchestration boilerplate (everything except `at-red-test` substance, `ct-red-test` substance, `architecture/test`, `dsl-core`, `language-equivalents`). That's ~1,930 tokens of mechanical re-loading per `atdd-test` invocation.

### atdd-dsl (156 words ≈ 207 tokens body) + 9 includes (≈ 2,810 tokens) ⇒ **~3,017 tokens**

`@includes`: `shared-commit-confirmation`, `shared-phase-progression`, `at-cycle-conventions`, `ct-cycle-conventions`, `at-red-dsl`, `ct-red-dsl`, `architecture/dsl-core`, `architecture/driver-port`, `code/language-equivalents`.

Same shape as `atdd-test`: body is dispatch (M), includes are mixed.

| Include | Tokens | Tag | Mechanical share |
|---|---:|:-:|---:|
| shared-commit-confirmation | 504 | M | 100% |
| shared-phase-progression | 69 | M | 100% |
| at-cycle-conventions | 239 | M | 100% |
| ct-cycle-conventions | 477 | M | 100% |
| at-red-dsl | 375 | X | ~60% M (enable disabled tests, run gh optivem, mark disabled, set External/System Driver Interface Changed flags, COMMIT, STOP); 40% C (DSL implementation guidance, "interface change" judgement) |
| ct-red-dsl | 303 | X | ~60% M / 40% C |
| architecture/dsl-core | 392 | C | 0% |
| architecture/driver-port | 388 | C | 0% |
| code/language-equivalents | 455 | C | 0% |

**Mechanical share of includes:** ~1,696 tokens / 2,810 = ~60%.

### atdd-driver (155 words ≈ 206 tokens body) + 8 includes (≈ 2,664 tokens) ⇒ **~2,870 tokens**

`@includes`: `shared-commit-confirmation`, `shared-phase-progression`, `at-cycle-conventions`, `ct-cycle-conventions`, `at-red-system-driver`, `ct-red-external-driver`, `architecture/driver-port`, `code/language-equivalents`.

Same shape. The two phase docs (`at-red-system-driver`, `ct-red-external-driver`) are highly mechanical (~75% M each — "enable tests, replace TODOs, run gh optivem, mark disabled, COMMIT, STOP"); only the implementation guidance ("model new methods on existing driver methods in the same file") is C.

**Mechanical share of includes:** ~1,800 / 2,664 = ~68%.

### atdd-backend (57 words ≈ 76 tokens body) + 3 includes (≈ 1,056 tokens) ⇒ **~1,132 tokens**

Body: 100% dispatch. Includes:

| Include | Tokens | Tag | Mechanical share |
|---|---:|:-:|---:|
| shared-commit-confirmation | 504 | M | 100% |
| shared-phase-progression | 69 | M | 100% |
| at-green-system | 483 | X | ~70% M — "enable disabled, run `gh optivem test system --rebuild`, fix until passing, REVIEW STOP, COMMIT, tick checklist, IN ACCEPTANCE"; ~30% C — backend implementation guidance |

**Mechanical share of includes:** ~919 / 1,056 = ~87%.

### atdd-frontend (57 words ≈ 76 tokens body) + 3 includes (≈ 1,056 tokens) ⇒ **~1,132 tokens**

Identical shape to `atdd-backend`.

### atdd-release (98 words ≈ 130 tokens body) + 5 includes (≈ 1,907 tokens) ⇒ **~2,037 tokens**

`@includes`: `shared-commit-confirmation`, `shared-phase-progression`, `shared-ticket-status-in-acceptance`, `at-cycle-conventions`, `at-green-system`.

Body: ~100% M (release is mechanical per plan).

**Mechanical share of includes:** ~1,580 / 1,907 = ~83% (only the at-green-system "implement backend/frontend" half is C, but the release agent doesn't actually use that half — it only runs the COMMIT step).

---

## Aggregate per-ticket token cost (UI-only story, no CT, no chore)

A typical UI story flowing through the AT cycle goes through these invocations:

| # | Invocation | Tokens loaded |
|---:|---|---:|
| 1 | atdd-orchestrator | 950 |
| 2 | atdd-dispatcher | 882 |
| 3 | atdd-story | 170 |
| 4 | atdd-test (WRITE) | 3,540 |
| 5 | atdd-test (COMMIT) | 3,540 |
| 6 | atdd-dsl (WRITE) | 3,017 |
| 7 | atdd-dsl (COMMIT) | 3,017 |
| 8 | atdd-driver (WRITE) — only if System Driver Interface Changed | 2,870 |
| 9 | atdd-driver (COMMIT) — same condition | 2,870 |
| 10 | atdd-frontend (WRITE) | 1,132 |
| 11 | atdd-release (COMMIT) | 2,037 |
| | **Total** | **~24,025 tokens** |

For an API-only story, swap atdd-frontend for atdd-backend (same cost). For a story without driver interface changes, drop invocations 8–9 (saves ~5,740 tokens).

A `task` or `chore` ticket is shorter (no test/dsl/driver) but each invocation is heavier (~6,906 for task, ~3,293 for chore).

---

## Mechanical surface area, summed across the 11-invocation pass

**Per the section-level tagging:**

| Source of static prose loaded | Mechanical tokens | Creative tokens | Mechanical % |
|---|---:|---:|---:|
| atdd-orchestrator (1×) | 950 | 0 | 100% |
| atdd-dispatcher (1×) | 882 | 0 | 100% |
| atdd-story (1×) | ~45 | ~125 | ~26% |
| atdd-test body (2×) | ~430 | ~0 | ~100% |
| atdd-dsl body (2×) | ~415 | ~0 | ~100% |
| atdd-driver body (2×) | ~410 | ~0 | ~100% |
| atdd-frontend body (1×) | ~76 | ~0 | 100% |
| atdd-release body (1×) | ~130 | ~0 | 100% |
| Includes loaded across the 11 invocations (sum, with duplicates) | ~12,300 | ~6,300 | ~66% |
| **Pass total** | **~15,640** | **~6,425** | **~71%** |

≈ **71% of the static prose loaded per ticket is mechanical**. The rest (creative phase substance + Gherkin authoring rules) is what an LLM actually needs to do its job.

> *Caveat:* "static prose" here is just the agent body + included docs. The agents *also* load the test/system source files they edit (variable per ticket) and emit completions. Both are excluded from this tally — the plan only proposes savings on the **prose overhead**. So "71% mechanical of the static surface" is not the same as "71% reduction in total tokens per ticket". It's the upper bound on what could be removed by demoting orchestration to Go.

---

## Confidence in the plan's "40-60% reduction" estimate

The plan (motivation §3, token-cost argument) estimates 40–60% reduction in tokens per ticket, "concentrated in the orchestration overhead that's currently paid every phase".

The 71% mechanical share **across static prose** is consistent with that range — but the realised savings will be lower, because:

- The agent **also** loads test/system source files at WRITE time. That payload is unchanged by the script-vs-agent split. If source-loading is on average half the per-invocation token spend, the total reduction lands closer to 35-40% (≈ 71% × 50%).
- Some mechanical content (commit-message format conventions, "stop on STOP" rule) needs to be available *somewhere* the agent can see, even after demotion — likely a one-line front-matter directive in the trimmed agent body. Net savings on that fraction is closer to 90% than 100%.
- The phase substance docs themselves are "mixed" (~50% M for at-red-test, ~60% M for at-red-dsl, etc.) — splitting them cleanly will require some restructuring effort, which may leave residual M content for cohesion's sake.

**Realistic range: 30-50% per ticket**, with the high end requiring aggressive slimming of the "mixed" phase docs. The plan's 40-60% estimate is achievable on the high end only with the per-phase doc restructuring described in plan §"Process flow: YAML-canonical, Mermaid-derived view".

---

## Confirmations vs revisions of the plan's split decisions

### Confirmed

- **Demote `atdd-orchestrator` to Go.** Body is 100% mechanical. No carve-outs needed.
- **Demote `atdd-dispatcher` fast path to Go.** Fast-path classification (single canonical label) covers the common case as a pure rule. The "stop and ask user on conflict / missing subtype" escape hatches are stdin prompts, not LLM calls. The plan's `atdd-classify-fallback` agent is **not strictly needed** for v1 — the existing dispatcher just escalates to the user, and a Go classifier can do the same with no quality loss. Re-evaluate if a future fallback policy ever wants an LLM second-opinion on ambiguous tickets.
- **Demote `atdd-release` to Go.** Body is 100% mechanical (regex `@Disabled` removal, `gh issue close`, IN ACCEPTANCE move). The "ask before commit" gate is an interactive stdin prompt — already mechanical.
- **Keep all 9 creative agents.** `atdd-story`, `atdd-bug`, `atdd-task`, `atdd-chore`, `atdd-test`, `atdd-dsl`, `atdd-driver`, `atdd-backend`, `atdd-frontend`. Their bodies vary in size but the *substance* they apply lives in the included docs and must remain LLM-driven.

### Revisions / additional observations

1. **The biggest token sink is not the demotable agents themselves — it's the per-phase `@includes` re-loaded by every creative agent.** The plan's §"Implementation order" item 2 already addresses this ("restructure per-phase docs to drop 'what runs next' prose"), but the inventory makes the size of the prize concrete: ~12,300 mechanical tokens × multiple invocations per ticket. **Recommend: prioritise the per-phase doc restructure (plan step 2) over the Go runtime build-out (plan step 3) for early token wins.** A single PR that strips orchestration prose from `at-red-test.md`, `at-red-dsl.md`, `at-red-system-driver.md`, `at-green-system.md`, and the four `ct-*.md` docs would yield a meaningful reduction *before* any Go code lands — and the demoted prose has a natural home (the YAML process-flow file in plan step 2).

2. **The thin "dispatcher" agents (`atdd-test`, `atdd-dsl`, `atdd-driver`, `atdd-backend`, `atdd-frontend`) are already paying minimal body cost.** Their bodies are ~155–215 tokens each; slimming them further has marginal value. The plan's intention is right but the savings come from the includes, not the bodies.

3. **`shared-commit-confirmation.md` (504t) and `shared-phase-progression.md` (69t)** are loaded by 8 of the 12 agents. Demoting the commit-confirmation gate to a Go interactive prompt removes 504t × 8 ≈ 4,000t from a typical ticket pass *across all invocations* — second-largest single win after orchestrator/dispatcher demotion.

4. **`docs/atdd/process/glossary.md` (1,609t)** is loaded only by `atdd-task`. It's mostly creative reference but ~30% M (routing implications). Splitting glossary into a "definitions" file (C, kept) and a "routing-implications" appendix (M, retired into the YAML / cycle docs) would shave ~480t off `atdd-task` alone.

5. **`task-and-chore-cycles.md` (1,875t)** is the single biggest mechanical doc loaded — by `atdd-task` and `atdd-chore`. ~85% M (structural-cycle TEST gate, structural-cycle COMMIT gate, commit-message format, shared procedures). After the Go runtime owns those gates, this file contracts dramatically (the residual ~15% — the "WRITE goal" definitions — moves into the per-phase docs for `system-api-redesign`, `system-ui-redesign`, `chore`).

6. **One agent has no mechanical includes: `atdd-story` and `atdd-bug`** carry zero `@includes`. They pay only their body cost (170t and 422t). They are already minimal — no slimming needed.

7. **`atdd-orchestrator` deletion sequencing.** Once demoted, the agent file should not be deleted before `atdd:atdd-implement-ticket` and `atdd:atdd-manage-project` slash commands are repointed (plan step 5), or the user-facing entry-points break. Plan step 7 ("delete demoted agents only after one full week of green pipeline runs") covers this — flagging here for explicit traceability.

---

## Output artefacts of step 1

- This file: `plans/20260430-070908-mechanical-creative-inventory.md`.
- No code changes.

## Inputs to step 2

The per-phase doc restructure (plan step 2 item 4 — "Restructure per-phase docs … to drop 'what runs next' prose and focus on substance") should target these files in order of mechanical-share × token cost:

1. `task-and-chore-cycles.md` — 1,875t × 85% M = ~1,594t demotable. **Highest-impact single doc.**
2. `at-red-test.md` — 710t × 50% M = ~355t demotable.
3. `shared-commit-confirmation.md` — 504t, 100% M, demotable wholesale once Go owns the commit gate. Hot doc (loaded by 8 agents).
4. `at-red-dsl.md` — 375t × 60% M = ~225t demotable.
5. `at-green-system.md` — 483t × 70% M = ~338t demotable.
6. `ct-cycle-conventions.md` — 477t, 100% M, demotable wholesale.
7. `at-red-system-driver.md` — 299t × 75% M = ~224t demotable.
8. `at-cycle-conventions.md` — 239t, 100% M, demotable wholesale.
9. `ct-red-test.md` / `ct-red-dsl.md` / `ct-red-external-driver.md` — same shape as their AT counterparts; smaller absolute savings.
10. `glossary.md` — 1,609t × 30% M = ~480t demotable (routing-implications appendix only).
