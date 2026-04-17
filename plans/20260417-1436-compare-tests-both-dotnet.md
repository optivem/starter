# .NET — System Test Alignment Plan

Reference report: `reports/20260417-1436-compare-tests-both.md`

Reference implementation: **Java**. Each task aligns .NET to Java unless noted.

Ordering: architectural mismatches first, then architecture layers (clients → drivers → channels → use-case DSL → scenario DSL → common → ports), then tests (acceptance → contract → e2e → smoke).

---

## B. Architecture Layers — Clients

### ✅ B8. .NET — add `SystemErrorMapper`
- .NET has no equivalent. Error mapping currently happens inline in `BaseTaxClient.cs` and stub drivers (no centralized transform from `ProblemDetailResponse` → `SystemError`).
- Java reference: `SystemErrorMapper.java` in `testkit/driver/adapter/shop/api/`.
- Action: add `system-test/dotnet/Driver.Adapter/Shop/Api/SystemErrorMapper.cs` mirroring Java.
DONE

---

## F. Architecture Layers — Scenario DSL

### ✅ F1. .NET — remove scenario-layer `GoToShop` step and align mod08 smoke test to Java's `assume` pattern
- Scope (expanded — deleting the file alone would break compilation because the step is exposed by the `IWhenStage` interface and used by the mod08 smoke test):
  1. Delete `system-test/dotnet/Dsl.Core/Scenario/When/Steps/WhenGoToShop.cs`.
  2. Delete `system-test/dotnet/Dsl.Port/When/Steps/IGoToShop.cs`.
  3. Remove `IGoToShop GoToShop();` from `system-test/dotnet/Dsl.Port/When/IWhenStage.cs`.
  4. Remove both `GoToShop()` methods (public + explicit interface impl) from `system-test/dotnet/Dsl.Core/Scenario/When/WhenStage.cs`.
  5. Rewrite `system-test/dotnet/SystemTests/Legacy/Mod08/SmokeTests/System/ShopSmokeTest.cs` body to `await Scenario(channel).Assume().Shop().ShouldBeRunning();` (mirrors Java mod08 `scenario.assume().shop().shouldBeRunning();`).
- Not affected (these reference the unrelated use-case-layer `GoToShop` class under `Dsl.Core/UseCase/Shop/UseCases/GoToShop.cs`, which is kept):
  - `SystemTests/Legacy/Mod07/SmokeTests/System/ShopSmokeTest.cs` (uses `_app.Shop(channel).GoToShop()`).
  - `Dsl.Core/Scenario/Assume/AssumeStage.cs` (uses `_app.Shop(_channel).GoToShop()` internally).
  - `Dsl.Core/UseCase/Shop/ShopDsl.cs`.
DONE (expanded scope)

### ✅ F2. .NET — keep Success/Failure split (language-specific divergence; registered as an exception in the compare-tests agent)
- Files under `system-test/dotnet/Dsl.Core/Scenario/Then/Steps/`.
- The split is required by C# async semantics (different `RunPrelude()` for success vs failure; `GetAwaiter()` enables `await thenClause.HasSku(...)`). Java is synchronous and does not need it.
- No code change in the .NET source.
- Action: **registered as an exception in `.claude/agents/compare-tests.md` → Known Language-Specific Divergences → .NET-only.** Future compare-tests runs will not re-flag this.
RESOLVED

### ✅ F6. .NET — no change (two-step `.And().{Entity}()` is the async-adapted equivalent of Java's `ThenStep<TThen>`; registered as an exception in the compare-tests agent)
- Java has a single `ThenStep<TThen>` base port extended by every `ThenOrder`/`ThenCoupon`/`ThenClock`/`ThenProduct`/`ThenCountry`/`ThenSuccess`/`ThenFailure` interface — `and()` and entity accessors live on the same interface.
- .NET splits the same functionality across `IThenSuccess` + `IThenSuccessAnd` (and `IThenFailure` + `IThenFailureAnd`) — `.Then().ShouldSucceed().And().Order().HasSku(...)`. Mixed sync/async returns (`IThenOrder Order()` sync, `Task<IThenClock> Clock()` async) follow the existing pattern.
- Adding an `IThenStep<TThen>` port without refactoring every `IThenXxx` interface + `BaseThenResult*` class would be dead code. Doing the full refactor would fight the async-driven Success/Failure split already registered in F2.
- No code change.
- Action: **registered as an exception in `.claude/agents/compare-tests.md` → Known Language-Specific Divergences → .NET-only.** Future compare-tests runs will not propose adding `IThenStep<TThen>` to .NET.
RESOLVED

### ✅ F8. .NET — remove `ScenarioDslFactory.cs`
- Investigation: grep across `system-test/dotnet/**/*.cs` finds only the class definition at `Dsl.Core/Scenario/ScenarioDslFactory.cs:6` — **zero references / instantiations**. Dead code.
- Action: delete the file.
DONE

---

## G. Architecture Layers — Common

### ✅ G4. .NET — keep `ResultTaskExtensions.cs` and `VoidValue.cs` (language-specific; registered as exceptions in the compare-tests agent)
- Files: `system-test/dotnet/Common/ResultTaskExtensions.cs`, `VoidValue.cs`.
- Both are idiomatic .NET helpers (`VoidValue` fills the `Result<T, E>` generic gap for void; `ResultTaskExtensions` enables async composition over `Task<Result<T, E>>`).
- No code change in the .NET source.
- Action: **registered as exceptions in `.claude/agents/compare-tests.md` → Known Language-Specific Divergences → .NET-only.** Future compare-tests runs will not propose porting them to Java/TS.
RESOLVED

### ✅ G5. No .NET action (Java-only utility; registered as an exception in the compare-tests agent)
- `Closer.java` is a Java-side helper for `AutoCloseable.close()` with checked-exception wrapping. .NET uses native `IDisposable` + `using`; no equivalent needed.
- No code change in any language.
- Action: **registered as an exception in `.claude/agents/compare-tests.md` → Known Language-Specific Divergences → Java-only.** Future compare-tests runs will not propose porting `Closer` to .NET or TS.
RESOLVED

---

## H. Architecture Layers — Driver Ports

### ✅ H2. .NET — add `GetCountryRequest`
- File (new): `system-test/dotnet/Driver.Port/External/Tax/Dtos/GetCountryRequest.cs`.
- Reference: Java `GetCountryRequest.java`.
- Why it worked without the DTO: `BaseTaxClient.GetCountryAsync(string? country)` takes a raw `string` and inlines it into the URL (`$"{CountriesEndpoint}/{country}"`). A simple GET with a single path parameter doesn't strictly require a request object — the call compiles and executes fine. The DTO is added for **structural parity with Java**, not because the runtime needs it.
DONE

### ✅ H4. .NET — add `GetPromotionResponse` (TS half split out to a separate plan)
- File (new): `system-test/dotnet/Driver.Port/External/Erp/Dtos/GetPromotionResponse.cs`.
- Reference: Java `GetPromotionResponse.java` (fields: `promotionActive: bool`, `discount: decimal`).
- TypeScript is also missing this DTO (confirmed: zero hits in `system-test/typescript/`), but the TS add belongs in a **TypeScript-scoped plan file** to mirror how we've split per-language plans. Do not do the TS work here.
DONE (.NET); TS tracked separately

---

## P. Legacy Tests — mod05

### ✅ P1. .NET — convert `PlaceOrderNegativeBaseTest` to non-parameterized, single-case, matching Java
- Files:
  - .NET: `system-test/dotnet/SystemTests/Legacy/Mod05/E2eTests/PlaceOrderNegativeBaseTest.cs` — currently `[Theory] + [InlineData("3.5"), InlineData("lala")]` (parameterized, 2 rows).
  - Java: `system-test/java/.../legacy/mod05/e2e/PlaceOrderNegativeBaseTest.java` — `@Test` (plain, not parameterized), hardcoded `"3.5"`.
- Is it parameterized? **.NET yes, Java no.** To reach structural parity with Java (reference):
  - Remove `[InlineData("lala")]`.
  - Replace `[Theory] + [InlineData("3.5")]` with `[Fact]` and hardcode `"3.5"` in the method body.
- Net effect: both languages run a single plain test asserting the `"3.5"` → "Quantity must be an integer" failure path.
DONE

---

## U. Legacy Tests — mod10

### ✅ U1. .NET — add `ShouldRejectOrderWithNonPositiveQuantity` to mod10 acceptance
- File: `system-test/dotnet/SystemTests/Legacy/Mod10/AcceptanceTests/PlaceOrderNegativeTest.cs`.
- Add method parameterized over `"-10"`, `"-1"`, `"0"` asserting field error `quantity / Quantity must be positive`.
- Reference: `system-test/java/.../legacy/mod10/acceptance/PlaceOrderNegativeTest.java` lines with `@ValueSource(strings = {"-10", "-1", "0"})`.
DONE

---

## Local verification & commit

- From `system-test/dotnet/`, run `Run-SystemTests -Architecture monolith` (latest suite) and `Run-SystemTests -Architecture monolith -Legacy` (legacy suite). Do not substitute `dotnet test` — `Run-SystemTests.ps1` is the only supported entry point because it manages containers and config.
- Fix any failures before moving on.
- Commit .NET changes as one logical commit (or a small series if the .NET work groups into distinct concerns such as scenario-DSL cleanup vs. driver-port additions).
