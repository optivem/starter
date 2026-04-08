# System Test Comparison Report

**Mode:** both (latest + legacy)  
**Depth:** tests  
**Date:** 2026-04-08

---

## Overview

This report compares system tests across Java, .NET, and TypeScript for both the `latest` version and each legacy module (mod02–mod11). The comparison covers three levels: test classes, test method names, and test body logic.

---

## LATEST TESTS

### Category: Smoke (latest)

#### Smoke — External

**Test Classes:**

| Class | Java | .NET | TypeScript |
|-------|------|------|------------|
| ClockSmokeTest | `latest/smoke/external/ClockSmokeTest.java` | `Latest/SmokeTests/External/ClockSmokeTest.cs` | `latest/smoke/external/clock-smoke-test.spec.ts` |
| ErpSmokeTest | `latest/smoke/external/ErpSmokeTest.java` | `Latest/SmokeTests/External/ErpSmokeTest.cs` | `latest/smoke/external/erp-smoke-test.spec.ts` |
| TaxSmokeTest | `latest/smoke/external/TaxSmokeTest.java` | `Latest/SmokeTests/External/TaxSmokeTest.cs` | `latest/smoke/external/tax-smoke-test.spec.ts` |

All three classes exist in all three languages. No missing classes.

**ClockSmokeTest — Method Comparison:**

| Method | Java | .NET | TypeScript |
|--------|------|------|------------|
| shouldBeAbleToGoToClock | `shouldBeAbleToGoToClock()` | `ShouldBeAbleToGoToClock` | `shouldBeAbleToGoToClock` |

**ClockSmokeTest — Body Logic:**
- Java: `scenario.assume().clock().shouldBeRunning()`
- .NET: `await Scenario().Assume().Clock().ShouldBeRunning()`
- TypeScript: `await scenario.assume().clock().shouldBeRunning()`
- All three match in intent and DSL calls.

**ErpSmokeTest — Method Comparison:**

All three: single method `shouldBeAbleToGoToErp`. DSL: `scenario.assume().erp().shouldBeRunning()` equivalent. Consistent.

**TaxSmokeTest — Method Comparison:**

All three: single method `shouldBeAbleToGoToTax`. DSL: `scenario.assume().tax().shouldBeRunning()` equivalent. Consistent.

---

#### Smoke — System

**Test Classes:**

| Class | Java | .NET | TypeScript |
|-------|------|------|------------|
| ShopSmokeTest | `latest/smoke/system/ShopSmokeTest.java` | `Latest/SmokeTests/System/ShopSmokeTest.cs` | `latest/smoke/system/shop-smoke-test.spec.ts` |

All exist. No missing classes.

**ShopSmokeTest — Method:**

| Method | Java | .NET | TypeScript |
|--------|------|------|------------|
| shouldBeAbleToGoToShop | `@Channel({UI,API})` | `[ChannelData(UI,API)]` | iterates `['api','ui']` |

DSL: `scenario.assume().shop().shouldBeRunning()` equivalent in all three. Consistent.

---

### Category: E2E (latest)

**Test Classes:**

| Class | Java | .NET | TypeScript |
|-------|------|------|------------|
| PlaceOrderPositiveTest | `latest/e2e/PlaceOrderPositiveTest.java` | `Latest/E2eTests/PlaceOrderPositiveTest.cs` | `latest/e2e/place-order-positive-test.spec.ts` |

All exist. No missing classes.

**PlaceOrderPositiveTest — Method:**

All three: single method `shouldPlaceOrder` with channel `{UI,API}`. DSL: `when().placeOrder().then().shouldSucceed()`. Consistent.

---

### Category: Contract (latest)

#### Contract — Clock

**Test Classes:**

| Class | Java | .NET | TypeScript |
|-------|------|------|------------|
| ClockRealContractTest | yes | yes | `clock-real-contract-test.spec.ts` |
| ClockStubContractTest | yes | yes | **MISSING** |
| ClockStubContractIsolatedTest | yes | yes | `clock-stub-contract-isolated-test.spec.ts` (combined) |

**FLAG — Missing class in TypeScript:**  
TypeScript has no separate `clock-stub-contract-test.spec.ts` (the non-isolated variant that inherits `shouldBeAbleToGetTime`). Java and .NET both have `ClockStubContractTest` which inherits `shouldBeAbleToGetTime` from the base class.

**ClockRealContractTest — Method/Body:**
- Java/. NET: inherits `shouldBeAbleToGetTime` → `given().clock().withTime("2024-01-02T09:00:00Z").then().clock().hasTime()`
- TypeScript `clock-real-contract-test.spec.ts`: `scenario.given().then().clock().hasTime()` — **MISSING the `given().clock().withTime(...)` setup step**. The `given()` goes directly to `.then()` without setting a clock time.

**FLAG — TypeScript `clock-real-contract-test.spec.ts`:** Missing `clock().withTime("2024-01-02T09:00:00Z")` in the given step. Should be: `given().clock().withTime("2024-01-02T09:00:00Z").then().clock().hasTime()`.

**ClockStubContractIsolatedTest — Method/Body:**
- Java: `shouldBeAbleToGetConfiguredTime` → `given().clock().withTime("2024-01-02T09:00:00Z").then().clock().hasTime("2024-01-02T09:00:00Z")`
- .NET: same pattern
- TypeScript: has both `shouldBeAbleToGetTime` and `shouldBeAbleToGetConfiguredTime`

**FLAG — TypeScript `clock-stub-contract-isolated-test.spec.ts`:** Has an extra method `shouldBeAbleToGetTime` (via `given().clock().withTime().then().clock().hasTime()`) that does not exist in Java/. NET's `ClockStubContractIsolatedTest`. This may be intentional as TypeScript combines the base contract method into the isolated test, but it is a structural difference.

---

#### Contract — ERP

**Test Classes:**

| Class | Java | .NET | TypeScript |
|-------|------|------|------------|
| ErpRealContractTest | yes | yes | `erp-real-contract-test.spec.ts` |
| ErpStubContractTest | yes | yes | `erp-stub-contract-test.spec.ts` |

All exist. No missing classes.

**ErpRealContractTest / ErpStubContractTest — Method/Body:**
- Java/. NET: `shouldBeAbleToGetProduct` → `given().product().withSku("SKU-123").withUnitPrice(12.0).then().product("SKU-123").hasSku("SKU-123").hasPrice(12.0)`
- TypeScript (both real and stub): same scenario DSL calls with identical values. Consistent.

---

#### Contract — Tax

**Test Classes:**

| Class | Java | .NET | TypeScript |
|-------|------|------|------------|
| TaxRealContractTest | yes | yes | `tax-real-contract-test.spec.ts` |
| TaxStubContractTest | yes | yes | `tax-stub-contract-test.spec.ts` |

All exist. No missing classes.

**TaxRealContractTest — Method/Body:**
- Java/. NET: inherits `shouldBeAbleToGetTaxRate` → `given().country().withCode("US").withTaxRate(0.09).then().country("US").hasTaxRateIsPositive()`
- TypeScript: same DSL calls. Consistent.

**TaxStubContractTest — Method/Body:**
- Java has `shouldBeAbleToGetConfiguredTaxRate` → `given().country().withCode("LALA").withTaxRate(0.23).then().country("LALA").hasCountry("LALA").hasTaxRate(0.23)`
- .NET: same
- TypeScript: `shouldBeAbleToGetConfiguredTaxRate` with same values but only runs if `externalSystemMode === 'stub'` (conditional via `(isStub ? it : it.skip)`). The logic is equivalent but uses a runtime skip guard rather than a test framework isolation mechanism. This is a structural difference, not a logic error.

**FLAG — TypeScript `tax-stub-contract-test.spec.ts`:** Missing the inherited `shouldBeAbleToGetTaxRate` method from the base class that Java and .NET inherit. TypeScript only has `shouldBeAbleToGetConfiguredTaxRate`.

---

### Category: Acceptance (latest)

#### BrowseCouponsPositiveTest

**Classes:** All three languages have this class.

**Methods:**

| Method | Java | .NET | TypeScript |
|--------|------|------|------------|
| shouldBeAbleToBrowseCoupons | yes | `ShouldBeAbleToBrowseCoupons` | `shouldBeAbleToBrowseCoupons_${channel}` |
| publishedCouponShouldAppearInList / ShouldReturnPublishedCoupon | yes | yes | yes |

**FLAG — Method name difference:**  
- Java: `publishedCouponShouldAppearInList`  
- .NET: `ShouldReturnPublishedCoupon`  
- TypeScript: `publishedCouponShouldAppearInList_${channel}`  
Java and TypeScript align; .NET uses a different name. **Action: rename .NET method to `PublishedCouponShouldAppearInList`.**

**Body Logic — publishedCouponShouldAppearInList:**
- Java: `given().coupon().withCouponCode("BROWSE10").withDiscountRate(0.10)` — uses hardcoded `"BROWSE10"`
- .NET: `given().Coupon().WithCouponCode("BROWSE10").WithDiscountRate(0.1m)` — same hardcoded value
- TypeScript: `given().coupon().withCode(couponCode).withDiscountRate(0.1)` — uses `randomUUID()` suffix, calls `.withCode()` instead of `.withCouponCode()`

**FLAG — DSL method name difference in TypeScript:** TypeScript calls `.withCode(couponCode)` but Java and .NET call `.withCouponCode(...)`. TypeScript should use `.withCouponCode(...)` for consistency, or all three should align on one name.

**FLAG — TypeScript BrowseCoupons:** uses a UUID-randomized coupon code; Java and .NET use a fixed `"BROWSE10"`. This is a deliberate uniqueness strategy in TypeScript but is an observable difference.

---

#### CancelOrderNegativeIsolatedTest

**Classes:** All three languages have this class.

**Methods:**

| Method | Java | .NET | TypeScript |
|--------|------|------|------------|
| cannotCancelAnOrderOn31stDecBetween2200And2230 | yes | yes | `cannotCancelOrderDuringBlackoutPeriod_${channel}_%s` |

**FLAG — Method name difference:**  
- Java: `cannotCancelAnOrderOn31stDecBetween2200And2230`  
- .NET: `CannotCancelAnOrderOn31stDecBetween2200And2230`  
- TypeScript: `cannotCancelOrderDuringBlackoutPeriod_${channel}_%s` (differs in wording)  
**Action: align TypeScript test name to `cannotCancelAnOrderOn31stDecBetween2200And2230_${channel}_%s`.**

**Body Logic:**
- Java: `given().clock().withTime(timeIso).and().order().withStatus(OrderStatus.PLACED).when().cancelOrder().then().shouldFail().errorMessage("Order cancellation is not allowed on December 31st between 22:00 and 23:00").and().order().hasStatus(OrderStatus.PLACED)`
- .NET: same including `.And().Order().HasStatus(OrderStatus.Placed)` after `then().shouldFail()`
- TypeScript: `given().clock().withTime(time).and().order().withStatus(OrderStatus.PLACED).when().cancelOrder().then().shouldFail().errorMessage(BLACKOUT_ERROR)` — **MISSING `.and().order().hasStatus(OrderStatus.PLACED)` assertion**

**FLAG — TypeScript `cancel-order-negative-isolated-test.spec.ts`:** Missing post-failure order status assertion `.and().order().hasStatus(OrderStatus.PLACED)`. **Action: add this assertion chain after `.shouldFail().errorMessage(...)` in TypeScript.**

**Data Sources (time values):**  
All three use the same 5 time values: `2024-12-31T22:00:00Z`, `2026-12-31T22:00:01Z`, `2025-12-31T22:15:00Z`, `2028-12-31T22:29:59Z`, `2021-12-31T22:30:00Z`. Consistent.

---

#### CancelOrderNegativeTest

**Classes:** All three languages have this class.

**Methods:**

| Method | Java | .NET | TypeScript |
|--------|------|------|------------|
| shouldNotCancelNonExistentOrder | yes | yes | `shouldNotCancelNonExistentOrder_API_$orderNumber` |
| shouldNotCancelAlreadyCancelledOrder | yes | yes | `shouldNotCancelAlreadyCancelledOrder_API` |
| cannotCancelNonExistentOrder | yes | yes | `cannotCancelNonExistentOrder_API` |

Methods align across all three. Channel: Java and .NET use `@Channel(API)` / `[ChannelData(API)]`; TypeScript hardcodes `channel: 'api'`. Consistent.

**Body Logic:** All three match for all three methods with same data values.

---

#### CancelOrderPositiveIsolatedTest

**Classes:** All three languages have this class.

**Methods:**

| Method | Java | .NET | TypeScript |
|--------|------|------|------------|
| shouldBeAbleToCancelOrderOutsideOfBlackoutPeriod31stDecBetween2200And2230 | yes | yes | `shouldBeAbleToCancelOrderOutsideBlackoutPeriod_${channel}_%s` |

**FLAG — Method name difference (minor wording):**  
- Java/. NET: `shouldBeAbleToCancelOrderOutsideOfBlackoutPeriod31stDecBetween2200And2230`  
- TypeScript: `shouldBeAbleToCancelOrderOutsideBlackoutPeriod_${channel}_%s` (missing "31stDecBetween2200And2230" qualifier)  
This is a minor inconsistency. **Action: align TypeScript name to include the full qualifier.**

**Body Logic:** All three identical — `given().clock().withTime(time).and().order().withStatus(PLACED).when().cancelOrder().then().shouldSucceed()`. Consistent.

**Data Sources:** All three use the same 4 time values: `2024-12-31T21:59:59Z`, `2024-12-31T22:30:01Z`, `2024-12-31T10:00:00Z`, `2025-01-01T22:15:00Z`. Consistent.

---

#### CancelOrderPositiveTest

**Classes:** All three languages have this class.

**Methods:**

| Method | Java | .NET | TypeScript |
|--------|------|------|------------|
| shouldHaveCancelledStatusWhenCancelled | yes | yes | `shouldHaveCancelledStatusWhenCancelled_API` |

**FLAG — Channel scope difference:**
- Java: `@Channel({UI, API})`
- .NET: `[ChannelData(UI, API)]`
- TypeScript: hardcodes `channel: 'api'` (API only)

**Action: TypeScript should run this test for both channels (UI and API), not just API.**

**Body Logic:** `given().order().when().cancelOrder().then().shouldSucceed().and().order().hasStatus(CANCELLED)`. All three match.

---

#### PlaceOrderNegativeIsolatedTest

**Classes:** All three languages have this class.

**Methods:**

| Method | Java | .NET | TypeScript |
|--------|------|------|------------|
| cannotPlaceOrderWithExpiredCoupon | yes | yes | `cannotPlaceOrderWithExpiredCoupon_${channel}` |
| shouldRejectOrderPlacedAtYearEnd | yes | yes | `shouldRejectOrderPlacedAtYearEnd_${channel}` |

**Body Logic — cannotPlaceOrderWithExpiredCoupon:**
- Java: `given().clock().withTime("2023-09-01T12:00:00Z").and().coupon().withCouponCode("SUMMER2023").withValidFrom("2023-06-01T00:00:00Z").withValidTo("2023-08-31T23:59:59Z").when().placeOrder().withCouponCode("SUMMER2023").then().shouldFail().errorMessage(...).fieldErrorMessage("couponCode","Coupon code SUMMER2023 has expired")`
- .NET: identical
- TypeScript: `given().clock().withTime("2023-09-01T12:00:00Z").and().coupon().withCode("SUMMER2023").withDiscountRate(0.15).withValidFrom(...).withValidTo(...).when().placeOrder().withCouponCode("SUMMER2023")...`

**FLAG — TypeScript `place-order-negative-isolated-test.spec.ts`:** Uses `.withCode("SUMMER2023")` and also adds `.withDiscountRate(0.15)` in the given. Java and .NET do not set `withDiscountRate` in this test. Also TypeScript uses `.withCode()` instead of `.withCouponCode()`. **Actions:**
1. Align coupon DSL method name from `.withCode()` to `.withCouponCode()`.
2. Remove `.withDiscountRate(0.15)` from the given step (not present in Java/.NET).

**FLAG — Java `PlaceOrderNegativeIsolatedTest`:** Java has `@TimeDependent` annotation only on `cannotPlaceOrderWithExpiredCoupon`, but `shouldRejectOrderPlacedAtYearEnd` does not have `@TimeDependent`. .NET uses `[Time]` only on the `CannotPlaceOrderWithExpiredCoupon` method. This is consistent between Java and .NET.

**FLAG — .NET `PlaceOrderNegativeIsolatedTest.cs`:** Method order differs — `CannotPlaceOrderWithExpiredCoupon` is listed first, `ShouldRejectOrderPlacedAtYearEnd` is second. Java has `shouldRejectOrderPlacedAtYearEnd` first, `cannotPlaceOrderWithExpiredCoupon` second. This is cosmetic, not a logic issue.

---

#### PlaceOrderNegativeTest

**Classes:** All three languages have this class.

**Methods Comparison:**

| Method | Java | .NET | TypeScript |
|--------|------|------|------------|
| shouldRejectOrderWithInvalidQuantity | yes | yes | covered by `shouldRejectOrderWithNonIntegerQuantity_*` |
| shouldRejectOrderWithNonExistentSku | yes | yes | `shouldRejectOrderForNonExistentProduct_${channel}` |
| shouldRejectOrderWithNegativeQuantity | yes | yes | covered by `shouldRejectOrderWithNonPositiveQuantity_*` |
| shouldRejectOrderWithZeroQuantity | yes | yes | covered by `shouldRejectOrderWithNonPositiveQuantity_*` |
| shouldRejectOrderWithNonPositiveQuantity (parameterized) | yes | yes | yes (forEach loop) |
| shouldRejectOrderWithEmptySku | yes | yes | yes (forEach loop) |
| shouldRejectOrderWithEmptyQuantity | yes | yes | yes (forEach loop) |
| shouldRejectOrderWithNonIntegerQuantity (parameterized) | yes | yes | yes (forEach loop) |
| shouldRejectOrderWithEmptyCountry | yes | yes | yes (forEach loop) |
| shouldRejectOrderWithInvalidCountry | yes | yes | `shouldRejectOrderWithInvalidCountry_${channel}` |
| shouldRejectOrderWithNullQuantity | yes (API only) | yes (API only) | yes (API only, conditional) |
| shouldRejectOrderWithNullSku | yes (API only) | yes (API only) | **MISSING** |
| shouldRejectOrderWithNullCountry | yes (API only) | yes (API only) | **MISSING** |
| cannotPlaceOrderWithNonExistentCoupon | yes | yes | `cannotPlaceOrderWithNonExistentCoupon_${channel}` |
| cannotPlaceOrderWithCouponThatHasExceededUsageLimit | yes | yes | yes |

**FLAG — TypeScript missing `shouldRejectOrderWithNullSku` (API-only):** Java and .NET both have `shouldRejectOrderWithNullSku()` that calls `.withSku(null)` and expects `fieldErrorMessage("sku", "SKU must not be empty")`. TypeScript does not have this test.  
**Action: Add `shouldRejectOrderWithNullSku_API` to TypeScript `place-order-negative-test.spec.ts`.**

**FLAG — TypeScript missing `shouldRejectOrderWithNullCountry` (API-only):** Java and .NET both have `shouldRejectOrderWithNullCountry()` that calls `.withCountry(null)` and expects `fieldErrorMessage("country", "Country must not be empty")`. TypeScript does not have this test.  
**Action: Add `shouldRejectOrderWithNullCountry_API` to TypeScript `place-order-negative-test.spec.ts`.**

**FLAG — Method name difference:**
- Java: `shouldRejectOrderWithNonExistentSku`
- .NET: `ShouldRejectOrderWithNonExistentSku`
- TypeScript: `shouldRejectOrderForNonExistentProduct_${channel}`
**Action: rename TypeScript method to `shouldRejectOrderWithNonExistentSku_${channel}`.**

**FLAG — Coupon code value difference in `cannotPlaceOrderWithNonExistentCoupon`:**
- Java: `.withCouponCode("INVALIDCOUPON")` expects error `"Coupon code INVALIDCOUPON does not exist"`
- .NET: `.WithCouponCode("INVALIDCOUPON")` expects error `"Coupon code INVALIDCOUPON does not exist"`
- TypeScript: `.withCouponCode("NON-EXISTENT-COUPON")` expects error `"Coupon code NON-EXISTENT-COUPON does not exist"`
**Action: align TypeScript to use coupon code `"INVALIDCOUPON"` to match Java and .NET.**

**FLAG — `cannotPlaceOrderWithCouponThatHasExceededUsageLimit` body differences:**
- Java/. NET: uses a stub-style setup — `given().coupon().withCouponCode("LIMITED2024").withUsageLimit(2).and().order().withOrderNumber("ORD-1").withCouponCode("LIMITED2024").and().order().withOrderNumber("ORD-2").withCouponCode("LIMITED2024").when().placeOrder().withOrderNumber("ORD-3").withCouponCode("LIMITED2024").then().shouldFail()`
- TypeScript: creates a coupon with `withUsageLimit(1)`, places one order via `placeOrder()`, then attempts another. This is a behavioral simulation, not a direct DSL setup.
This is a significant structural difference. **Action: align TypeScript to use the same stub/given DSL approach as Java and .NET.**

---

#### PlaceOrderPositiveIsolatedTest

**Classes:** All three languages have this class.

**Methods Comparison:**

| Method | Java | .NET | TypeScript |
|--------|------|------|------------|
| shouldRecordPlacementTimestamp | yes | yes | yes |
| shouldApplyFullPriceWithoutPromotion | yes | yes | yes |
| shouldApplyDiscountWhenPromotionIsActive | yes | yes | yes |

**Body Logic — shouldApplyFullPriceWithoutPromotion:**
- Java: `given().product().withUnitPrice(20.00).and().promotion().withActive(false).and().country().withCode("US").withTaxRate("0.00").when().placeOrder().withQuantity(5).then().shouldSucceed().and().order().hasTotalPrice(100.00)`
- .NET: same but `hasTotalPrice(107.00m)` — **DIFFERENT total price value**
- TypeScript: `given().product().withUnitPrice(20.0).and().promotion().withActive(false).when().placeOrder().withQuantity(5).then().shouldSucceed().and().order().hasTotalPrice(100.0)` — no `country` setup, different total price

**FLAG — Major value discrepancy in `shouldApplyFullPriceWithoutPromotion`:**
- Java expects `hasTotalPrice(100.00)` and sets `withTaxRate("0.00")`
- .NET expects `hasTotalPrice(107.00m)` (implies a 7% tax is applied)
- TypeScript expects `hasTotalPrice(100.0)` and does not set country/tax
The expected total price differs between .NET (107.00) and Java/TypeScript (100.00). **Action: Reconcile the expected total price. If the intent is to test full price without any tax, Java and TypeScript are consistent. .NET must be corrected to 100.00m, or all three must set the same tax rate and expected total.**

**Body Logic — shouldApplyDiscountWhenPromotionIsActive:**
- Java: `given().product().withUnitPrice(20.00).and().promotion().withActive(true).withDiscount(0.5).and().country().withCode("US").withTaxRate("0.00").when().placeOrder().withQuantity(5).then().shouldSucceed().and().order().hasTotalPrice(50.00)`
- .NET: `given().Product().WithUnitPrice(20.00m).And().Promotion().WithActive(true).WithDiscount(0.5m).when()...hasTotalPrice(53.50m)` — **DIFFERENT expected total**
- TypeScript: `given().product().withUnitPrice(20.0).and().promotion().withActive(true).withDiscount(0.5).when()...hasTotalPrice(50.0)` — no country/tax setup

**FLAG — Major value discrepancy in `shouldApplyDiscountWhenPromotionIsActive`:**
- Java and TypeScript both expect `hasTotalPrice(50.0)` (20 × 5 × 0.5 = 50, no tax)
- .NET expects `hasTotalPrice(53.50m)` (suggests some tax component)
**Action: .NET must be corrected to `53.50m` or all three must converge on the same setup and expectation.**

---

#### PlaceOrderPositiveTest

**Classes:** All three languages have this class.

**Methods Comparison:**

| Method | Java | .NET | TypeScript |
|--------|------|------|------------|
| shouldBeAbleToPlaceOrderForValidInput | yes | yes | `shouldBeAbleToPlaceOrderForValidInput_${channel}` |
| orderStatusShouldBePlacedAfterPlacingOrder | yes | yes | `orderStatusShouldBePlacedAfterPlacingOrder_${channel}` |
| shouldCalculateBasePriceAsProductOfUnitPriceAndQuantity | yes | yes | `shouldCalculateBasePriceAsProductOfUnitPriceAndQuantity_${channel}` |
| shouldPlaceOrderWithCorrectBasePriceParameterized | yes | yes | yes (it.each) |
| orderPrefixShouldBeORD | yes | yes | `orderPrefixShouldBeORD_${channel}` |
| discountRateShouldBeAppliedForCoupon | yes | yes | `discountRateShouldBeAppliedForCoupon_${channel}` |
| discountRateShouldBeNotAppliedWhenThereIsNoCoupon | yes | yes | `discountRateShouldNotBeAppliedWhenThereIsNoCoupon_${channel}` |
| subtotalPriceShouldBeCalculatedAsTheBasePriceMinusDiscountAmountWhenWeHaveCoupon | yes | yes | yes |
| subtotalPriceShouldBeSameAsBasePriceWhenNoCoupon | yes | yes | yes |
| correctTaxRateShouldBeUsedBasedOnCountry | yes | yes | yes (it.each) |
| totalPriceShouldBeSubtotalPricePlusTaxAmount | yes | yes | yes (it.each) |
| couponUsageCountHasBeenIncrementedAfterItsBeenUsed | yes | yes | yes |
| orderTotalShouldIncludeTax | yes (API only) | **MISSING** | `orderTotalShouldIncludeTax_API` |
| orderTotalShouldReflectCouponDiscount | yes (API only) | **MISSING** | `orderTotalShouldReflectCouponDiscount_API` |
| orderTotalShouldApplyCouponDiscountAndTax | yes (API only) | **MISSING** | `orderTotalShouldApplyCouponDiscountAndTax_API` |
| shouldPublishCouponSuccessfully | yes (PublishCoupon) — wrong class | N/A | N/A |

**FLAG — .NET missing three API-only methods:**  
Java has `orderTotalShouldIncludeTax`, `orderTotalShouldReflectCouponDiscount`, `orderTotalShouldApplyCouponDiscountAndTax` (all `@Channel(API)`). TypeScript also has these. .NET `PlaceOrderPositiveTest.cs` does not.  
**Action: Add these three API-only methods to .NET `PlaceOrderPositiveTest.cs`.**

**FLAG — Java `PlaceOrderPositiveTest` has extra method `orderTotalShouldIncludeTax`:**
- Uses `withCode("DE")`, `withTaxRate("0.19")`, expects `hasSubtotalPrice(20.00)`, `hasTaxRate(0.19)`, `hasTotalPrice(23.80)`
- TypeScript: same country `DE` and same values `subtotalPrice(20.0)`, `taxRate(0.19)`, `totalPrice(23.8)`. **Java and TypeScript align.**

**FLAG — `orderTotalShouldReflectCouponDiscount` — total price discrepancy:**
- Java: `hasTotalPrice(19.26)` — applies both discount and tax
- TypeScript: `hasTotalPrice(18.0)` — implies discount only (no tax)
**Action: Reconcile expected total price between Java and TypeScript for `orderTotalShouldReflectCouponDiscount`.**

**FLAG — Coupon code in several tests:**
- Java uses hardcoded codes (`"SUMMER2025"`, `"LIMITED2024"`, etc.)
- TypeScript uses UUID-suffixed codes (`uniqueCode("SUMMER")`, `uniqueCode("DISC10")`, etc.)
This is a structural difference. .NET uses hardcoded codes like Java. While this does not affect test intent, the TypeScript approach is more robust for parallel test runs.

**FLAG — `discountRateShouldBeAppliedForCoupon` coupon code:**
- Java: `withCouponCode("SUMMER2025")`
- .NET: `WithCouponCode("SUMMER2025")`
- TypeScript: `withCode(uniqueCode("SUMMER"))` — uses `.withCode()` not `.withCouponCode()`
**Action: Align TypeScript to `.withCouponCode()` naming.**

**FLAG — `correctTaxRateShouldBeUsedBasedOnCountry` — data values differ:**
- Java/. NET: `@DataSource({"UK", "0.09"})` and `@DataSource({"US", "0.20"})`
- TypeScript: `{ country: 'UK', taxRate: '0.09' }` and `{ country: 'DE', taxRate: '0.20' }` — uses `DE` instead of `US`
**Action: align TypeScript to use `US` instead of `DE` as the second country, matching Java and .NET.**

**FLAG — `totalPriceShouldBeSubtotalPricePlusTaxAmount` — data values partially differ:**
- Java/. NET: `@DataSource({"UK", "0.09", "50.00", "4.50", "54.50"})` and `@DataSource({"US", "0.20", "100.00", "20.00", "120.00"})`
- TypeScript: `{ country: 'UK', ..., subtotalPrice: 50, ...54.5 }` and `{ country: 'DE', ..., subtotalPrice: 100, ...120 }` — uses `DE` instead of `US`
**Action: align TypeScript second data row country from `DE` to `US`, matching Java and .NET.**

**FLAG — TypeScript `couponUsageCountHasBeenIncrementedAfterItsBeenUsed` — missing `hasUsedCount(1)` assertion:**
- Java: `.and().coupon("SUMMER2025").hasUsedCount(1)`
- .NET: `.And().Coupon("SUMMER2025").HasUsedCount(1)`
- TypeScript: `.and().order().hasAppliedCouponCode(code)` — checks order's coupon code, not the coupon's usage count
**Action: TypeScript should assert `.and().coupon(code).hasUsedCount(1)` to match Java and .NET.**

---

#### PublishCouponNegativeTest

**Classes:** All three languages have this class.

**Methods Comparison:**

| Method | Java | .NET | TypeScript |
|--------|------|------|------------|
| cannotPublishCouponWithZeroOrNegativeDiscount | yes | yes | `shouldRejectCouponWithNonPositiveDiscountRate_${channel}_*` |
| cannotPublishCouponWithDiscountGreaterThan100Percent | yes | yes | `shouldRejectCouponWithDiscountRateAboveOne_${channel}_*` |
| cannotPublishCouponWithDuplicateCouponCode | yes | yes | `cannotPublishCouponWithDuplicateCouponCode_${channel}` |
| cannotPublishCouponWithZeroOrNegativeUsageLimit | yes | yes | `cannotPublishCouponWithZeroOrNegativeUsageLimit_${channel}_*` |
| shouldRejectCouponWithBlankCode | yes (API only) | **MISSING** | `shouldRejectCouponWithBlankCode_${channel}_*` |

**FLAG — .NET missing `shouldRejectCouponWithBlankCode`:** Java has `shouldRejectCouponWithBlankCode` (API only, `@ArgumentsSource(EmptyArgumentsProvider.class)`) that calls `.withCouponCode(code)` with blank/empty values expecting `fieldErrorMessage("code", "Coupon code must not be blank")`. TypeScript also has this. .NET `PublishCouponNegativeTest.cs` does not.  
**Action: Add `ShouldRejectCouponWithBlankCode` (API only) to .NET `PublishCouponNegativeTest.cs`.**

**FLAG — Method name differences:**
- Java: `cannotPublishCouponWithZeroOrNegativeDiscount` / TypeScript: `shouldRejectCouponWithNonPositiveDiscountRate_*`
- Java: `cannotPublishCouponWithDiscountGreaterThan100percent` / TypeScript: `shouldRejectCouponWithDiscountRateAboveOne_*`
**Action: align TypeScript method names to match Java/. NET style (`cannotPublishCouponWithZeroOrNegativeDiscount`, `cannotPublishCouponWithDiscountGreaterThan100Percent`).**

**FLAG — DSL method names in TypeScript PublishCoupon tests:** TypeScript uses `.withCode(...)` while Java/. NET use `.withCouponCode(...)`. This is consistent with the pattern flagged elsewhere.  
**Action: Unify to one name across all three languages.**

**FLAG — Data values in `cannotPublishCouponWithZeroOrNegativeDiscount`:**
- Java: `"0.0", "-0.01", "-0.15"`
- .NET: `"0.0", "-0.01", "-0.15"`
- TypeScript: `0.0, -0.1` (only two values, different second value)
**Action: TypeScript should use `[0.0, -0.01, -0.15]` to match Java and .NET.**

---

#### PublishCouponPositiveTest

**Classes:** All three languages have this class.

**Methods Comparison:**

| Method | Java | .NET | TypeScript |
|--------|------|------|------------|
| shouldBeAbleToPublishValidCoupon | yes | yes | `shouldBeAbleToPublishValidCoupon_${channel}` |
| shouldBeAbleToPublishCouponWithEmptyOptionalFields | yes | yes | `shouldBeAbleToPublishCouponWithEmptyOptionalFields_${channel}` |
| shouldBeAbleToCorrectlySaveCoupon | yes | yes | `shouldBeAbleToCorrectlySaveCoupon_${channel}` |
| shouldPublishCouponSuccessfully | yes (API only) | **MISSING** | **MISSING** |

**FLAG — Java extra method `shouldPublishCouponSuccessfully`:** Java `PublishCouponPositiveTest` has a fourth method `shouldPublishCouponSuccessfully` (API only) that calls `.withCouponCode("SAVE10").withDiscountRate(0.10).then().shouldSucceed()`. This does not exist in .NET or TypeScript.  
**Action: Add `ShouldPublishCouponSuccessfully` to .NET and `shouldPublishCouponSuccessfully_API` to TypeScript.**

**FLAG — `shouldBeAbleToCorrectlySaveCoupon` — body differences:**
- Java/. NET: fully asserts `hasDiscountRate(0.15).isValidFrom(...).isValidTo(...).hasUsageLimit(100).hasUsedCount(0)` using hardcoded code `"SUMMER2025"`
- TypeScript: calls the DSL but **only checks `.shouldSucceed()`** — no assertions on the saved coupon fields  
**Action: TypeScript `shouldBeAbleToCorrectlySaveCoupon` must add `.and().coupon(code).hasDiscountRate(0.15).isValidFrom(...).isValidTo(...).hasUsageLimit(100).hasUsedCount(0)` assertions.**

**FLAG — `shouldBeAbleToPublishCouponWithEmptyOptionalFields` — body difference:**
- Java/. NET: calls `.withValidFrom("").withValidTo("").withUsageLimit("")`
- TypeScript: does not set optional fields at all (just `.withCode(...).withDiscountRate(0.05)`)
**Action: TypeScript should explicitly call `.withValidFrom("").withValidTo("").withUsageLimit("")` to match Java and .NET intent.**

---

#### ViewOrderNegativeTest

**Classes:** All three languages have this class.

**Methods:**

| Method | Java | .NET | TypeScript |
|--------|------|------|------------|
| shouldNotBeAbleToViewNonExistentOrder | yes | yes | `shouldNotBeAbleToViewNonExistentOrder_API_$orderNumber` |

Channel: Java/. NET use `@Channel({UI, API})`. TypeScript hardcodes `channel: 'api'`.

**FLAG — TypeScript only tests API channel:** Java and .NET run this test for both UI and API. TypeScript only runs for API.  
**Action: TypeScript `view-order-negative-test.spec.ts` should also run for UI channel.**

**Body Logic:** all three use same three order numbers and error messages. Consistent.

---

#### ViewOrderPositiveTest

**Classes:** All three languages have this class.

**Methods:**

| Method | Java | .NET | TypeScript |
|--------|------|------|------------|
| shouldBeAbleToViewOrder | yes | yes | `shouldBeAbleToViewOrder_${channel}` |

Channel: all three run for UI and API. Consistent.

**Body Logic:** `given().order().when().viewOrder().then().shouldSucceed()`. All three match.

---

---

## LEGACY TESTS

### Mod02 (smoke only)

**File Structure Comparison:**

| File | Java | .NET | TypeScript |
|------|------|------|------------|
| ErpSmokeTest | yes | yes | yes |
| TaxSmokeTest | yes | yes | **MISSING** |
| ShopApiSmokeTest | yes | yes | yes |
| ShopUiSmokeTest | yes | yes | yes |

**FLAG — TypeScript mod02 missing `tax-smoke-test.spec.ts`:** Java has `TaxSmokeTest` and .NET has `TaxSmokeTest` for mod02. TypeScript has no corresponding file.  
**Action: Add `tax-smoke-test.spec.ts` to TypeScript `test/legacy/mod02/smoke/external/` matching Java/. NET method `shouldBeAbleToGoToTax`.**

**Body Logic (ErpSmokeTest):** All three use raw HTTP GET to `/health` endpoint. Java: `HttpRequest.newBuilder()`. .NET: `new HttpRequestMessage(HttpMethod.Get, ...)`. TypeScript: `fetch(${config.externalSystems.erp.url}/health)`. Functionally consistent.

**Body Logic (ShopApiSmokeTest):** All three do raw HTTP GET to shop API `/health`. Consistent.

**Body Logic (ShopUiSmokeTest):** All three use browser navigation to UI, check status 200, content-type `text/html`, html tags in content. Java uses Playwright Java API; .NET uses Playwright .NET API; TypeScript uses Playwright. Consistent.

---

### Mod03 (e2e)

**File Structure Comparison:**

| File | Java | .NET | TypeScript |
|------|------|------|------------|
| PlaceOrderNegativeApiTest | yes | yes | **MERGED** |
| PlaceOrderNegativeUiTest | yes | yes | **MERGED** |
| PlaceOrderPositiveApiTest | yes | yes | **MERGED** |
| PlaceOrderPositiveUiTest | yes | yes | **MERGED** |

TypeScript has two files (`place-order-negative-test.spec.ts`, `place-order-positive-test.spec.ts`) that iterate over both channels (`['api','ui']`) rather than having separate API and UI test classes.

**FLAG — Structural difference in TypeScript mod03:** Java and .NET have four separate test classes (ApiTest + UiTest for negative and positive). TypeScript collapses them into two files with channel iteration. This is a structural divergence. **Action: Consider whether TypeScript should be split into separate Api/Ui files to match Java and .NET structure.**

**Methods:**

| Method | Java (Negative) | .NET (Negative) | TypeScript (Negative) |
|--------|---------|---------|---------|
| shouldRejectOrderWithNonIntegerQuantity | yes | yes | `shouldRejectOrderWithNonIntegerQuantity_${channel}` |

| Method | Java (Positive) | .NET (Positive) | TypeScript (Positive) |
|--------|---------|---------|---------|
| shouldPlaceOrderForValidInput | yes | yes | `shouldPlaceOrder_${channel}` |

**FLAG — Method name difference in mod03 Positive:** Java/. NET: `shouldPlaceOrderForValidInput`. TypeScript: `shouldPlaceOrder_${channel}`.  
**Action: align TypeScript to `shouldPlaceOrderForValidInput_${channel}`.**

**Body Logic (Positive):** Java and .NET do a full raw implementation (create product via ERP, place order, view order, assert all fields). TypeScript calls the DSL `scenario.when().placeOrder().then().shouldSucceed()` — much simpler. This is expected as TypeScript uses the scenario DSL directly.

---

### Mod04 (e2e + smoke)

**File Structure Comparison:**

| File | Java | .NET | TypeScript |
|------|------|------|------------|
| PlaceOrderNegativeApiTest | yes | yes | **MERGED** |
| PlaceOrderNegativeUiTest | yes | yes | **MERGED** |
| PlaceOrderPositiveApiTest | yes | yes | **MERGED** |
| PlaceOrderPositiveUiTest | yes | yes | **MERGED** |
| ErpSmokeTest | yes | yes | yes |
| TaxSmokeTest | yes | yes | **MISSING** |
| ShopApiSmokeTest | yes | yes | **MISSING** |
| ShopUiSmokeTest | yes | yes | **MISSING** |

**FLAG — TypeScript mod04 missing smoke tests:** Only `erp-smoke-test.spec.ts` and `shop-smoke-test.spec.ts` exist. Java and .NET have four smoke tests: Erp, Tax, ShopApi, ShopUi. TypeScript has:
- `erp-smoke-test.spec.ts` (yes)
- No `tax-smoke-test.spec.ts`
- `shop-smoke-test.spec.ts` (combines Api + UI via channel iteration — equivalent to ShopApi+ShopUi)
- But missing Tax smoke test

**Action: Add `tax-smoke-test.spec.ts` to TypeScript `test/legacy/mod04/smoke/external/`.**

**FLAG — TypeScript mod04 `shop-smoke-test.spec.ts` has combined ShopApi+ShopUi** (iterates both channels), whereas Java/. NET have separate `ShopApiSmokeTest` and `ShopUiSmokeTest`. Minor structural difference.

---

### Mod05 (e2e + smoke)

**File Structure Comparison:**

| File | Java | .NET | TypeScript |
|------|------|------|------------|
| PlaceOrderNegativeApiTest | yes | yes | **MERGED** |
| PlaceOrderNegativeUiTest | yes | yes | **MERGED** |
| PlaceOrderNegativeBaseTest | yes | yes | N/A |
| PlaceOrderPositiveApiTest | yes | yes | **MERGED** |
| PlaceOrderPositiveUiTest | yes | yes | **MERGED** |
| PlaceOrderPositiveBaseTest | yes | yes | N/A |
| ErpSmokeTest | yes | yes | yes |
| TaxSmokeTest | yes | yes | **MISSING** |
| ShopApiSmokeTest | yes | yes | **MISSING** |
| ShopBaseSmokeTest | yes | yes | N/A |
| ShopUiSmokeTest | yes | yes | **MISSING** |

**FLAG — TypeScript mod05 missing Tax, ShopApi, ShopUi smoke tests:** Only `erp-smoke-test.spec.ts` and `shop-smoke-test.spec.ts` exist.  
**Action: Add `tax-smoke-test.spec.ts` to TypeScript `test/legacy/mod05/smoke/external/`. Consider adding separated `shop-api-smoke-test.spec.ts` and `shop-ui-smoke-test.spec.ts` or ensure `shop-smoke-test.spec.ts` covers both channels.**

---

### Mod06 (e2e + smoke)

**File Structure Comparison:**

| File | Java | .NET | TypeScript |
|------|------|------|------------|
| PlaceOrderNegativeTest | yes | yes | yes |
| PlaceOrderPositiveTest | yes | yes | yes |
| ErpSmokeTest | yes | yes | yes |
| TaxSmokeTest | yes | yes | **MISSING** |
| ShopSmokeTest | yes | yes | yes |

**FLAG — TypeScript mod06 missing `tax-smoke-test.spec.ts`:**  
**Action: Add `tax-smoke-test.spec.ts` to TypeScript `test/legacy/mod06/smoke/external/`.**

**Methods — PlaceOrderNegativeTest:**
All three: `shouldRejectOrderWithNonIntegerQuantity`. Java and .NET use `@Channel({UI,API})`. TypeScript reads from `process.env.CHANNEL`. Consistent intent.

**Methods — PlaceOrderPositiveTest:**
All three: `shouldPlaceOrderForValidInput`. Body logic is consistent (product given, place order, check order prefix and status).

---

### Mod07 (e2e + smoke)

**File Structure Comparison:**

| File | Java | .NET | TypeScript |
|------|------|------|------------|
| PlaceOrderNegativeTest | yes | yes | yes |
| PlaceOrderPositiveTest | yes | yes | yes |
| ErpSmokeTest | yes | yes | yes |
| TaxSmokeTest | yes | yes | **MISSING** |
| ShopSmokeTest | yes | yes | yes |

**FLAG — TypeScript mod07 missing `tax-smoke-test.spec.ts`:**  
**Action: Add `tax-smoke-test.spec.ts` to TypeScript `test/legacy/mod07/smoke/external/`.**

**Body Logic — PlaceOrderPositiveTest:**
- Java: uses `app.erp().returnsProduct()...app.shop().placeOrder()...app.shop().viewOrder()...` (use-case DSL)
- .NET: uses `_app.Erp().ReturnsProduct()..._app.Shop(channel).PlaceOrder()..._app.Shop(channel).ViewOrder()...` (use-case DSL)
- TypeScript: uses scenario DSL `given().product().withUnitPrice(20.0).when().placeOrder()...hasOrderNumberPrefix('ORD-').hasStatus(PLACED)` — uses the scenario DSL, not use-case DSL
This is an expected structural divergence as TypeScript adopted the scenario DSL style earlier/differently.

---

### Mod08 (e2e + smoke)

**File Structure Comparison:**

| File | Java | .NET | TypeScript |
|------|------|------|------------|
| PlaceOrderNegativeTest | yes | yes | yes |
| PlaceOrderPositiveTest | yes | yes | yes |
| ErpSmokeTest | yes | yes | yes |
| TaxSmokeTest | yes | yes | **MISSING** |
| ShopSmokeTest | yes | yes | yes |

**FLAG — TypeScript mod08 missing `tax-smoke-test.spec.ts`:**  
**Action: Add `tax-smoke-test.spec.ts` to TypeScript `test/legacy/mod08/smoke/external/`.**

**FLAG — .NET mod08 `ErpSmokeTest` and `TaxSmokeTest`:** These use `BaseSystemDslTest` (from Mod07) instead of `BaseScenarioDslTest` (from Mod08) — likely a copy-paste artifact since the namespace says `Mod08.SmokeTests` but `using SystemTests.Legacy.Mod07.Base`. This is an internal .NET issue, not a cross-language discrepancy.

**Body Logic — PlaceOrderPositiveTest:**
- Java: `scenario.given().product().withUnitPrice(20.00).when().placeOrder().withQuantity(5).then().shouldSucceed().and().order().hasOrderNumberPrefix("ORD-").hasQuantity(5).hasUnitPrice(20.00).hasStatus(PLACED).hasTotalPriceGreaterThanZero()`
- .NET: same including `HasQuantity`, `HasUnitPrice`, `HasTotalPriceGreaterThanZero`
- TypeScript: `given().product().withUnitPrice(20.0).when().placeOrder().withQuantity(5).then().shouldSucceed().and().order().hasOrderNumberPrefix('ORD-').hasStatus(PLACED)` — **MISSING `hasQuantity`, `hasUnitPrice`, `hasTotalPriceGreaterThanZero` assertions**

**FLAG — TypeScript mod08 `place-order-positive-test.spec.ts`:** Missing assertions: `hasQuantity(5)`, `hasUnitPrice(20.0)`, `hasTotalPriceGreaterThanZero()`. Java and .NET both have these.  
**Action: Add the missing assertions to TypeScript mod08 `PlaceOrderPositiveTest`.**

---

### Mod09 (smoke only)

**File Structure Comparison:**

| File | Java | .NET | TypeScript |
|------|------|------|------------|
| ClockSmokeTest | yes | yes | yes |
| ErpSmokeTest | yes | yes | yes |
| TaxSmokeTest | yes | yes | **MISSING** |
| ShopSmokeTest | yes | yes | yes |

**FLAG — TypeScript mod09 missing `tax-smoke-test.spec.ts`:**  
**Action: Add `tax-smoke-test.spec.ts` to TypeScript `test/legacy/mod09/smoke/external/`.**

---

### Mod10 (acceptance)

**File Structure Comparison:**

| File | Java | .NET | TypeScript |
|------|------|------|------------|
| PlaceOrderNegativeIsolatedTest | yes | yes | yes |
| PlaceOrderNegativeTest | yes | yes | yes |
| PlaceOrderPositiveIsolatedTest | yes | yes | yes |
| PlaceOrderPositiveTest | yes | yes | yes |

All four files exist in all three languages. Good.

**PlaceOrderNegativeIsolatedTest — Methods:**

| Method | Java | .NET | TypeScript |
|--------|------|------|------------|
| shouldRejectOrderPlacedAtYearEnd | yes | yes | `shouldRejectOrderPlacedAtYearEnd` |

All three consistent. Body: `given().clock().withTime("2026-12-31T23:59:30Z").when().placeOrder().then().shouldFail().errorMessage("Orders cannot be placed between 23:59 and 00:00 on December 31st")`. Match.

**PlaceOrderNegativeTest — Methods:**

| Method | Java | .NET | TypeScript |
|--------|------|------|------------|
| shouldRejectOrderWithNonIntegerQuantity | yes | yes | yes (forEach) |
| shouldRejectOrderForNonExistentProduct | yes | yes | yes |
| shouldRejectOrderWithEmptySku | yes | yes | yes (forEach) |
| shouldRejectOrderWithNonPositiveQuantity | yes | yes | yes (forEach) |
| shouldRejectOrderWithEmptyQuantity | yes | yes | yes (forEach) |
| shouldRejectOrderWithNullQuantity | yes (API only) | yes (API only) | yes (API only, conditional) |

All methods match across three languages with consistent DSL calls and error messages. No flags.

**PlaceOrderPositiveIsolatedTest — Methods:**

| Method | Java | .NET | TypeScript |
|--------|------|------|------------|
| shouldApplyFullPriceOnWeekday | yes | yes | `shouldApplyFullPriceOnWeekday` |
| shouldApplyDiscountWhenPromotionIsActive | yes | yes | `shouldApplyDiscountWhenPromotionIsActive` |
| shouldRecordPlacementTimestamp | yes | yes | `shouldRecordPlacementTimestamp` |

**Body Logic — shouldApplyFullPriceOnWeekday:**
- Java: `given().product().withUnitPrice(20.00).and().promotion().withActive(false).and().country().withCode("US").withTaxRate("0.00").and().clock().withWeekday().when().placeOrder().withQuantity(5).then().shouldSucceed().and().order().hasTotalPrice(100.00)`
- .NET: same but `HasTotalPrice(100.00m)` — consistent  
- TypeScript: `given().product().withUnitPrice(20.0).and().clock().withWeekday().when().placeOrder().withQuantity(5)...hasTotalPrice(100.0)` — **MISSING `promotion().withActive(false)` and `country().withCode("US").withTaxRate("0.00")` setup steps**

**FLAG — TypeScript mod10 `shouldApplyFullPriceOnWeekday`:** Missing `and().promotion().withActive(false)` and `and().country().withCode("US").withTaxRate("0.00")` setup steps from the given chain.  
**Action: Add the missing given steps.**

**Body Logic — shouldApplyDiscountWhenPromotionIsActive:**
- Java: `given().product().withUnitPrice(20.00).and().promotion().withActive(true).withDiscount("0.5").and().country().withCode("US").withTaxRate("0.00").when()...hasTotalPrice(50.00)`
- .NET: same
- TypeScript: `given().product().withUnitPrice(20.0).and().promotion().withActive(true).withDiscount('0.5').when()...hasTotalPrice(50.0)` — **MISSING `and().country().withCode("US").withTaxRate("0.00")` step**

**FLAG — TypeScript mod10 `shouldApplyDiscountWhenPromotionIsActive`:** Missing country/tax-rate setup step.  
**Action: Add `and().country().withCode("US").withTaxRate("0.00")` to the given chain.**

**PlaceOrderPositiveTest — Methods:**

| Method | Java | .NET | TypeScript |
|--------|------|------|------------|
| orderNumberShouldStartWithORD | yes | yes | `orderNumberShouldStartWithORD` |
| orderStatusShouldBePlacedAfterPlacingOrder | yes | yes | `orderStatusShouldBePlacedAfterPlacingOrder` |

All three consistent. Body matches.

---

### Mod11 (e2e + contract)

**File Structure Comparison:**

| File | Java | .NET | TypeScript |
|------|------|------|------------|
| PlaceOrderPositiveTest (e2e) | yes | yes | yes |
| ClockRealContractTest | yes | yes | yes |
| ClockStubContractTest | yes | yes | **MISSING** |
| ClockStubContractIsolatedTest | yes | yes | `clock-stub-contract-isolated-test.spec.ts` (combined) |
| ErpRealContractTest | yes | yes | yes |
| ErpStubContractTest | yes | yes | yes |

**FLAG — TypeScript mod11 missing `ClockStubContractTest`:** Java and .NET both have a `ClockStubContractTest` that inherits `shouldBeAbleToGetTime` from `BaseClockContractTest` (with `ExternalSystemMode.STUB`). TypeScript only has the isolated variant which combines both methods.  
**Action: Add separate `clock-stub-contract-test.spec.ts` to TypeScript `test/legacy/mod11/contract/clock/` with `shouldBeAbleToGetTime` using stub mode.**

**FLAG — TypeScript mod11 missing Tax contract tests:** Java and .NET do not have Tax contract tests in mod11 either, so this is consistent (Tax contract is only in the latest tests). No action needed.

**PlaceOrderPositiveTest (e2e) — Methods:**

| Method | Java | .NET | TypeScript |
|--------|------|------|------------|
| shouldPlaceOrder | yes | yes | `shouldPlaceOrder` |

All three: `when().placeOrder().then().shouldSucceed()`. Consistent.

**ClockRealContractTest body:**
- Java: inherits `shouldBeAbleToGetTime` from base
- .NET: inherits `ShouldBeAbleToGetTime` from base
- TypeScript: `scenario.given().then().clock().hasTime()` — **same issue as latest: missing `clock().withTime("2024-01-02T09:00:00Z")` in the given step**

**FLAG — TypeScript mod11 `clock-real-contract-test.spec.ts`:** Missing `clock().withTime("2024-01-02T09:00:00Z")` in the given step. Same defect as the latest version.  
**Action: Fix to `given().clock().withTime("2024-01-02T09:00:00Z").then().clock().hasTime()`.**

---

---

## SUMMARY OF ALL ACTIONABLE CHANGES

### TypeScript Changes Required

#### Latest Tests

1. **`test/latest/contract/clock/clock-real-contract-test.spec.ts`** — Fix `shouldBeAbleToGetTime`: add `clock().withTime("2024-01-02T09:00:00Z")` to the given step. Currently goes directly `given().then()` without setting a time.

2. **`test/latest/contract/clock/clock-stub-contract-isolated-test.spec.ts`** — Extra `shouldBeAbleToGetTime` method exists that is not in Java/.NET `ClockStubContractIsolatedTest`. Decide if it should be removed or if Java/.NET should add it.

3. **`test/latest/contract/tax/tax-stub-contract-test.spec.ts`** — Missing inherited `shouldBeAbleToGetTaxRate`. Java and .NET inherit this from `BaseTaxContractTest`. Add this test method.

4. **`test/latest/acceptance/browse-coupons-positive-test.spec.ts`** — `publishedCouponShouldAppearInList`: uses `.withCode()` instead of `.withCouponCode()`. Align DSL method name.

5. **`test/latest/acceptance/cancel-order-negative-isolated-test.spec.ts`** — Method name: rename from `cannotCancelOrderDuringBlackoutPeriod_*` to `cannotCancelAnOrderOn31stDecBetween2200And2230_*`. Also add missing post-failure assertion `.and().order().hasStatus(OrderStatus.PLACED)`.

6. **`test/latest/acceptance/cancel-order-positive-isolated-test.spec.ts`** — Method name: add full qualifier `31stDecBetween2200And2230` to align with Java/.NET.

7. **`test/latest/acceptance/cancel-order-positive-test.spec.ts`** — Run for both UI and API channels, not just API.

8. **`test/latest/acceptance/place-order-negative-isolated-test.spec.ts`** — `cannotPlaceOrderWithExpiredCoupon`: change `.withCode()` to `.withCouponCode()`, remove extra `.withDiscountRate(0.15)` from given step.

9. **`test/latest/acceptance/place-order-negative-test.spec.ts`** — Add `shouldRejectOrderWithNullSku_API` and `shouldRejectOrderWithNullCountry_API` methods. Rename `shouldRejectOrderForNonExistentProduct` to `shouldRejectOrderWithNonExistentSku`. Change coupon code in `cannotPlaceOrderWithNonExistentCoupon` from `"NON-EXISTENT-COUPON"` to `"INVALIDCOUPON"`. Refactor `cannotPlaceOrderWithCouponThatHasExceededUsageLimit` to use stub DSL pattern instead of side-effect approach.

10. **`test/latest/acceptance/place-order-positive-isolated-test.spec.ts`** — `shouldApplyFullPriceWithoutPromotion`: add `and().country().withCode("US").withTaxRate("0.00")` to given chain, change expected total from `100.0` to match consistent value. Review against .NET discrepancy (100 vs 107).

11. **`test/latest/acceptance/place-order-positive-test.spec.ts`** — Multiple issues:
    - All coupon DSL calls: change `.withCode()` to `.withCouponCode()` throughout.
    - `correctTaxRateShouldBeUsedBasedOnCountry`: change second data row country from `DE` to `US`.
    - `totalPriceShouldBeSubtotalPricePlusTaxAmount`: change second data row country from `DE` to `US`.
    - `couponUsageCountHasBeenIncrementedAfterItsBeenUsed`: change assertion from `.and().order().hasAppliedCouponCode(code)` to `.and().coupon(code).hasUsedCount(1)`.
    - `orderTotalShouldReflectCouponDiscount`: reconcile `hasTotalPrice(18.0)` vs Java's `19.26`.

12. **`test/latest/acceptance/publish-coupon-negative-test.spec.ts`** — Rename methods to match Java style. Change discount rate data from `[0.0, -0.1]` to `[0.0, -0.01, -0.15]`. Change DSL from `.withCode()` to `.withCouponCode()` (or whatever is decided).

13. **`test/latest/acceptance/publish-coupon-positive-test.spec.ts`** — `shouldBeAbleToCorrectlySaveCoupon`: add saved coupon field assertions. `shouldBeAbleToPublishCouponWithEmptyOptionalFields`: explicitly call `.withValidFrom("").withValidTo("").withUsageLimit("")`. Add `shouldPublishCouponSuccessfully_API` method.

14. **`test/latest/acceptance/view-order-negative-test.spec.ts`** — Run for both channels (UI and API), not just API.

#### Legacy Tests

15. **`test/legacy/mod02/smoke/external/`** — Add `tax-smoke-test.spec.ts` with `shouldBeAbleToGoToTax` method.

16. **`test/legacy/mod04/smoke/external/`** — Add `tax-smoke-test.spec.ts` with `shouldBeAbleToGoToTax` method.

17. **`test/legacy/mod05/smoke/external/`** — Add `tax-smoke-test.spec.ts` with `shouldBeAbleToGoToTax` method.

18. **`test/legacy/mod06/smoke/external/`** — Add `tax-smoke-test.spec.ts` with `shouldBeAbleToGoToTax` method.

19. **`test/legacy/mod07/smoke/external/`** — Add `tax-smoke-test.spec.ts` with `shouldBeAbleToGoToTax` method.

20. **`test/legacy/mod08/smoke/external/`** — Add `tax-smoke-test.spec.ts` with `shouldBeAbleToGoToTax` method. Also fix `place-order-positive-test.spec.ts` to add missing assertions: `hasQuantity(5)`, `hasUnitPrice(20.0)`, `hasTotalPriceGreaterThanZero()`.

21. **`test/legacy/mod09/smoke/external/`** — Add `tax-smoke-test.spec.ts` with `shouldBeAbleToGoToTax` method.

22. **`test/legacy/mod10/acceptance/place-order-positive-isolated-test.spec.ts`** — `shouldApplyFullPriceOnWeekday`: add `and().promotion().withActive(false).and().country().withCode("US").withTaxRate("0.00")` to given chain. `shouldApplyDiscountWhenPromotionIsActive`: add `and().country().withCode("US").withTaxRate("0.00")` to given chain.

23. **`test/legacy/mod11/contract/clock/`** — Add `clock-stub-contract-test.spec.ts` for the non-isolated stub variant with `shouldBeAbleToGetTime`. Fix `clock-real-contract-test.spec.ts` to include `clock().withTime("2024-01-02T09:00:00Z")` in given step.

---

### .NET Changes Required

24. **`SystemTests/Latest/AcceptanceTests/BrowseCouponsPositiveTest.cs`** — Rename `ShouldReturnPublishedCoupon` to `PublishedCouponShouldAppearInList`.

25. **`SystemTests/Latest/AcceptanceTests/PlaceOrderPositiveIsolatedTest.cs`** — `ShouldApplyFullPriceWithoutPromotion`: change expected `hasTotalPrice(107.00m)` to `100.00m` (or add `WithTaxRate` to match expected value — resolve against Java which expects 100.00 with 0% tax). `ShouldApplyDiscountWhenPromotionIsActive`: change expected `53.50m` to `50.00m` (or add tax to match — resolve against Java which expects 50.00 with 0% tax).

26. **`SystemTests/Latest/AcceptanceTests/PlaceOrderPositiveTest.cs`** — Add three API-only methods: `OrderTotalShouldIncludeTax`, `OrderTotalShouldReflectCouponDiscount`, `OrderTotalShouldApplyCouponDiscountAndTax` (matching Java's `@Channel(API)` methods).

27. **`SystemTests/Latest/AcceptanceTests/PublishCouponNegativeTest.cs`** — Add `ShouldRejectCouponWithBlankCode` (API only, with empty strings from `EmptyArgumentsProvider`).

28. **`SystemTests/Latest/AcceptanceTests/PublishCouponPositiveTest.cs`** — Add `ShouldPublishCouponSuccessfully` (API only) with `.WithCouponCode("SAVE10").WithDiscountRate(0.10m).then().ShouldSucceed()`.

---

### Java Changes Required

29. **`latest/acceptance/PlaceOrderPositiveTest.java`** — `orderTotalShouldReflectCouponDiscount`: expects `hasTotalPrice(19.26)` while TypeScript expects `hasTotalPrice(18.0)`. Reconcile the expected total price (verify business rule: discount only, or discount + tax?).

30. **`latest/acceptance/PublishCouponPositiveTest.java`** — `shouldPublishCouponSuccessfully` is only in Java (extra method). Verify if intended, and add to .NET and TypeScript if so.

---

### Cross-Language DSL Naming Issue

31. **`withCouponCode()` vs `withCode()`:** TypeScript consistently uses `.withCode()` for coupon DSL calls where Java and .NET use `.withCouponCode()`. This needs to be unified across all three languages. **Recommended action: standardize to `.withCouponCode()` in TypeScript to match Java and .NET.**

---

## STRUCTURAL OBSERVATIONS (Non-Flag)

- **TypeScript channel handling:** TypeScript reads `CHANNEL` from environment variables and runs one channel per test run. Java and .NET use `@Channel`/`[ChannelData]` annotations to run both in one test run. This is an intentional architectural difference.
- **TypeScript UUID-randomized test data:** TypeScript uses `randomUUID()` suffixes for unique coupon codes and SKUs to avoid collisions in parallel runs. Java and .NET use fixed test data (relying on test isolation/cleanup). This is acceptable but means test names are dynamic.
- **TypeScript legacy mod03-mod05:** Uses a channel iteration loop (`channels.forEach`) while later modules (mod06+) switch to environment variable approach. The transition point differs slightly from Java/.NET but the intent is consistent.
- **TypeScript mod11 contract tests:** Missing `ClockStubContractTest` (non-isolated base contract) and Tax contract tests entirely. Tax contract tests do not exist in Java/. NET mod11 either, so this is consistent.
