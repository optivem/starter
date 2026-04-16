# System Test Comparison Report

**Mode:** both (latest + legacy)
**Depth:** tests
**Date:** 2026-04-16

---

## PART 1: LATEST VERSION COMPARISON

### 1.1 Smoke Tests

#### Class Coverage

| Test Class | Java | .NET | TypeScript |
|---|---|---|---|
| ErpSmokeTest | Y | Y | Y |
| TaxSmokeTest | Y | Y | Y |
| ClockSmokeTest | Y | Y | Y |
| ShopSmokeTest | Y | Y | Y |

All classes present in all three languages. No gaps.

#### Method & Body Differences

No differences. All DSL chains are equivalent. Channel annotations for ShopSmokeTest are consistent: UI + API in all three languages.

---

### 1.2 E2E Tests

#### Class Coverage

| Test Class | Java | .NET | TypeScript |
|---|---|---|---|
| PlaceOrderPositiveTest | Y | Y | Y |

All consistent. No gaps.

#### Method & Body Differences

No differences. All use `scenario.when().placeOrder().then().shouldSucceed()` with UI + API channels.

---

### 1.3 Acceptance Tests

#### Class Coverage

| Test Class | Java | .NET | TypeScript |
|---|---|---|---|
| PlaceOrderPositiveTest | Y | Y | Y |
| PlaceOrderNegativeTest | Y | Y | Y |
| PlaceOrderPositiveIsolatedTest | Y | Y | Y |
| PlaceOrderNegativeIsolatedTest | Y | Y | Y |
| CancelOrderPositiveTest | Y | Y | Y |
| CancelOrderNegativeTest | Y | Y | Y |
| CancelOrderPositiveIsolatedTest | Y | Y | Y |
| CancelOrderNegativeIsolatedTest | Y | Y | Y |
| BrowseCouponsPositiveTest | Y | Y | Y |
| PublishCouponPositiveTest | Y | Y | Y |
| PublishCouponNegativeTest | Y | Y | Y |
| ViewOrderPositiveTest | Y | Y | Y |
| ViewOrderNegativeTest | Y | Y | Y |

All classes present in all three languages.

#### PlaceOrderPositiveTest — Method Coverage

| Method | Java | .NET | TypeScript | Match? |
|---|---|---|---|---|
| shouldBeAbleToPlaceOrderForValidInput | Y | Y | Y | Body diff (DIFF-L1) |
| orderStatusShouldBePlacedAfterPlacingOrder | Y | Y | Y | Full |
| shouldCalculateBasePriceAsProductOfUnitPriceAndQuantity | Y | Y | Y | Full |
| shouldPlaceOrderWithCorrectBasePriceParameterized | Y | Y | Y | Full |
| orderPrefixShouldBeORD | Y | Y | Y | Full |
| discountRateShouldBeAppliedForCoupon | Y | Y | Y | Full |
| discountRateShouldBeNotAppliedWhenThereIsNoCoupon | Y | Y | Y | Name + body diff (DIFF-L2, L3) |
| subtotalPriceShouldBeCalculatedAsTheBasePriceMinusDiscountAmountWhenWeHaveCoupon | Y | Y | Y | Body diff (DIFF-L5) |
| subtotalPriceShouldBeSameAsBasePriceWhenNoCoupon | Y | Y | Y | Full |
| correctTaxRateShouldBeUsedBasedOnCountry | Y | Y | Y | Full |
| totalPriceShouldBeSubtotalPricePlusTaxAmount | Y | Y | Y | Body diff (DIFF-L6, L7) |
| couponUsageCountHasBeenIncrementedAfterItsBeenUsed | Y | Y | Y | Body diff (DIFF-L8) |
| orderTotalShouldIncludeTax | Y | Y | Y | Full |
| orderTotalShouldReflectCouponDiscount | Y | Y | Y | Full |
| orderTotalShouldApplyCouponDiscountAndTax | Y | Y | Y | Full |

#### PlaceOrderPositiveTest — Body Differences

**DIFF-L1: `shouldBeAbleToPlaceOrderForValidInput` — TS missing given-setup**
- Java/.NET: Has `.given().product().withSku("ABC").withUnitPrice(20.00).and().country().withCode("US").withTaxRate(0.10)` then `.when().placeOrder().withSku("ABC").withQuantity(5).withCountry("US")`
- TypeScript: Only has `.when().placeOrder().then().shouldSucceed()` — no given-setup, no withSku/withQuantity/withCountry
- **Action**: TS must add the given-setup and when-params to match Java/.NET.

**DIFF-L2: `discountRateShouldBeNotAppliedWhenThereIsNoCoupon` — Name mismatch**
- Java/.NET: `discountRateShouldBeNotAppliedWhenThereIsNoCoupon`
- TypeScript: `discountRateShouldNotBeAppliedWhenThereIsNoCoupon`
- **Action**: Rename TS test to match Java/.NET convention.

**DIFF-L3: `discountRateShouldBeNotAppliedWhenThereIsNoCoupon` — TS body diverges**
- Java/.NET: `.when().placeOrder().withCouponCode(null)` then checks `.hasAppliedCoupon(null).hasDiscountRate(0.00).hasDiscountAmount(0.00)`
- TypeScript: `.when().placeOrder()` (no `.withCouponCode(null)`) then checks `.hasAppliedCouponCode(null).hasDiscountRate(0).hasDiscountAmount('0.00')`
- **Action**: TS should add `.withCouponCode(null)` on the when-step.

**DIFF-L4: DSL method naming — `hasAppliedCoupon` vs `hasAppliedCouponCode` (systematic)**
- Java/.NET: `hasAppliedCoupon("SUMMER2025")`
- TypeScript: `hasAppliedCouponCode("SUMMER2025")`
- Affects all tests using this assertion.
- **Action**: Align TS DSL method name `hasAppliedCouponCode` to `hasAppliedCoupon` to match Java/.NET.

**DIFF-L5: `subtotalPriceShouldBeCalculatedAsTheBasePriceMinusDiscountAmountWhenWeHaveCoupon` — TS missing assertions**
- Java/.NET: Asserts `.hasAppliedCoupon().hasDiscountRate(0.15).hasBasePrice(100.00).hasDiscountAmount(15.00).hasSubtotalPrice(85.00)`
- TypeScript: Asserts `.hasBasePrice('100.00').hasDiscountAmount('15.00').hasSubtotalPrice('85.00')` — missing `hasAppliedCoupon()` and `hasDiscountRate(0.15)`
- **Action**: TS must add `.hasAppliedCoupon()` and `.hasDiscountRate(0.15)` assertions.

**DIFF-L6: `totalPriceShouldBeSubtotalPricePlusTaxAmount` — TS missing hasTaxRate assertion**
- Java/.NET: Asserts `.hasTaxRate(taxRate).hasSubtotalPrice(...).hasTaxAmount(...).hasTotalPrice(...)`
- TypeScript: Only asserts `.hasSubtotalPrice(...).hasTaxAmount(...).hasTotalPrice(...)` — missing `.hasTaxRate(taxRate)`
- **Action**: TS must add `.hasTaxRate(...)` assertion.

**DIFF-L7: `totalPriceShouldBeSubtotalPricePlusTaxAmount` — TS given-order differs**
- Java/.NET: `.given().country()...and().product()...` (country first, then product)
- TypeScript: `.given().product()...and().country()...` (product first, then country)
- **Action**: Reorder TS given-steps to match Java/.NET (country first).

**DIFF-L8: `couponUsageCountHasBeenIncrementedAfterItsBeenUsed` — TS missing key assertion**
- Java/.NET: After placing order, asserts `.and().coupon("SUMMER2025").hasUsedCount(1)`
- TypeScript: Only asserts `.and().order().hasAppliedCouponCode(code)` — missing `.coupon("SUMMER2025").hasUsedCount(1)`, asserts on order instead of coupon
- **Action**: TS must change to assert on coupon with `hasUsedCount(1)`, not on order.

#### PlaceOrderNegativeTest — Method Coverage

| Method | Java | .NET | TypeScript | Match? |
|---|---|---|---|---|
| shouldRejectOrderWithInvalidQuantity | Y | Y | N | DIFF-L9 |
| shouldRejectOrderWithNonExistentSku | Y | Y | Y | Body diff (DIFF-L11) |
| shouldRejectOrderWithNegativeQuantity | Y | N | N | DIFF-L10 |
| shouldRejectOrderWithZeroQuantity | Y | N | N | DIFF-L10 |
| shouldRejectOrderWithNonPositiveQuantity (parameterized) | N | Y | Y | DIFF-L10 |
| shouldRejectOrderWithEmptySku | Y | Y | Y | Body diff (DIFF-L12) |
| shouldRejectOrderWithEmptyQuantity | Y | Y | Y | Full |
| shouldRejectOrderWithNonIntegerQuantity (parameterized) | Y | Y | Y | Full |
| shouldRejectOrderWithEmptyCountry | Y | Y | Y | Body diff (DIFF-L15) |
| shouldRejectOrderWithInvalidCountry | Y | Y | Y | Body diff (DIFF-L13) |
| shouldRejectOrderWithNullQuantity | Y | Y | Y | Full |
| shouldRejectOrderWithNullSku | Y | Y | Y | Full |
| shouldRejectOrderWithNullCountry | Y | Y | Y | Full |
| cannotPlaceOrderWithNonExistentCoupon | Y | Y | Y | Body diff (DIFF-L14) |
| cannotPlaceOrderWithCouponThatHasExceededUsageLimit | Y | Y | Y | Full |

**DIFF-L9: `shouldRejectOrderWithInvalidQuantity` — Missing from TS**
- Java/.NET have this as a standalone test using `"invalid-quantity"` value
- TypeScript includes `"invalid-quantity"` in the `shouldRejectOrderWithNonIntegerQuantity` parameterized loop
- **Action**: TS should add a separate test, OR Java/.NET should fold it into the parameterized test.

**DIFF-L10: `shouldRejectOrderWithNegativeQuantity`/`shouldRejectOrderWithZeroQuantity` — Java vs .NET/TS structure**
- Java: Separate `shouldRejectOrderWithNegativeQuantity` (`-10`) and `shouldRejectOrderWithZeroQuantity` (`0`)
- .NET/TS: Parameterized `shouldRejectOrderWithNonPositiveQuantity` with values `["-10", "-1", "0"]`
- **Action**: Converge — recommended: Java adopts the parameterized approach.

**DIFF-L11 through DIFF-L15: TS adds `.withQuantity(1)` to several negative tests**
- Affected methods: `shouldRejectOrderWithNonExistentSku`, `shouldRejectOrderWithEmptySku`, `shouldRejectOrderWithInvalidCountry`, `cannotPlaceOrderWithNonExistentCoupon`, `shouldRejectOrderWithEmptyCountry`
- Java/.NET do not include `.withQuantity(1)` in these tests
- **Action**: Align — either add `.withQuantity(1)` to Java/.NET or remove from TS.

#### PlaceOrderPositiveIsolatedTest

All methods present and consistent across languages. No differences (type differences like `0.5` vs `0.5m` are language-specific).

#### PlaceOrderNegativeIsolatedTest

All methods present and consistent. No body differences.

#### CancelOrderPositiveTest, CancelOrderNegativeTest, CancelOrderPositiveIsolatedTest

All consistent across languages.

#### CancelOrderNegativeIsolatedTest

**DIFF-L19: TS missing status assertion**
- Java/.NET: After `.shouldFail().errorMessage(...)`, also asserts `.and().order().hasStatus(OrderStatus.PLACED)`
- TypeScript: Only asserts `.shouldFail().errorMessage(BLACKOUT_ERROR)` — missing the status assertion
- **Action**: TS must add `.and().order().hasStatus(OrderStatus.PLACED)`.

#### BrowseCouponsPositiveTest

All consistent. No differences.

#### PublishCouponPositiveTest

**DIFF-L20: `shouldBeAbleToPublishCouponWithEmptyOptionalFields` — TS uses `undefined` vs `null`**
- Java/.NET: `.withValidFrom(null).withValidTo(null).withUsageLimit(null)`
- TypeScript: `.withValidFrom(undefined).withValidTo(undefined).withUsageLimit(undefined)`
- Flag for review — may be intentional language difference.

#### PublishCouponNegativeTest

**DIFF-L21: `cannotPublishCouponWithZeroOrNegativeDiscount` — TS uses different coupon code**
- Java/.NET: `.withCouponCode("INVALID-COUPON")`
- TypeScript: `.withCouponCode('INVALID')`
- **Action**: TS should use `'INVALID-COUPON'` to match Java/.NET.

**DIFF-L22: `cannotPublishCouponWithZeroOrNegativeUsageLimit` — TS uses different discountRate**
- Java/.NET: `.withDiscountRate(0.15)`
- TypeScript: `.withDiscountRate(0.1)`
- **Action**: TS should use `0.15` to match Java/.NET.

#### ViewOrderPositiveTest

All consistent.

#### ViewOrderNegativeTest

**DIFF-L23: TS channel scope differs**
- Java/.NET: Channel is `API` with `alsoForFirstRow = UI`
- TypeScript: Channel is only `api` (no UI)
- **Action**: TS should add `'ui'` to channel scope for the first row.

---

### 1.4 Contract Tests

#### Class Coverage

| Test Class | Java | .NET | TypeScript |
|---|---|---|---|
| BaseErpContractTest | Y | Y | N/A (inlined) |
| ErpRealContractTest | Y | Y | Y |
| ErpStubContractTest | Y | Y | Y |
| BaseTaxContractTest | Y | Y | N/A (inlined) |
| TaxRealContractTest | Y | Y | Y |
| TaxStubContractTest | Y | Y | Y |
| BaseClockContractTest | Y | Y | N/A (inlined) |
| ClockRealContractTest | Y | Y | Y |
| ClockStubContractTest | Y | Y | Y |
| ClockStubContractIsolatedTest | Y | Y | Y |

TypeScript inlines the base test method into each real/stub file rather than using inheritance. Structurally equivalent.

#### Body Differences

**DIFF-L24: `ClockStubContractTest` — `shouldBeAbleToGetTime` body differs**
- Java/.NET: `scenario.given().clock().withTime("2024-01-02T09:00:00Z").then().clock().hasTime()`
- TypeScript: `scenario.given().clock().withTime().then().clock().hasTime()` — uses `.withTime()` (no argument)
- **Action**: TS should use `.withTime('2024-01-02T09:00:00Z')` to match Java/.NET.

**DIFF-L25: `ClockStubContractIsolatedTest` — TS has extra test**
- Java/.NET: Only has `shouldBeAbleToGetConfiguredTime`
- TypeScript: Has both `shouldBeAbleToGetTime` AND `shouldBeAbleToGetConfiguredTime`
- **Action**: TS should remove the extra `shouldBeAbleToGetTime` from the isolated test.

**DIFF-L26: `TaxStubContractTest` — TS type inconsistency**
- TypeScript tax-real-contract-test: `.withTaxRate('0.09')` (string)
- TypeScript tax-stub-contract-test: `.withTaxRate(0.09)` (number)
- **Action**: TS should be internally consistent — use the same type in both.

---

## PART 2: LEGACY VERSION COMPARISON (Module by Module)

### 2.1 Mod02 (Smoke — Raw)

#### Class Coverage

| Test Class | Java | .NET | TypeScript |
|---|---|---|---|
| ErpSmokeTest | Y | Y | Y |
| TaxSmokeTest | Y | Y | Y |
| ShopApiSmokeTest | Y | Y | Y |
| ShopUiSmokeTest | Y | Y | Y |

All consistent. Bodies use raw HTTP/Playwright calls, logic equivalent across all three languages.

---

### 2.2 Mod03 (E2E — Raw)

#### Class Coverage

| Test Class | Java | .NET | TypeScript |
|---|---|---|---|
| PlaceOrderPositiveApiTest | Y | Y | N |
| PlaceOrderPositiveUiTest | Y | Y | N |
| PlaceOrderNegativeApiTest | Y | Y | N |
| PlaceOrderNegativeUiTest | Y | Y | N |
| PlaceOrderPositiveTest (unified) | N | N | Y |
| PlaceOrderNegativeTest (unified) | N | N | Y |

**DIFF-G1: TS mod03 uses unified channel-aware tests while Java/.NET use separate Api/Ui test classes.**

**DIFF-G2: TS mod03 positive test is much simpler**
- Java/.NET: Full e2e with product creation, order placement, order viewing, assertions on all fields
- TypeScript: Just `scenario.when().placeOrder().then().shouldSucceed()` — already uses DSL
- TS is structurally ahead of Java/.NET at this module stage.

**DIFF-G3: TS mod03 negative test uses different quantity value**
- Java/.NET: Use `"invalid-quantity"`
- TypeScript: Uses `'3.5'`
- **Action**: Align test data.

---

### 2.3 Mod04 (E2E — Client)

Same structural pattern as mod03. TS uses unified files while Java/.NET use separate Api/Ui classes.

**DIFF-G4: TS mod04 smoke already uses unified ShopSmokeTest with DSL, while Java/.NET still use separate ShopApiSmokeTest/ShopUiSmokeTest with client calls.**

---

### 2.4 Mod05 (E2E — Driver)

| Test Class | Java | .NET | TypeScript |
|---|---|---|---|
| PlaceOrderPositiveBaseTest + Api/Ui | Y | Y | N |
| PlaceOrderNegativeBaseTest + Api/Ui | Y | Y | N |
| PlaceOrderPositiveTest (unified) | N | N | Y |
| PlaceOrderNegativeTest (unified) | N | N | Y |

**DIFF-G5: TS mod05 positive test has fewer assertions than Java/.NET**
- Java/.NET: Full assertions (orderNumber prefix, quantity, unitPrice, status, totalPrice > 0)
- TypeScript: Only checks `hasOrderNumberPrefix('ORD-').hasStatus(OrderStatus.PLACED)`
- **Action**: TS should add missing assertions (quantity, unitPrice, totalPrice) or Java/.NET should simplify.

---

### 2.5 Mod06 (E2E — Channel Driver)

All consistent across all three languages. All use unified test files with channel annotation. Method names and bodies match.

---

### 2.6 Mod07 (E2E — Use Case DSL)

**DIFF-G6: Java/.NET use use-case DSL while TS uses scenario DSL**
- Java: `app.shop().placeOrder().orderNumber(...).sku(...).quantity(5).country(...).execute().shouldSucceed()`
- TypeScript: `scenario.given().product()...when().placeOrder()...then().shouldSucceed()`
- TS is already at scenario DSL level while Java/.NET are at use-case DSL level.

**DIFF-G7: Base test class name differs**
- Java: `BaseUseCaseDslTest`
- .NET: `BaseSystemDslTest`
- **Action**: Align names.

---

### 2.7 Mod08 (E2E — Scenario DSL)

**DIFF-G8: Method name/structure differs**
- Java/.NET: `shouldPlaceOrderForValidInput` (single combined test)
- TypeScript: Two separate tests: `orderNumberShouldStartWithORD` and `orderStatusShouldBePlacedAfterPlacingOrder`

**DIFF-G9: Java/.NET have more assertions than TS**
- Java/.NET: `.hasOrderNumberPrefix("ORD-").hasQuantity(5).hasUnitPrice(20.00).hasStatus(OrderStatus.PLACED).hasTotalPriceGreaterThanZero()`
- TS test 1: `.hasOrderNumberPrefix('ORD-')` only
- TS test 2: `.hasStatus(OrderStatus.PLACED)` only
- **Action**: Align — either TS adds missing assertions or Java/.NET splits into two tests.

---

### 2.8 Mod09 (Smoke — Clock Added)

All consistent across all three languages. No differences.

---

### 2.9 Mod10 (Acceptance)

#### Class Coverage

| Test Class | Java | .NET | TypeScript |
|---|---|---|---|
| PlaceOrderPositiveTest | Y | Y | Y |
| PlaceOrderNegativeTest | Y | Y | Y |
| PlaceOrderPositiveIsolatedTest | Y | Y | Y |
| PlaceOrderNegativeIsolatedTest | Y | Y | Y |

All consistent.

#### Body Differences

**DIFF-G12: `shouldApplyFullPriceOnWeekday`**
- All three languages are consistent in legacy mod10, using `.and().clock().withWeekday()`.
- Note: In latest, this was renamed to `shouldApplyFullPriceWithoutPromotion` and the `.clock().withWeekday()` step was removed. All three languages made this change consistently.

**DIFF-G13: `shouldApplyDiscountWhenPromotionIsActive` — `.withDiscount` argument type**
- All three use string `"0.5"` in legacy. In latest, Java uses `0.5` (double), .NET uses `0.5m` (decimal), TS uses `0.5` (number). All consistent within each version.

---

### 2.10 Mod11 (Contract + E2E)

#### E2E
All consistent: `shouldPlaceOrder` with `scenario.when().placeOrder().then().shouldSucceed()`.

#### Contract — Class Coverage

| Test Class | Java | .NET | TypeScript |
|---|---|---|---|
| ErpRealContractTest | Y | Y | Y |
| ErpStubContractTest | Y | Y | Y |
| ClockRealContractTest | Y | Y | Y |
| ClockStubContractTest | Y | Y | Y |
| ClockStubContractIsolatedTest | Y | Y | Y |
| TaxRealContractTest | N | N | N |
| TaxStubContractTest | N | N | N |

Tax contract tests missing from all three languages in legacy mod11 (present in latest). Consistent gap.

#### Contract Body Differences

**DIFF-G16: `BaseClockContractTest.shouldBeAbleToGetTime`**
- Java (legacy mod11): `scenario.given().clock().withTime()` (no argument)
- .NET/TS (legacy mod11): `scenario.given().clock().withTime("2024-01-02T09:00:00Z")`
- **Action**: Java legacy mod11 should use `.withTime("2024-01-02T09:00:00Z")` to match .NET/TS.

**DIFF-G17: Legacy mod11 ClockStubContractIsolatedTest — TS has extra test**
- Java/.NET: Only `shouldBeAbleToGetConfiguredTime`
- TypeScript: Has both `shouldBeAbleToGetTime` AND `shouldBeAbleToGetConfiguredTime`
- **Action**: TS should remove the extra `shouldBeAbleToGetTime`.

---

## SUMMARY OF REQUIRED CHANGES

### By Language

| Language | Issues |
|---|---|
| **TypeScript** | 18 |
| **Java** | 3 |
| **.NET** | 1 |

### By Area

| Area | Count | Diff IDs |
|---|---|---|
| Missing assertions in TS | 5 | DIFF-L1, L5, L6, L8, L19 |
| Structural differences (legacy) | 5 | DIFF-G1, G2, G4, G5, G8 |
| Body logic differences | 5 | DIFF-L3, L7, L24, G6, G16 |
| Test data value mismatches | 4 | DIFF-L21, L22, G3, L11-L15 |
| Method naming mismatches | 3 | DIFF-L2, L4, G7 |
| Missing/extra tests | 3 | DIFF-L9, L10, L25 |
| Channel scope differences | 1 | DIFF-L23 |

### Critical Items

1. **DIFF-L1**: TS latest `shouldBeAbleToPlaceOrderForValidInput` missing entire given-setup and when-params
2. **DIFF-L8**: TS latest `couponUsageCountHasBeenIncrementedAfterItsBeenUsed` asserts on wrong entity (order instead of coupon)
3. **DIFF-L19**: TS latest `CancelOrderNegativeIsolatedTest` missing status assertion
4. **DIFF-L10**: Java has separate negative/zero quantity tests while .NET/TS use parameterized — must converge
5. **DIFF-L4**: Systematic DSL naming gap — TS `hasAppliedCouponCode` vs Java/.NET `hasAppliedCoupon`
6. **DIFF-G16**: Legacy mod11 Java `BaseClockContractTest` missing time argument
