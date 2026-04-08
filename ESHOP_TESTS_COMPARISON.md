# Verbatim Comparison: eshop-tests vs starter (.NET / Java / TypeScript test code)

## File Inventory (.NET)

| Metric | Count |
|--------|-------|
| Files in both repos (identical after whitespace normalization) | 61 |
| Files in both repos (real differences) | 88 |
| Files only in eshop-tests | 44 (mostly legacy module tests) |
| Files only in starter | 12 (promotion + channel mode + new tests) |

---

## 1. Structural Differences

| Aspect | eshop-tests | starter |
|--------|------------|---------|
| Test code path | `dotnet/` | `system-test/dotnet/` |
| System code | `eshop/` (empty) | `system/` (full monolith + multitier in 3 langs) |
| Namespace | `Optivem.EShop.SystemTest` | `Optivem.Shop.SystemTest` |

---

## 2. Functional Differences (starter has, eshop-tests doesn't)

### 2a. Promotion Concept (the known difference)

**New files in starter:**
- `Driver.Port/External/Erp/Dtos/ReturnsPromotionRequest.cs` — `PromotionActive` (bool), `Discount` (string?)
- `Driver.Adapter/External/Erp/Client/Dtos/ExtGetPromotionResponse.cs` — `PromotionActive` (bool), `Discount` (decimal)
- `Dsl.Port/Given/Steps/IGivenPromotion.cs` — `WithActive(bool)`, `WithDiscount(decimal/string)`
- `Dsl.Core/Scenario/Given/Steps/GivenPromotion.cs` — full Given step
- `Dsl.Core/UseCase/External/Erp/UseCases/ReturnsPromotion.cs` — ERP use case

**Modified files:**
- `IErpDriver.cs` — adds `ReturnsPromotionAsync(ReturnsPromotionRequest)`
- `IGivenStage.cs` — adds `Promotion()` method
- `GherkinDefaults.cs` — adds `DefaultPromotionActive = false`, `DefaultPromotionDiscount = "1.00"`, `WeekdayTime`, `WeekendTime`
- `GivenStage.cs` — adds `_promotion` field, `SetupPromotion()` call, `Promotion()` method
- `WhenStage.cs` — adds `hasPromotion` parameter; `EnsureGiven()` sets up promotion BEFORE product
- `ErpDsl.cs` — adds `ReturnsPromotion()` method
- All ERP adapter drivers — add promotion-related methods

**Business logic impact (NEEDS FIX):** Currently starter applies promotionFactor at basePrice level (`basePrice = unitPrice * quantity * promotionFactor`). This is wrong. Promotion discount should be applied at subtotalPrice level, same as coupon discounts. The correct formula should be:
- `basePrice = unitPrice * quantity` (same as eshop-tests)
- `promotionAmount = basePrice * promotionDiscount`
- `couponAmount = basePrice * couponDiscount`
- `subtotalPrice = basePrice - promotionAmount - couponAmount`
- `taxAmount = subtotalPrice * taxRate`
- `totalPrice = subtotalPrice + taxAmount`

**New test scenarios:**
- `PlaceOrderPositiveIsolatedTest.cs` (new file) — `ShouldApplyFullPriceWithoutPromotion()`, `ShouldApplyDiscountWhenPromotionIsActive()`, `ShouldRecordPlacementTimestamp()`

### 2b. ChannelMode

**New files in starter:**
- `Dsl.Port/ChannelMode.cs` — enum: `Dynamic`, `Static`

**Modified files:**
- `Configuration.cs` — adds `channelMode` field and constructor param (default: `Dynamic`); removes unused `using Microsoft.CodeAnalysis.CSharp.Syntax`
- `BaseConfigurableTest.cs` — adds `GetFixedChannelMode()` virtual method
- `PropertyLoader.cs` — adds `GetChannelMode()` reading `CHANNEL_MODE` env var
- `SystemConfigurationLoader.cs` — passes `channelMode` to Configuration
- `WhenBrowseCoupons.cs` — passes `ChannelMode.Dynamic` to `app.Shop()`
- `ShopDsl.cs`, `UseCaseDsl.cs` — accept ChannelMode parameter

### 2c. IAsyncDisposable

Starter adds `IAsyncDisposable` to:
- `IClockDriver`
- `IErpDriver`
- Clock/ERP adapter driver implementations

### 2d. ThenBrowseCoupons

**New files in starter:**
- `Dsl.Port/Then/Steps/IThenBrowseCoupons.cs` — `ContainsCouponWithCode()`, `CouponCount()`, `GetAwaiter()`
- `Dsl.Core/Scenario/Then/Steps/ThenBrowseCoupons.cs` — full implementation

### 2e. December 31st Order Rejection Test

`PlaceOrderNegativeIsolatedTest.cs` adds:
```csharp
ShouldRejectOrderPlacedAtYearEnd() // Clock("2026-12-31T23:59:30Z") -> error
```

### 2f. Clock Isolated Contract Test

**New file:** `ClockStubContractIsolatedTest.cs` — `ShouldBeAbleToGetConfiguredTime()`

### 2g. Test Trait on Isolated Tests

Starter adds `[Trait("Category", "isolated")]` to all `[Collection("Isolated")]` test classes.

---

## 3. Things in eshop-tests but NOT in starter

### 3a. ThenFailureCoupon

**File only in eshop-tests:** `Dsl.Core/Scenario/Then/Steps/ThenFailureCoupon.cs`

### 3b. Legacy Module Tests (44 files)

eshop-tests has additional legacy tests that starter trimmed:
- **Mod02**: `TaxSmokeTest.cs`
- **Mod03**: ViewOrder E2e tests (4), Smoke tests (3)
- **Mod04**: ViewOrder E2e tests (4), `TaxSmokeTest.cs`
- **Mod05**: ViewOrder E2e tests (3), `TaxSmokeTest.cs`
- **Mod06**: ViewOrder E2e tests (2), `SystemErrorAssertExtensions.cs`, `TaxSmokeTest.cs`
- **Mod07**: ViewOrder E2e tests (2), `TaxSmokeTest.cs`
- **Mod08**: ViewOrder E2e tests (2), `TaxSmokeTest.cs`
- **Mod09**: `TaxSmokeTest.cs`
- **Mod10**: CancelOrder tests (4), ViewOrder tests (2), Coupon tests (3)
- **Mod11**: Tax contract tests (3)

---

## 4. Content Differences in Shared Files

### 4a. Legacy Test Method Names and Scenarios

Across Mod02-Mod10, many tests have different names and different test scenarios:

| eshop-tests | starter |
|------------|---------|
| `ShouldPlaceOrderWithCorrectSubtotalPrice` | `ShouldPlaceOrderForValidInput` |
| Has parameterized subtotal price tests | Simpler/fewer test cases |
| `BaseRawTest` has `_taxHttpClient` field | No `_taxHttpClient` field |

### 4b. Legacy Test Structure

- Mod02-05 legacy tests in starter have been refactored (different ERP setup patterns, different assertions)
- Starter's legacy tests create products using raw JSON; eshop-tests uses helper methods
- Starter adds `// GivenStage` comments in some legacy tests

### 4c. Inline Comment Removal

Starter removes inline comments from `ChannelInlineData` attributes:
```csharp
// eshop-tests:
[ChannelInlineData("2024-12-31T22:00:00Z")]   // Start of blackout period
// starter:
[ChannelInlineData("2024-12-31T22:00:00Z")]
```

### 4d. Latest Test Differences (beyond promotion)

- `PlaceOrderNegativeTest.cs` — differs (specific scenario differences TBD)
- `PlaceOrderNegativeIsolatedTest.cs` — starter adds `ShouldRejectOrderPlacedAtYearEnd()`
- `BrowseCouponsPositiveTest.cs` — differs
- `PublishCouponPositiveTest.cs` — differs

### 4e. WireMock/Driver Adapter

- `JsonWireMockClient.cs` — has differences (likely IAsyncDisposable-related)
- `CouponController.cs` (API client) — has differences
- Various shop UI pages — have differences (likely minor)

---

## 5. Summary (.NET)

### Confirmed functional differences (starter has, eshop-tests doesn't):
1. **Promotion** — ERP promotion endpoint, Given().Promotion() DSL, price factor in basePrice calculation
2. **ChannelMode** — Dynamic/Static channel configuration
3. **IAsyncDisposable** — on Clock and ERP driver interfaces
4. **ThenBrowseCoupons** — coupon browsing verification step
5. **Dec 31st order rejection test**
6. **Clock isolated contract test**
7. **Trait annotations** on isolated tests

### eshop-tests has but starter doesn't:
1. **ThenFailureCoupon** class
2. **~44 legacy module test files** (ViewOrder E2e, TaxSmoke, Mod10 CancelOrder/Coupon, Mod11 Tax contract)

### Non-functional differences:
- Namespace: `EShop` -> `Shop`
- ~61 whitespace-only diffs
- Legacy test refactoring (method names, scenario structure, comments)
- Unused import removal in Configuration.cs

---

## 6. Java Comparison

Package: `com.optivem.eshop.dsl.*` (eshop-tests) vs `com.optivem.shop.dsl.*` (starter)

**Same pattern as .NET confirmed:**

| Difference | Status |
|-----------|--------|
| Promotion concept | Same — GivenPromotion, ReturnsPromotion, ScenarioDefaults with PROMOTION_ACTIVE/DISCOUNT |
| ChannelMode (Dynamic/Static) | Same — enum + UseCaseDsl.shop(ChannelMode) overload |
| WeekdayTime/WeekendTime constants | Same |
| @Isolated annotations | Same — includes new ClockStubContractIsolatedTest |
| ThenBrowseCoupons | Partial — interface/impl exist but not integrated into ThenStage |
| Additional test scenarios | starter adds `orderTotalShouldIncludeTax()`, `orderTotalShouldReflectCouponDiscount()`, `orderTotalShouldApplyCouponDiscountAndTax()` |

**Java-specific notes:**
- No IAsyncDisposable equivalent (Java uses try-with-resources, not relevant)
- Dec 31st order rejection test not found in Java (present in .NET only)
- eshop-tests has ~39 more test files due to legacy modules (same pattern as .NET)

---

## 7. TypeScript Comparison

**Major architectural divergence** — TypeScript differs the most between the two repos:

### Structure
| Aspect | eshop-tests | starter |
|--------|------------|---------|
| Architecture | Full hexagonal (dsl-core/dsl-port/driver-port/driver-adapter) | Flat (src/clients, src/drivers, src/dsl) |
| Test framework | **Playwright test** | **Jest** |
| DSL files | ~97 files across layers | Single `scenario-dsl.ts` (1,662 lines) |
| Config | Property-based loader | YAML-based loader |
| Test setup | Fixture-based (`forChannels()`) | Factory function (`createScenario()`) |
| File count | ~390 files | ~89 files |
| Test files | ~105 (.spec.ts) | ~64 (.spec.ts) |

### Feature parity
| Feature | starter | eshop-tests |
|---------|---------|------------|
| Promotion | Has `.promotion()` DSL + ERP endpoint | Missing |
| ChannelMode | `'dynamic'\|'static'` union type | Different — uses channel type abstraction |
| WeekdayTime/WeekendTime | Has + `.withWeekday()/.withWeekend()` | Missing |
| ThenBrowseCoupons | Has | Has |
| ThenFailureCoupon | Missing | Has |
| Dec 31st blackout test | Has | Has |
| Clock isolated contract | Has | Has |
| @isolated tags | No | `test.describe('@isolated', ...)` |
| Serial mode | No | `test.describe.configure({ mode: 'serial' })` |

### Key architectural differences

**eshop-tests TypeScript** (hexagonal, modular):
- 97 separate files organized into layers (dsl-core/dsl-port/driver-port/driver-adapter)
- Each Given/When/Then builder is its own file with single responsibility
- Deferred execution: `ThenSuccessVerifier` implements `PromiseLike<void>` — assertions collected, executed on `await`
- Uses Playwright test framework with custom fixtures
- `forChannels()` wrapper runs single test for both UI and API channels

**starter TypeScript** (flat, pragmatic):
- Entire DSL in single `scenario-dsl.ts` (1,662 lines, 50+ inner classes)
- Eager execution: setup runs when `then()` is called
- Uses Jest with manual `createScenario()` factory per test
- Manual `try/finally` cleanup instead of fixture teardown
- No base classes, no port/adapter separation

---

## 8. Cross-Language Summary

| Difference | .NET | Java | TypeScript |
|-----------|------|------|-----------|
| Promotion | starter-only | starter-only | starter-only |
| ChannelMode | starter-only | starter-only | starter-only (different impl) |
| IAsyncDisposable | starter-only | N/A | N/A |
| WeekdayTime/WeekendTime | starter-only | starter-only | starter-only |
| ThenBrowseCoupons | starter-only | partial | Both have it |
| ThenFailureCoupon | eshop-only | ? | eshop-only |
| Dec 31st rejection test | starter-only | neither | Both have it |
| Isolated test annotations | starter adds Trait | starter adds more | eshop-tests has more |
| Additional test scenarios | starter adds | starter adds 3 | varies |
| Namespace | EShop -> Shop | eshop -> shop | same pattern |
| Architecture | Same pattern | Same pattern | **Major divergence** (hexagonal vs flat) |
| Test framework | Both xUnit | Both JUnit | **Playwright vs Jest** |
| Extra legacy tests | ~44 in eshop | ~39 in eshop | ~41 in eshop |

---

## 9. Migration Plan: Make starter the superset

Goal: starter should have everything eshop-tests has, plus keep everything better that starter already has.

### 9a. Fix: Promotion pricing logic (all 3 languages)

Promotion discount is currently applied at basePrice level. It should be at subtotalPrice level (same as coupon discount).

**Current (wrong):** `basePrice = unitPrice * quantity * promotionFactor`
**Correct:** `basePrice = unitPrice * quantity`, then `subtotalPrice = basePrice - promotionAmount - couponAmount`

Affected: system code (all 3 langs), test assertions, DSL defaults.

### 9b. Bring from eshop-tests: ThenFailureCoupon (all 3 languages)

Add the missing `ThenFailureCoupon` class to starter's DSL.

**.NET:** Add `Dsl.Core/Scenario/Then/Steps/ThenFailureCoupon.cs`
**Java/TypeScript:** Add equivalent.

### 9c. Bring from eshop-tests: Missing legacy module tests (all 3 languages)

Add the ~44 (.NET), ~39 (Java), ~41 (TypeScript) legacy test files that eshop-tests has but starter trimmed:

- ViewOrder E2e tests (Mod03-Mod08)
- TaxSmokeTest (Mod02-Mod09)
- Mod03 Smoke tests (ErpSmokeTest, ShopApiSmokeTest, ShopUiSmokeTest)
- Mod10: CancelOrder tests (4), ViewOrder tests (2), Coupon tests (3)
- Mod11: Tax contract tests (3)
- Mod06: SystemErrorAssertExtensions

### 9d. Keep in starter (already better than eshop-tests)

These features exist in starter but not eshop-tests. Keep them:

1. **Promotion concept** (after pricing fix in 9a)
2. **ChannelMode** (Dynamic/Static)
3. **IAsyncDisposable** (.NET only)
4. **ThenBrowseCoupons** verification step
5. **Dec 31st order rejection test**
6. **Clock isolated contract test**
7. **Trait annotations** on isolated tests
8. **WeekdayTime/WeekendTime** time constants
9. **Additional test scenarios** (e.g. Java's tax+coupon interaction tests)

### 9e. TypeScript: Refactor monolithic DSL to hexagonal architecture

Refactor `scenario-dsl.ts` (1,662 lines, 50+ inner classes) into multi-file hexagonal architecture matching eshop-tests and matching .NET/Java's structure in starter.

**Target structure** (mirroring eshop-tests TypeScript):

```
src/
├── common/                          # Keep existing (dtos.ts, result.ts)
├── channel/
│   └── channel-type.ts
├── dsl-port/                        # DSL interfaces (contracts)
│   ├── scenario-root-port.ts
│   ├── scenario/
│   │   ├── assume/assume-stage-port.ts
│   │   ├── given/given-stage-port.ts
│   │   ├── when/when-stage-port.ts
│   │   └── then/then-result-port.ts
│   └── types.ts
├── driver-port/                     # Driver interfaces (keep existing types.ts, expand)
│   ├── shop/
│   │   ├── shop-driver.ts
│   │   └── dtos/  (PlaceOrderRequest, ViewOrderResponse, etc.)
│   └── external/
│       ├── erp/  (ErpDriver + DTOs)
│       ├── clock/  (ClockDriver + DTOs)
│       └── tax/  (TaxDriver + DTOs)
├── driver-adapter/                  # Keep existing drivers, reorganize
│   ├── shop/
│   │   ├── api/shop-api-driver.ts
│   │   └── ui/shop-ui-driver.ts
│   └── external/
│       ├── erp/  (erp-real-driver.ts, erp-stub-driver.ts)
│       ├── clock/  (clock-real-driver.ts, clock-stub-driver.ts)
│       └── tax/  (tax-real-driver.ts, tax-stub-driver.ts)
├── dsl-core/                        # Split scenario-dsl.ts into these
│   ├── scenario/
│   │   ├── scenario-dsl.ts          # Entry point (assume/given/when/then)
│   │   ├── gherkin-defaults.ts      # Move from defaults.ts
│   │   ├── execution-result.ts
│   │   ├── given/
│   │   │   ├── given-stage.ts       # GivenStage orchestrator
│   │   │   ├── base-given-step.ts
│   │   │   ├── given-product.ts
│   │   │   ├── given-promotion.ts
│   │   │   ├── given-coupon.ts
│   │   │   ├── given-country.ts
│   │   │   ├── given-clock.ts
│   │   │   └── given-order.ts
│   │   ├── when/
│   │   │   ├── when-stage.ts        # WhenStage orchestrator
│   │   │   ├── base-when-step.ts
│   │   │   ├── when-place-order.ts
│   │   │   ├── when-cancel-order.ts
│   │   │   ├── when-view-order.ts
│   │   │   ├── when-publish-coupon.ts
│   │   │   ├── when-browse-coupons.ts
│   │   │   └── when-go-to-shop.ts
│   │   ├── then/
│   │   │   ├── then-result-stage.ts
│   │   │   ├── then-success-order.ts
│   │   │   ├── then-failure-order.ts
│   │   │   ├── then-success-coupon.ts
│   │   │   ├── then-failure-coupon.ts  # NEW (from eshop-tests)
│   │   │   ├── then-browse-coupons.ts
│   │   │   ├── then-contract-stage.ts
│   │   │   └── then-given-stage.ts
│   │   └── assume/
│   │       └── assume-stage.ts
│   ├── usecase/
│   │   ├── usecase-dsl.ts           # Factory for shop/erp/tax/clock DSLs
│   │   ├── configuration.ts
│   │   ├── shop/
│   │   │   ├── shop-dsl.ts
│   │   │   └── usecases/  (place-order, view-order, cancel-order, etc.)
│   │   └── external/
│   │       ├── erp/  (erp-dsl, returns-product, returns-promotion, get-product)
│   │       ├── clock/  (clock-dsl, returns-time, get-time)
│   │       └── tax/  (tax-dsl, returns-tax-rate, get-tax-rate)
│   └── shared/
│       ├── base-usecase.ts
│       ├── usecase-context.ts
│       └── response-verification.ts
├── clients/                         # Keep existing (http-client.ts, wiremock-client.ts)
├── config/                          # Keep existing
└── test-setup.ts                    # Keep, update imports
```

**Key decisions:**
- Keep Jest (not switch to Playwright) — Jest is simpler for students
- Keep `createScenario()` factory pattern — simpler than fixture-based setup
- Extract classes from monolithic file into individual files with proper imports
- Add PromiseLike deferred execution pattern from eshop-tests (collect assertions, execute on await)
- Add base classes (BaseGivenStep, BaseWhenStep) for code reuse
- Preserve all starter-only features (promotion, ChannelMode, etc.)

### 9f. Backport starter improvements to eshop-tests (optional)

After starter is the superset, backport these features to eshop-tests:
1. Promotion concept
2. ChannelMode
3. IAsyncDisposable (.NET)
4. ThenBrowseCoupons
5. Additional test scenarios
6. WeekdayTime/WeekendTime constants
7. Dec 31st / Clock isolated tests

This makes eshop-tests a subset of starter (same features, minus system code).
