# System Tests & Architecture Comparison Report

Reference implementation: **Java**. All differences should be aligned to Java unless noted otherwise.

---

## 1. LATEST TESTS COMPARISON

### 1.1 Smoke Tests

| Test File | Java | .NET | TypeScript | Status |
|-----------|------|------|------------|--------|
| ClockSmokeTest | `shouldBeAbleToGoToClock` | `ShouldBeAbleToGoToClock` | `shouldBeAbleToGoToClock` | ALIGNED |
| ErpSmokeTest | `shouldBeAbleToGoToErp` | `ShouldBeAbleToGoToErp` | `shouldBeAbleToGoToErp` | ALIGNED |
| TaxSmokeTest | `shouldBeAbleToGoToTax` | `ShouldBeAbleToGoToTax` | `shouldBeAbleToGoToTax` | ALIGNED |
| ShopSmokeTest | `shouldBeAbleToGoToShop` @Channel(UI, API) | `ShouldBeAbleToGoToShop` @ChannelData(UI, API) | `shouldBeAbleToGoToShop` forChannels(ui, api) | ALIGNED |

**Smoke tests: FULLY ALIGNED across all 3 languages.**

---

### 1.2 E2E Tests

| Test File | Java | .NET | TypeScript | Status |
|-----------|------|------|------------|--------|
| PlaceOrderPositiveTest | `shouldPlaceOrder` @Channel(UI, API) | `ShouldPlaceOrder` @ChannelData(UI, API) | `shouldPlaceOrder` forChannels(ui, api) | ALIGNED |

**E2E tests: FULLY ALIGNED across all 3 languages.**

---

### 1.3 Contract Tests

| Test File | Java | .NET | TypeScript | Status |
|-----------|------|------|------------|--------|
| BaseClockContractTest | `shouldBeAbleToGetTime` | `ShouldBeAbleToGetTime` | N/A (no base file) | SEE BELOW |
| ClockRealContractTest | inherits `shouldBeAbleToGetTime` from base | inherits from base | `shouldBeAbleToGetTime` | ALIGNED |
| ClockStubContractTest | inherits `shouldBeAbleToGetTime` from base | inherits from base | **`shouldBeAbleToGetTime` + `shouldBeAbleToGetConfiguredTime`** | **DIFF-TS-1** |
| ClockStubContractIsolatedTest | `shouldBeAbleToGetConfiguredTime` | `ShouldBeAbleToGetConfiguredTime` | `shouldBeAbleToGetTime` + `shouldBeAbleToGetConfiguredTime` | **DIFF-TS-2** |
| BaseErpContractTest | `shouldBeAbleToGetProduct` | `ShouldBeAbleToGetProduct` | N/A | ALIGNED |
| ErpRealContractTest | inherits from base | inherits from base | `shouldBeAbleToGetProduct` | ALIGNED |
| ErpStubContractTest | inherits from base | inherits from base | `shouldBeAbleToGetProduct` | ALIGNED |
| BaseTaxContractTest | `shouldBeAbleToGetTaxRate` | `ShouldBeAbleToGetTaxRate` | N/A | ALIGNED |
| TaxRealContractTest | inherits from base | inherits from base | `shouldBeAbleToGetTaxRate` | ALIGNED |
| TaxStubContractTest | inherits `shouldBeAbleToGetTaxRate` + `shouldBeAbleToGetConfiguredTaxRate` | inherits + `ShouldBeAbleToGetConfiguredTaxRate` | `shouldBeAbleToGetTaxRate` + `shouldBeAbleToGetConfiguredTaxRate` | ALIGNED |

#### Contract Test Differences

**DIFF-TS-1: TS `clock-stub-contract-test.spec.ts` has extra test `shouldBeAbleToGetConfiguredTime`**
- Java `ClockStubContractTest` only inherits `shouldBeAbleToGetTime` from the base class. It does NOT have `shouldBeAbleToGetConfiguredTime`.
- TS adds `shouldBeAbleToGetConfiguredTime` in the non-isolated stub contract test file.
- **Action:** Remove `shouldBeAbleToGetConfiguredTime` from TS `clock-stub-contract-test.spec.ts` to match Java.

**DIFF-TS-2: TS `clock-stub-contract-isolated-test.spec.ts` has extra test `shouldBeAbleToGetTime`**
- Java `ClockStubContractIsolatedTest` only has `shouldBeAbleToGetConfiguredTime`.
- TS adds `shouldBeAbleToGetTime` in the isolated stub contract test.
- **Action:** Remove `shouldBeAbleToGetTime` from TS `clock-stub-contract-isolated-test.spec.ts` to match Java.

---

### 1.4 Acceptance Tests

#### 1.4.1 BrowseCouponsPositiveTest

| Method | Java | .NET | TypeScript | Status |
|--------|------|------|------------|--------|
| `shouldBeAbleToBrowseCoupons` | @Channel(UI, API) | @ChannelData(UI, API) | forChannels(ui, api) | ALIGNED |

#### 1.4.2 CancelOrderNegativeIsolatedTest

| Method | Java | .NET | TypeScript | Status |
|--------|------|------|------------|--------|
| `cannotCancelAnOrderOn31stDecBetween2200And2230` | @Isolated, @TimeDependent, @Channel(API, alsoForFirstRow=UI), 5 @DataSource | @Collection("Isolated"), [Time], @ChannelData(API, AlsoForFirstRow=UI), 5 @ChannelInlineData | @isolated serial, forChannels(ui, api), forEach 5 times | **DIFF-TS-3** |

**DIFF-TS-3: TS channel strategy differs from Java**
- Java uses `@Channel(value = {ChannelType.API}, alsoForFirstRow = ChannelType.UI)` meaning first data row runs on both UI+API, remaining rows only API.
- .NET matches Java with `AlsoForFirstRow = new[] { ChannelType.UI }`.
- TS uses `forChannels('ui', 'api')` which runs ALL data rows on BOTH channels. This over-tests compared to Java.
- **Action:** TS should implement `alsoForFirstRow` pattern to match Java behavior.

#### 1.4.3 CancelOrderNegativeTest

| Method | Java | .NET | TypeScript | Status |
|--------|------|------|------------|--------|
| `shouldNotCancelNonExistentOrder` | @Channel(API), 3 @DataSource | @ChannelData(API), 3 @ChannelInlineData | forChannels(api), test.each(3 cases) | ALIGNED |
| `shouldNotCancelAlreadyCancelledOrder` | @Channel(API) | @ChannelData(API) | forChannels(api) | ALIGNED |
| `cannotCancelNonExistentOrder` | @Channel(API) | @ChannelData(API) | forChannels(api) | ALIGNED |

#### 1.4.4 CancelOrderPositiveIsolatedTest

| Method | Java | .NET | TypeScript | Status |
|--------|------|------|------------|--------|
| `shouldBeAbleToCancelOrderOutsideOfBlackoutPeriod...` | @Isolated, @TimeDependent, @Channel(API, alsoForFirstRow=UI), 4 @DataSource | @Collection("Isolated"), [Time], @ChannelData(API, AlsoForFirstRow=UI), 4 @ChannelInlineData | @isolated serial, forChannels(ui, api), forEach 4 times | **DIFF-TS-3** (same issue as above) |

#### 1.4.5 CancelOrderPositiveTest

| Method | Java | .NET | TypeScript | Status |
|--------|------|------|------------|--------|
| `shouldHaveCancelledStatusWhenCancelled` | @Channel(UI, API) | @ChannelData(UI, API) | forChannels(ui, api) | ALIGNED |

#### 1.4.6 PlaceOrderNegativeIsolatedTest

| Method | Java | .NET | TypeScript | Status |
|--------|------|------|------------|--------|
| `shouldRejectOrderPlacedAtYearEnd` | @Isolated, @Channel(UI, API), no @TimeDependent | @Collection("Isolated"), @ChannelData(UI, API), [Time] but no @TimeDependent equivalent | @isolated serial, forChannels(ui, api) | ALIGNED |
| `cannotPlaceOrderWithExpiredCoupon` | @Isolated, @TimeDependent, @Channel(UI, API) | @Collection("Isolated"), [Time], @ChannelData(UI, API) | @isolated serial, forChannels(ui, api) | ALIGNED |

#### 1.4.7 PlaceOrderNegativeTest

| Method | Java | .NET | TypeScript | Status |
|--------|------|------|------------|--------|
| `shouldRejectOrderWithInvalidQuantity` | @Channel(UI, API) | @ChannelData(UI, API) | forChannels(ui, api) | ALIGNED |
| `shouldRejectOrderWithNonExistentSku` | @Channel(UI, API) | @ChannelData(UI, API) | forChannels(ui, api) - adds `.withQuantity(1)` | **DIFF-TS-4** |
| `shouldRejectOrderWithNegativeQuantity` | @Channel(UI, API) | @ChannelData(UI, API) | N/A (merged into nonPositiveQuantities) | SEE BELOW |
| `shouldRejectOrderWithZeroQuantity` | @Channel(UI, API) | @ChannelData(UI, API) | N/A (merged into nonPositiveQuantities) | SEE BELOW |
| `shouldRejectOrderWithEmptySku` | @Channel(API, alsoForFirstRow=UI) | @ChannelData(API, AlsoForFirstRow=UI) | forChannels(ui, api) - adds `.withQuantity(1)` | **DIFF-TS-4, DIFF-TS-3** |
| `shouldRejectOrderWithEmptyQuantity` | @Channel(API, alsoForFirstRow=UI) | @ChannelData(API, AlsoForFirstRow=UI) | forChannels(ui, api) | **DIFF-TS-3** |
| `shouldRejectOrderWithNonIntegerQuantity` | @Channel(API, alsoForFirstRow=UI), @ValueSource("3.5", "lala") | @ChannelData(API, AlsoForFirstRow=UI), 2 @ChannelInlineData | forChannels(ui, api), forEach 2 | **DIFF-TS-3** |
| `shouldRejectOrderWithEmptyCountry` | @Channel(API, alsoForFirstRow=UI) | @ChannelData(API, AlsoForFirstRow=UI) | forChannels(ui, api) - adds `.withQuantity(1)` | **DIFF-TS-3, DIFF-TS-4** |
| `shouldRejectOrderWithInvalidCountry` | @Channel(UI, API) | @ChannelData(UI, API) | forChannels(ui, api) - adds `.withQuantity(1)` | **DIFF-TS-4** |
| `shouldRejectOrderWithNullQuantity` | @Channel(API) | @ChannelData(API) | forChannels(api) | ALIGNED |
| `shouldRejectOrderWithNullSku` | @Channel(API) | @ChannelData(API) | forChannels(api) | ALIGNED |
| `shouldRejectOrderWithNullCountry` | @Channel(API) | @ChannelData(API) | forChannels(api) | ALIGNED |
| `cannotPlaceOrderWithNonExistentCoupon` | @Channel(UI, API) | @ChannelData(UI, API) | forChannels(ui, api) - adds `.withQuantity(1)` | **DIFF-TS-4** |
| `cannotPlaceOrderWithCouponThatHasExceededUsageLimit` | @Channel(UI, API) | @ChannelData(UI, API) | forChannels(ui, api) | ALIGNED |
| `ShouldRejectOrderWithNonPositiveQuantity` | N/A | @ChannelData(API, AlsoForFirstRow=UI), 3 @ChannelInlineData("-10", "-1", "0") | forChannels(ui, api), forEach 3 | **DIFF-NET-1** |

**DIFF-TS-4: TS tests add `.withQuantity(1)` that Java does not have**
- Multiple TS tests add an explicit `.withQuantity(1)` call that is not present in Java.
- This may be necessary to avoid default quantity being invalid, but it diverges from Java's test logic.
- **Action:** Remove `.withQuantity(1)` from TS tests to match Java, or verify if it's needed for TS-specific reasons.

**DIFF-NET-1: .NET has extra test `ShouldRejectOrderWithNonPositiveQuantity`**
- Java has `shouldRejectOrderWithNegativeQuantity` (with `withQuantity(-10)`) and `shouldRejectOrderWithZeroQuantity` (with `withQuantity(0)`) as separate tests.
- .NET has those same two tests PLUS an additional `ShouldRejectOrderWithNonPositiveQuantity` with 3 data rows ("-10", "-1", "0"). This is partially redundant.
- TS replaces the individual negative/zero tests with a single `shouldRejectOrderWithNonPositiveQuantity` loop (matching .NET's extra test).
- **Action:** Remove `ShouldRejectOrderWithNonPositiveQuantity` from .NET. TS should add back `shouldRejectOrderWithNegativeQuantity` and `shouldRejectOrderWithZeroQuantity` as separate tests and remove the `nonPositiveQuantities` loop.

#### 1.4.8 PlaceOrderPositiveIsolatedTest

| Method | Java | .NET | TypeScript | Status |
|--------|------|------|------------|--------|
| `shouldRecordPlacementTimestamp` | @Isolated, @Channel(UI, API) | @Collection("Isolated"), @ChannelData(UI, API) | @isolated serial, forChannels(ui, api) | ALIGNED |
| `shouldApplyFullPriceWithoutPromotion` | @Isolated, @Channel(UI, API) | @Collection("Isolated"), @ChannelData(UI, API) | @isolated serial, forChannels(ui, api) | ALIGNED |
| `shouldApplyDiscountWhenPromotionIsActive` | @Isolated, @Channel(UI, API) | @Collection("Isolated"), @ChannelData(UI, API) | @isolated serial, forChannels(ui, api) | ALIGNED |

#### 1.4.9 PlaceOrderPositiveTest

| Method | Java | .NET | TypeScript | Status |
|--------|------|------|------------|--------|
| `shouldBeAbleToPlaceOrderForValidInput` | @Channel(UI, API) | @ChannelData(UI, API) | forChannels(ui, api) | ALIGNED |
| `orderStatusShouldBePlacedAfterPlacingOrder` | @Channel(UI, API) | @ChannelData(UI, API) | forChannels(ui, api) | ALIGNED |
| `shouldCalculateBasePriceAsProductOfUnitPriceAndQuantity` | @Channel(UI, API) | @ChannelData(UI, API) | forChannels(ui, api) | ALIGNED |
| `shouldPlaceOrderWithCorrectBasePriceParameterized` | @Channel(API, alsoForFirstRow=UI), 4 @DataSource | @ChannelData(API, AlsoForFirstRow=UI), 4 @ChannelInlineData | forChannels(ui, api), test.each(4) | **DIFF-TS-3** |
| `orderPrefixShouldBeORD` | @Channel(UI, API) | @ChannelData(UI, API) | forChannels(ui, api) | ALIGNED |
| `discountRateShouldBeAppliedForCoupon` | @Channel(UI, API) | @ChannelData(UI, API) | forChannels(ui, api) | ALIGNED |
| `discountRateShouldBeNotAppliedWhenThereIsNoCoupon` | @Channel(UI, API) - `.withCouponCode(null)` | @ChannelData(UI, API) - `.WithCouponCode(null)` | forChannels(ui, api) - no `.withCouponCode(null)` call, just `.placeOrder()` | **DIFF-TS-5** |
| `subtotalPriceShouldBeCalculatedAsThe...WhenWeHaveCoupon` | @Channel(UI, API) - `.withCouponCode()` (no arg) | @ChannelData(UI, API) - `.WithCouponCode()` | forChannels(ui, api) - `.withCouponCode(code)` (explicit code) | **DIFF-TS-6** |
| `subtotalPriceShouldBeSameAsBasePriceWhenNoCoupon` | @Channel(UI, API) | @ChannelData(UI, API) | forChannels(ui, api) | ALIGNED |
| `correctTaxRateShouldBeUsedBasedOnCountry` | @Channel(API, alsoForFirstRow=UI), 2 @DataSource | @ChannelData(API, AlsoForFirstRow=UI), 2 @ChannelInlineData | forChannels(ui, api), test.each(2) | **DIFF-TS-3** |
| `totalPriceShouldBeSubtotalPricePlusTaxAmount` | @Channel(API, alsoForFirstRow=UI), 2 @DataSource | @ChannelData(API, AlsoForFirstRow=UI), 2 @ChannelInlineData | forChannels(ui, api), test.each(2) | **DIFF-TS-3** |
| `couponUsageCountHasBeenIncrementedAfterItsBeenUsed` | @Channel(UI, API) | @ChannelData(UI, API) | forChannels(ui, api) | ALIGNED |
| `orderTotalShouldIncludeTax` | @Channel(API) | @ChannelData(API) | forChannels(api) | ALIGNED |
| `orderTotalShouldReflectCouponDiscount` | @Channel(API) | @ChannelData(API) | forChannels(api) | ALIGNED |
| `orderTotalShouldApplyCouponDiscountAndTax` | @Channel(API) | @ChannelData(API) | forChannels(api) | ALIGNED |

**DIFF-TS-5: TS `discountRateShouldBeNotAppliedWhenThereIsNoCoupon` omits `.withCouponCode(null)`**
- Java explicitly calls `.withCouponCode(null)` in the when clause.
- TS just calls `.placeOrder()` without specifying the coupon code.
- **Action:** Add `.withCouponCode(null)` call to TS test.

**DIFF-TS-6: TS `subtotalPriceShouldBeCalculated...` uses explicit coupon code instead of default**
- Java calls `.withCouponCode()` (no argument) which uses the default coupon code from the given clause.
- TS explicitly passes the coupon code variable instead.
- This is a minor stylistic difference but should be aligned.
- **Action:** Align TS to use `.withCouponCode()` (no arg) to match Java.

#### 1.4.10 PublishCouponNegativeTest

| Method | Java | .NET | TypeScript | Status |
|--------|------|------|------------|--------|
| `cannotPublishCouponWithZeroOrNegativeDiscount` | @Channel(API, alsoForFirstRow=UI), @ValueSource("0.0", "-0.01", "-0.15") | @ChannelData(API, AlsoForFirstRow=UI), 3 @ChannelInlineData | forChannels(ui, api), forEach 3 | **DIFF-TS-3** |
| `cannotPublishCouponWithDiscountGreaterThan100percent` | @Channel(API, alsoForFirstRow=UI), @ValueSource("1.01", "2.00") | @ChannelData(API, AlsoForFirstRow=UI), 2 @ChannelInlineData | forChannels(ui, api), forEach 2 | **DIFF-TS-3** |
| `cannotPublishCouponWithDuplicateCouponCode` | @Channel(UI, API) | @ChannelData(UI, API) | forChannels(ui, api) | ALIGNED |
| `cannotPublishCouponWithZeroOrNegativeUsageLimit` | @Channel(API, alsoForFirstRow=UI), @ValueSource("0", "-1", "-100") | @ChannelData(API, AlsoForFirstRow=UI), 3 @ChannelInlineData | forChannels(ui, api), forEach 3 | **DIFF-TS-3** |
| `shouldRejectCouponWithBlankCode` | @Channel(API) | @ChannelData(API) | forChannels(api) | ALIGNED |

#### 1.4.11 PublishCouponPositiveTest

| Method | Java | .NET | TypeScript | Status |
|--------|------|------|------------|--------|
| `shouldBeAbleToPublishValidCoupon` | @Channel(UI, API) | @ChannelData(UI, API) | forChannels(ui, api) | ALIGNED |
| `shouldBeAbleToPublishCouponWithEmptyOptionalFields` | @Channel(UI, API), `.withValidFrom(null).withValidTo(null).withUsageLimit(null)` | @ChannelData(UI, API), same | forChannels(ui, api), uses `undefined` instead of `null` | **DIFF-TS-7** |
| `shouldBeAbleToCorrectlySaveCoupon` | @Channel(UI, API) | @ChannelData(UI, API) | forChannels(ui, api) | ALIGNED |
| `shouldPublishCouponSuccessfully` | @Channel(API) | @ChannelData(API) | forChannels(api) | ALIGNED |

**DIFF-TS-7: TS uses `undefined` instead of `null` for optional fields**
- Java and .NET explicitly pass `null` for optional fields (validFrom, validTo, usageLimit).
- TS uses `undefined`.
- This is a TS idiom (undefined vs null) and may be functionally equivalent, but should be verified.
- **Action:** Verify if `undefined` and `null` are handled identically by the TS DSL. If not, use `null`.

#### 1.4.12 ViewOrderNegativeTest

| Method | Java | .NET | TypeScript | Status |
|--------|------|------|------------|--------|
| `shouldNotBeAbleToViewNonExistentOrder` | @Channel(API, alsoForFirstRow=UI), @MethodSource(3 values) | @ChannelData(API, AlsoForFirstRow=UI), @ChannelMemberData(3 values) | forChannels(api), test.each(3) | **DIFF-TS-3, DIFF-TS-8** |

**DIFF-TS-8: TS ViewOrderNegativeTest uses `forChannels('api')` only, missing `alsoForFirstRow=UI`**
- Java runs first row on both UI and API.
- TS only runs on API.
- **Action:** Align TS to match Java's `alsoForFirstRow` behavior.

#### 1.4.13 ViewOrderPositiveTest

| Method | Java | .NET | TypeScript | Status |
|--------|------|------|------------|--------|
| `shouldBeAbleToViewOrder` | @Channel(UI, API) | @ChannelData(UI, API) | forChannels(ui, api) | ALIGNED |

---

## 2. LEGACY TESTS COMPARISON

### 2.1 Abstraction Layer Check

| Module | Expected Layer | Java Base Class | .NET Base Class | TypeScript Fixture | TS Layer Match |
|--------|---------------|-----------------|-----------------|-------------------|---------------|
| mod02 | Raw | BaseRawTest | BaseRawTest | @playwright/test (raw imports) | YES |
| mod03 | Raw | BaseRawTest | BaseRawTest | **withApp() -> Scenario DSL** | **NO** |
| mod04 | Client | BaseClientTest | BaseClientTest | **withApp() -> Scenario DSL** | **NO** |
| mod05 | Driver | BaseDriverTest | BaseDriverTest | **withApp() -> Scenario DSL** | **NO** |
| mod06 | Channel Driver | BaseChannelDriverTest | BaseChannelDriverTest | **withApp() -> Scenario DSL** | **NO** |
| mod07 | Use-Case DSL | BaseUseCaseDslTest | BaseUseCaseDslTest | **withApp() -> Scenario DSL** | **NO** |
| mod08 | Scenario DSL | BaseScenarioDslTest | BaseScenarioDslTest | withApp() -> Scenario DSL | YES |
| mod09 | Scenario DSL + Clock | BaseScenarioDslTest | BaseScenarioDslTest | withApp() -> Scenario DSL | YES |
| mod10 | Scenario DSL + Isolated | BaseScenarioDslTest | BaseScenarioDslTest | withApp() -> Scenario DSL | YES |
| mod11 | Scenario DSL + Contract | BaseScenarioDslTest | BaseScenarioDslTest | withApp() -> Scenario DSL | YES |

**CRITICAL FINDING (DIFF-LEGACY-1): TS legacy modules mod03-mod07 all use Scenario DSL level (via `withApp()`) instead of the expected abstraction layers (Raw, Client, Driver, Channel Driver, Use-Case DSL).**

- Java and .NET properly implement incremental abstraction layers per module.
- TS skips all intermediate layers and jumps straight to Scenario DSL.
- This defeats the pedagogical purpose of showing the progression of abstraction layers.
- **Action:** TS mod03-mod07 need to be rewritten to use the correct abstraction layers matching Java.

### 2.2 File Structure Differences

#### mod02: Smoke Tests Only

| File | Java | .NET | TypeScript | Status |
|------|------|------|------------|--------|
| ErpSmokeTest | raw HTTP | raw HTTP | raw fetch | ALIGNED |
| TaxSmokeTest | raw HTTP | raw HTTP | raw fetch | ALIGNED |
| ShopApiSmokeTest | raw HTTP | raw HTTP | raw fetch | ALIGNED |
| ShopUiSmokeTest | raw Playwright | raw Playwright | raw Playwright | ALIGNED |

#### mod03-mod05: E2E Tests (separate API/UI files)

| Pattern | Java | .NET | TypeScript | Status |
|---------|------|------|------------|--------|
| PlaceOrderPositiveApiTest | YES | YES | N/A | **DIFF-LEGACY-2** |
| PlaceOrderPositiveUiTest | YES | YES | N/A | **DIFF-LEGACY-2** |
| PlaceOrderNegativeApiTest | YES | YES | N/A | **DIFF-LEGACY-2** |
| PlaceOrderNegativeUiTest | YES | YES | N/A | **DIFF-LEGACY-2** |
| place-order-positive-test.spec.ts | N/A | N/A | YES (merged) | **DIFF-LEGACY-2** |
| place-order-negative-test.spec.ts | N/A | N/A | YES (merged) | **DIFF-LEGACY-2** |

**DIFF-LEGACY-2: TS mod03-mod05 merge API and UI tests into single files**
- Java and .NET have separate test files for API vs UI (e.g., `PlaceOrderPositiveApiTest` + `PlaceOrderPositiveUiTest`).
- TS uses channel parameterization (`forChannels('ui', 'api')`) in a single file.
- For mod03-mod05 (Raw/Client/Driver layers), the Java/NET approach uses separate files because the raw implementation code differs significantly between API and UI.
- TS can't replicate this because it uses Scenario DSL which abstracts away the API/UI difference.
- **Action:** This is a consequence of DIFF-LEGACY-1. Once TS implements proper abstraction layers, separate API/UI test files will be needed for mod03-mod05.

#### mod04-mod05: Smoke Tests

| Pattern | Java | .NET | TypeScript | Status |
|---------|------|------|------------|--------|
| mod04 ShopApiSmokeTest + ShopUiSmokeTest | YES (2 files) | YES (2 files) | **shop-smoke-test.spec.ts** (1 merged file using DSL) | **DIFF-LEGACY-3** |
| mod05 ShopApiSmokeTest + ShopUiSmokeTest + ShopBaseSmokeTest | YES (3 files) | YES (3 files) | **shop-smoke-test.spec.ts** (1 merged file using DSL) | **DIFF-LEGACY-3** |

**DIFF-LEGACY-3: TS mod04-mod05 smoke tests use merged DSL files instead of separate raw/client files**
- Same root cause as DIFF-LEGACY-1 and DIFF-LEGACY-2.

#### mod06-mod08: Merged Test Files

| File | Java | .NET | TypeScript | Status |
|------|------|------|------------|--------|
| PlaceOrderPositiveTest / place-order-positive-test | YES | YES | YES | ALIGNED (structure) |
| PlaceOrderNegativeTest / place-order-negative-test | YES | YES | YES | ALIGNED (structure) |
| Smoke tests (ErpSmokeTest, TaxSmokeTest, ShopSmokeTest) | YES | YES | YES | ALIGNED (structure) |

#### mod09: Smoke Tests + Clock

| File | Java | .NET | TypeScript | Status |
|------|------|------|------------|--------|
| ClockSmokeTest | YES | YES | YES | ALIGNED |
| ErpSmokeTest | YES | YES | YES | ALIGNED |
| TaxSmokeTest | YES | YES | YES | ALIGNED |
| ShopSmokeTest | YES | YES | YES | ALIGNED |

#### mod10: Acceptance Tests

| File | Java | .NET | TypeScript | Status |
|------|------|------|------------|--------|
| PlaceOrderPositiveTest | YES (2 methods) | YES (2 methods) | YES (2 tests) | ALIGNED |
| PlaceOrderNegativeTest | YES (6 methods) | YES (6 methods) | YES (6 test groups) | ALIGNED |
| PlaceOrderPositiveIsolatedTest | YES (3 methods) | YES (3 methods) | YES (3 tests) | ALIGNED |
| PlaceOrderNegativeIsolatedTest | YES (1 method) | YES (1 method) | YES (1 test) | ALIGNED |

**mod10 test content: FULLY ALIGNED across all 3 languages (test names, assertions, logic).**

#### mod11: Contract + E2E Tests

| File | Java | .NET | TypeScript | Status |
|------|------|------|------------|--------|
| PlaceOrderPositiveTest (E2E) | YES | YES | YES | ALIGNED |
| ClockRealContractTest | YES | YES | YES | ALIGNED |
| ClockStubContractTest | YES (inherits `shouldBeAbleToGetTime`) | YES | `shouldBeAbleToGetConfiguredTime` only | **DIFF-LEGACY-4** |
| ClockStubContractIsolatedTest | YES (`shouldBeAbleToGetConfiguredTime`) | YES | **`shouldBeAbleToGetTime` + `shouldBeAbleToGetConfiguredTime`** | **DIFF-LEGACY-5** |
| ErpRealContractTest | YES | YES | YES | ALIGNED |
| ErpStubContractTest | YES | YES | YES | ALIGNED |

**DIFF-LEGACY-4: TS mod11 `clock-stub-contract-test.spec.ts` has wrong test**
- Java inherits `shouldBeAbleToGetTime` from `BaseClockContractTest`.
- TS has `shouldBeAbleToGetConfiguredTime` instead.
- **Action:** TS should have `shouldBeAbleToGetTime` (not `shouldBeAbleToGetConfiguredTime`).

**DIFF-LEGACY-5: TS mod11 `clock-stub-contract-isolated-test.spec.ts` has extra test**
- Java only has `shouldBeAbleToGetConfiguredTime`.
- TS has both `shouldBeAbleToGetTime` and `shouldBeAbleToGetConfiguredTime`.
- **Action:** Remove `shouldBeAbleToGetTime` from the isolated test.

---

## 3. ARCHITECTURE LAYERS COMPARISON

### 3.1 Clients Layer

| Component | Java | .NET | TypeScript |
|-----------|------|------|------------|
| ShopApiClient | YES | YES | N/A (integrated into shop-api-driver.ts) |
| ShopUiClient | YES | YES | N/A (integrated into shop-ui-driver.ts) |
| ClockRealClient | YES | YES | N/A (integrated into clock-real-driver.ts) |
| ClockStubClient | YES | YES | N/A (integrated into clock-stub-driver.ts) |
| ErpRealClient | YES | YES | N/A (integrated into erp-real-driver.ts) |
| ErpStubClient | YES | YES | N/A (integrated into erp-stub-driver.ts) |
| TaxRealClient | YES | YES | N/A (integrated into tax-real-driver.ts) |
| TaxStubClient | YES | YES | N/A (integrated into tax-stub-driver.ts) |
| JsonHttpClient | YES | YES | YES (http-client.ts) |
| PageClient | YES | YES | N/A |
| JsonWireMockClient | YES | YES | YES (wiremock-client.ts) |

**DIFF-ARCH-1: TS does not have a separate Clients layer**
- Java and .NET have a clear separation between Clients (HTTP wrappers) and Drivers (business logic wrappers).
- TS merges the client and driver functionality into single driver files.
- **Action:** TS should be refactored to have separate client classes matching Java's structure.

### 3.2 Driver Ports Layer

| Component | Java | .NET | TypeScript | Status |
|-----------|------|------|------------|--------|
| ShopDriver (interface) | YES | IShopDriver | YES (shop-driver.ts) | ALIGNED |
| ClockDriver (interface) | YES | IClockDriver | YES (clock-driver.ts) | ALIGNED |
| ErpDriver (interface) | YES | IErpDriver | YES (erp-driver.ts) | ALIGNED |
| TaxDriver (interface) | YES | ITaxDriver | YES (tax-driver.ts) | ALIGNED |
| DTOs (PlaceOrderRequest, etc.) | YES | YES | Partially in common/dtos.ts | **DIFF-ARCH-2** |

**DIFF-ARCH-2: TS DTOs are not fully separated**
- Java and .NET have DTOs organized by domain (shop/dtos, clock/dtos, erp/dtos, tax/dtos).
- TS has a single `common/dtos.ts` file for some DTOs.
- **Action:** Verify completeness and consider splitting into domain-specific DTO files.

### 3.3 Use Case DSL Layer

| Component | Java | .NET | TypeScript | Status |
|-----------|------|------|------------|--------|
| UseCaseDsl | YES | YES | YES (use-case-context.ts) | ALIGNED (different naming) |
| ShopDsl | YES | YES | N/A (embedded) | **DIFF-ARCH-3** |
| ClockDsl | YES | YES | N/A (embedded) | **DIFF-ARCH-3** |
| ErpDsl | YES | YES | N/A (embedded) | **DIFF-ARCH-3** |
| TaxDsl | YES | YES | N/A (embedded) | **DIFF-ARCH-3** |

**DIFF-ARCH-3: TS does not have separate Use-Case DSL classes per domain**
- Java and .NET have explicit `ShopDsl`, `ClockDsl`, `ErpDsl`, `TaxDsl` classes.
- TS embeds this logic within the scenario DSL layer.
- **Action:** Consider extracting separate use-case DSL classes to match Java's architecture.

### 3.4 Scenario DSL Layer

| Component | Java | .NET | TypeScript | Status |
|-----------|------|------|------------|--------|
| ScenarioDsl (port/interface) | YES | IScenarioDsl | YES (scenario-dsl.ts) | ALIGNED |
| ScenarioDslImpl (core) | YES | ScenarioDsl | YES (scenario-dsl.ts in core) | ALIGNED |
| Given steps (Clock, Country, Coupon, Order, Product, Promotion) | YES (6) | YES (6) | YES (6) | ALIGNED |
| When steps (BrowseCoupons, CancelOrder, PlaceOrder, PublishCoupon, ViewOrder) | YES (5) | YES (6 - includes GoToShop) | YES (5) | **DIFF-ARCH-4** |
| Then steps (Clock, Country, Coupon, Failure, Order, Product, Success) | YES (7) | YES (many more with Failure/SuccessAnd, FailureCoupon, etc.) | YES (matching Java) | **DIFF-ARCH-5** |

**DIFF-ARCH-4: .NET has extra `WhenGoToShop` step not in Java or TS**
- **Action:** Remove `WhenGoToShop` from .NET or add it to Java (depends on whether it's used).

**DIFF-ARCH-5: .NET has additional Then step classes**
- .NET has `ThenFailureAnd`, `ThenSuccessAnd`, `ThenFailureCoupon`, `ThenFailureOrder`, `ThenSuccessCoupon`, `ThenSuccessOrder`, `BaseThenResultCoupon`, `BaseThenResultOrder` which Java doesn't have.
- These are due to .NET's different fluent API chaining approach (needed because C# doesn't support the same return-type covariance as Java).
- **Action:** This is an acceptable language-specific implementation detail, not a functional difference.

### 3.5 Common Layer

| Component | Java | .NET | TypeScript | Status |
|-----------|------|------|------------|--------|
| Closer | YES | N/A (uses IAsyncDisposable) | N/A | Language-specific |
| Converter | YES | YES | N/A | **DIFF-ARCH-6** |
| Result | YES | YES | YES (result.ts) | ALIGNED |
| ResultAssert | YES | ResultAssertExtensions | N/A | Language-specific |
| VoidValue | N/A | YES | N/A | Language-specific |
| ResultTaskExtensions | N/A | YES | N/A | Language-specific |

**DIFF-ARCH-6: TS missing Converter class**
- **Action:** Verify if TS needs a Converter class equivalent.

### 3.6 Channel Layer

| Component | Java | .NET | TypeScript | Status |
|-----------|------|------|------------|--------|
| ChannelType | YES | YES | N/A (uses string literals 'ui', 'api') | **DIFF-ARCH-7** |

**DIFF-ARCH-7: TS uses string literals instead of ChannelType enum**
- Java and .NET define `ChannelType` as an enum with `UI` and `API` values.
- TS uses string literals ('ui', 'api') throughout.
- **Action:** Consider defining a ChannelType enum/constant in TS to match Java.

---

## 4. SUMMARY OF REQUIRED CHANGES

### By Language

#### TypeScript Changes Required: 14

| # | ID | Description | Severity |
|---|-----|-------------|----------|
| 1 | DIFF-TS-1 | Remove extra `shouldBeAbleToGetConfiguredTime` from latest `clock-stub-contract-test.spec.ts` | Medium |
| 2 | DIFF-TS-2 | Remove extra `shouldBeAbleToGetTime` from latest `clock-stub-contract-isolated-test.spec.ts` | Medium |
| 3 | DIFF-TS-3 | Implement `alsoForFirstRow` pattern instead of running all data rows on all channels (affects ~12 test methods) | High |
| 4 | DIFF-TS-4 | Remove extra `.withQuantity(1)` calls from 5 tests in `place-order-negative-test.spec.ts` | Low |
| 5 | DIFF-TS-5 | Add `.withCouponCode(null)` to `discountRateShouldBeNotAppliedWhenThereIsNoCoupon` | Low |
| 6 | DIFF-TS-6 | Change `.withCouponCode(code)` to `.withCouponCode()` (no arg) in subtotal test | Low |
| 7 | DIFF-TS-7 | Verify `undefined` vs `null` behavior for optional fields in publish coupon test | Low |
| 8 | DIFF-TS-8 | Add `alsoForFirstRow=UI` behavior to `ViewOrderNegativeTest` | Medium |
| 9 | DIFF-LEGACY-1 | Rewrite TS legacy mod03-mod07 to use correct abstraction layers (Raw/Client/Driver/ChannelDriver/UseCaseDsl) | **Critical** |
| 10 | DIFF-LEGACY-2 | Split TS mod03-mod05 e2e tests into separate API/UI test files | High |
| 11 | DIFF-LEGACY-3 | Split TS mod04-mod05 smoke tests into separate API/UI files | High |
| 12 | DIFF-LEGACY-4 | Fix TS mod11 `clock-stub-contract-test.spec.ts` to have `shouldBeAbleToGetTime` | Medium |
| 13 | DIFF-LEGACY-5 | Remove extra `shouldBeAbleToGetTime` from TS mod11 `clock-stub-contract-isolated-test.spec.ts` | Medium |
| 14 | DIFF-ARCH-1 to DIFF-ARCH-7 | Architecture layer restructuring (separate clients, DTOs, channel types, etc.) | Medium |

#### .NET Changes Required: 2

| # | ID | Description | Severity |
|---|-----|-------------|----------|
| 1 | DIFF-NET-1 | Remove extra `ShouldRejectOrderWithNonPositiveQuantity` test from latest `PlaceOrderNegativeTest.cs` | Low |
| 2 | DIFF-ARCH-4 | Verify/remove `WhenGoToShop` if not present in Java | Low |

### Totals

| Language | Critical | High | Medium | Low | Total |
|----------|----------|------|--------|-----|-------|
| TypeScript | 1 | 3 | 5 | 5 | 14 |
| .NET | 0 | 0 | 0 | 2 | 2 |
| Java (reference) | 0 | 0 | 0 | 0 | 0 |
| **Overall** | **1** | **3** | **5** | **7** | **16** |
