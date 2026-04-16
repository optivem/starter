# System Test Comparison Report

**Mode:** both (latest + legacy)
**Depth:** tests
**Date:** 2026-04-16

---

## PART 1: LATEST VERSION COMPARISON

### 1.1 Smoke Tests

#### Class Coverage

| Class | Java | .NET | TS |
|-------|------|------|----|
| ShopSmokeTest | Y | Y | Y |
| ClockSmokeTest | Y | Y | Y |
| ErpSmokeTest | Y | Y | Y |
| TaxSmokeTest | Y | Y | Y |

All classes present in all three languages. No method or body differences.

---

### 1.2 E2E Tests

#### Class Coverage

| Class | Java | .NET | TS |
|-------|------|------|----|
| PlaceOrderPositiveTest | Y | Y | Y |

All consistent. Single method `shouldPlaceOrder` with `scenario.when().placeOrder().then().shouldSucceed()` and UI + API channels. No differences.

---

### 1.3 Acceptance Tests

#### Class Coverage

| Class | Java | .NET | TS |
|-------|------|------|----|
| PlaceOrderPositiveTest | Y | Y | Y |
| PlaceOrderNegativeTest | Y | Y | Y |
| PlaceOrderPositiveIsolatedTest | Y | Y | Y |
| PlaceOrderNegativeIsolatedTest | Y | Y | Y |
| CancelOrderPositiveTest | Y | Y | Y |
| CancelOrderNegativeTest | Y | Y | Y |
| CancelOrderPositiveIsolatedTest | Y | Y | Y |
| CancelOrderNegativeIsolatedTest | Y | Y | Y |
| ViewOrderPositiveTest | Y | Y | Y |
| ViewOrderNegativeTest | Y | Y | Y |
| BrowseCouponsPositiveTest | Y | Y | Y |
| PublishCouponPositiveTest | Y | Y | Y |
| PublishCouponNegativeTest | Y | Y | Y |

All classes present in all three languages.

#### PlaceOrderPositiveTest - Method Coverage

| Method | Java | .NET | TS | Match? |
|--------|------|------|----|--------|
| shouldBeAbleToPlaceOrderForValidInput | Y | Y | Y | Full |
| orderStatusShouldBePlacedAfterPlacingOrder | Y | Y | Y | Full |
| shouldCalculateBasePriceAsProductOfUnitPriceAndQuantity | Y | Y | Y | Full |
| shouldPlaceOrderWithCorrectBasePriceParameterized | Y | Y | Y | Full |
| orderPrefixShouldBeORD | Y | Y | Y | Full |
| discountRateShouldBeAppliedForCoupon | Y | Y | Y | Full |
| discountRateShouldBeNotAppliedWhenThereIsNoCoupon | Y | Y | Y | Body diff (DIFF-L001) |
| subtotalPriceShouldBeCalculatedAsTheBasePriceMinusDiscountAmountWhenWeHaveCoupon | Y | Y | Y | Full |
| subtotalPriceShouldBeSameAsBasePriceWhenNoCoupon | Y | Y | Y | Full |
| correctTaxRateShouldBeUsedBasedOnCountry | Y | Y | Y | Full |
| totalPriceShouldBeSubtotalPricePlusTaxAmount | Y | Y | Y | Full |
| couponUsageCountHasBeenIncrementedAfterItsBeenUsed | Y | Y | Y | Full |
| orderTotalShouldIncludeTax | Y | Y | Y | Full |
| orderTotalShouldReflectCouponDiscount | Y | Y | Y | Full |
| orderTotalShouldApplyCouponDiscountAndTax | Y | Y | Y | Full |

**DIFF-L001: `discountRateShouldBeNotAppliedWhenThereIsNoCoupon` - TS missing `.withCouponCode(null)`**
- Java/.NET: `.when().placeOrder().withCouponCode(null)` then checks `.hasAppliedCoupon(null).hasDiscountRate(0.00).hasDiscountAmount(0.00)`
- TS: `.when().placeOrder()` (no `.withCouponCode(null)`) then checks `.hasAppliedCouponCode(null).hasDiscountRate(0).hasDiscountAmount('0.00')`
- **Action**: TS should add `.withCouponCode(null)` on the when-step.

#### PlaceOrderNegativeTest - Method Coverage

| Method | Java | .NET | TS | Match? |
|--------|------|------|----|--------|
| shouldRejectOrderWithInvalidQuantity | Y | Y | Y | Full |
| shouldRejectOrderWithNonExistentSku | Y | Y | Y | Body diff (DIFF-L004) |
| shouldRejectOrderWithNegativeQuantity | Y | Y | N | DIFF-L003 |
| shouldRejectOrderWithZeroQuantity | Y | Y | N | DIFF-L003 |
| shouldRejectOrderWithNonPositiveQuantity (parameterized) | N | Y | Y | DIFF-L003 |
| shouldRejectOrderWithEmptySku | Y | Y | Y | Body diff (DIFF-L004) |
| shouldRejectOrderWithEmptyQuantity | Y | Y | Y | Full |
| shouldRejectOrderWithNonIntegerQuantity (parameterized) | Y | Y | Y | Full |
| shouldRejectOrderWithEmptyCountry | Y | Y | Y | Body diff (DIFF-L007) |
| shouldRejectOrderWithInvalidCountry | Y | Y | Y | Body diff (DIFF-L005) |
| shouldRejectOrderWithNullQuantity | Y | Y | Y | Full |
| shouldRejectOrderWithNullSku | Y | Y | Y | Full |
| shouldRejectOrderWithNullCountry | Y | Y | Y | Full |
| cannotPlaceOrderWithNonExistentCoupon | Y | Y | Y | Body diff (DIFF-L006) |
| cannotPlaceOrderWithCouponThatHasExceededUsageLimit | Y | Y | Y | Full |

**DIFF-L003: Negative/zero quantity test structure**
- Java: Separate `shouldRejectOrderWithNegativeQuantity` (-10) and `shouldRejectOrderWithZeroQuantity` (0)
- .NET: Has BOTH individual tests AND parameterized `ShouldRejectOrderWithNonPositiveQuantity` (redundant)
- TS: Parameterized `shouldRejectOrderWithNonPositiveQuantity` with values [-10, -1, 0]
- **Action**: Converge -- recommended: Java adopts parameterized approach; .NET removes individual tests.

**DIFF-L004: TS adds `.withQuantity(1)` to several negative tests**
- Affected: `shouldRejectOrderWithNonExistentSku`, `shouldRejectOrderWithEmptySku`
- Java/.NET do not include `.withQuantity(1)` in these tests
- **Action**: Align -- either add `.withQuantity(1)` to Java/.NET or remove from TS.

**DIFF-L005: TS adds `.withQuantity(1)` in `shouldRejectOrderWithInvalidCountry`**
- Same pattern as DIFF-L004.
- **Action**: Same as DIFF-L004.

**DIFF-L006: TS adds `.withQuantity(1)` in `cannotPlaceOrderWithNonExistentCoupon`**
- Same pattern as DIFF-L004.
- **Action**: Same as DIFF-L004.

**DIFF-L007: TS adds `.withQuantity(1)` in `shouldRejectOrderWithEmptyCountry`**
- Same pattern as DIFF-L004.
- **Action**: Same as DIFF-L004.

#### PlaceOrderPositiveIsolatedTest

All methods present and consistent across languages. No differences.

#### PlaceOrderNegativeIsolatedTest

All methods present and consistent. No body differences.

#### CancelOrderPositiveTest, CancelOrderNegativeTest, CancelOrderPositiveIsolatedTest

All consistent across languages.

#### CancelOrderNegativeIsolatedTest

All consistent across languages.

#### BrowseCouponsPositiveTest

All consistent. No differences.

#### PublishCouponPositiveTest

All consistent. No differences.

#### PublishCouponNegativeTest

**DIFF-L008: `cannotPublishCouponWithZeroOrNegativeDiscount` - TS channel scope differs**
- Java/.NET: `alsoForFirstRow = ChannelType.UI` (API for all rows, UI only for first row)
- TS: `forChannels('ui', 'api')` runs ALL rows on BOTH channels
- **Action**: Update TS to use alsoForFirstRow equivalent for first data row only.

**DIFF-L009: `cannotPublishCouponWithZeroOrNegativeUsageLimit` - same channel mismatch**
- Same pattern as DIFF-L008.
- **Action**: Same as DIFF-L008.

#### ViewOrderPositiveTest

All consistent.

#### ViewOrderNegativeTest

**DIFF-L010: TS channel scope differs**
- Java/.NET: Channel is `API` with `alsoForFirstRow = UI`
- TS: Channel is only `api` (no UI)
- **Action**: TS should add `'ui'` to channel scope for the first row.

---

### 1.4 Contract Tests

#### Class Coverage

| Test Class | Java | .NET | TS |
|------------|------|------|----|
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

TS inlines base test methods into each real/stub file rather than using inheritance. Structurally equivalent.

#### Clock Contract Body Differences

**DIFF-L011: `ClockStubContractTest.shouldBeAbleToGetTime` - TS has extra given-step**
- Java/.NET: `scenario.given().then().clock().hasTime()` (no `.clock().withTime()` in given)
- TS: `scenario.given().clock().withTime().then().clock().hasTime()` (calls `.withTime()` with no arg in given)
- **Action**: Remove `.clock().withTime()` from TS stub test's `shouldBeAbleToGetTime`.

**DIFF-L012: `ClockStubContractTest` - TS has duplicate `shouldBeAbleToGetConfiguredTime`**
- Java/.NET place `shouldBeAbleToGetConfiguredTime` ONLY in `ClockStubContractIsolatedTest`
- TS has it in BOTH `clock-stub-contract-test.spec.ts` AND `clock-stub-contract-isolated-test.spec.ts`
- **Action**: Remove `shouldBeAbleToGetConfiguredTime` from TS `clock-stub-contract-test.spec.ts`.

**DIFF-L013: `ClockStubContractIsolatedTest` - TS has extra test**
- Java/.NET: Only has `shouldBeAbleToGetConfiguredTime`
- TS: Has both `shouldBeAbleToGetTime` AND `shouldBeAbleToGetConfiguredTime`
- **Action**: Remove `shouldBeAbleToGetTime` from TS `clock-stub-contract-isolated-test.spec.ts`.

---

## PART 2: LEGACY VERSION COMPARISON (Module by Module)

### Architectural Abstraction Summary

| Module | Expected Layer | Java | .NET | TypeScript | Match? |
|--------|---------------|------|------|------------|--------|
| mod02 | Raw | Raw | Raw | Raw | Full |
| mod03 | Raw | Raw | Raw | **Scenario DSL** | **MISMATCH** |
| mod04 | Client | Client | Client | **Scenario DSL** | **MISMATCH** |
| mod05 | Driver | Driver | Driver | **Scenario DSL** | **MISMATCH** |
| mod06 | Channel Driver | Channel Driver | Channel Driver | **Scenario DSL** | **MISMATCH** |
| mod07 | Use-Case DSL | Use-Case DSL | Use-Case DSL | **Scenario DSL** | **MISMATCH** |
| mod08 | Scenario DSL | Scenario DSL | Scenario DSL | Scenario DSL | Full |
| mod09 | Scenario DSL + Clock | Scenario DSL + Clock | Scenario DSL + Clock | Scenario DSL + Clock | Full |
| mod10 | Scenario DSL + Isolated | Scenario DSL + Isolated | Scenario DSL + Isolated | Scenario DSL + Isolated | Full |
| mod11 | Scenario DSL + Contract | Scenario DSL + Contract | Scenario DSL + Contract | Scenario DSL + Contract | Full |

**All TS legacy modules mod03-mod07 share the same `withApp()` fixture providing Scenario DSL**, making them architecturally identical to mod08+. This defeats the pedagogical purpose of the legacy progression.

---

### 2.1 Mod02 (Smoke - Raw)

#### Class Coverage

| Test Class | Java | .NET | TS |
|------------|------|------|----|
| ErpSmokeTest | Y | Y | Y |
| TaxSmokeTest | Y | Y | Y |
| ShopApiSmokeTest | Y | Y | Y |
| ShopUiSmokeTest | Y | Y | Y |

All consistent. Bodies use raw HTTP/Playwright calls. No differences.

---

### 2.2 Mod03 (E2E - Raw)

**DIFF-G001: Architectural mismatch - TS uses Scenario DSL instead of Raw**
- Java/.NET: Raw HTTP requests and Playwright page interactions directly
- TS: `scenario.when().placeOrder().then().shouldSucceed()` via DSL
- **Action**: Rewrite TS mod03 e2e tests to use raw `fetch()` and Playwright calls.

#### Class Coverage

| Test Class | Java | .NET | TS |
|------------|------|------|----|
| PlaceOrderPositiveApiTest | Y | Y | N |
| PlaceOrderPositiveUiTest | Y | Y | N |
| PlaceOrderNegativeApiTest | Y | Y | N |
| PlaceOrderNegativeUiTest | Y | Y | N |
| PlaceOrderPositiveTest (unified) | N | N | Y |
| PlaceOrderNegativeTest (unified) | N | N | Y |

**DIFF-G002: TS uses unified files instead of separate Api/Ui classes**
- **Action**: When DIFF-G001 is fixed, split TS into separate api/ui files.

**DIFF-G003: TS mod03 positive test is vastly simpler**
- Java/.NET: Full e2e with product creation, order placement, order viewing, assertions on all fields
- TS: Just `scenario.when().placeOrder().then().shouldSucceed()` -- no field assertions
- **Action**: When DIFF-G001 is fixed, add full field assertions.

---

### 2.3 Mod04 (E2E - Client)

**DIFF-G004: Architectural mismatch - TS uses Scenario DSL instead of Client**
- **Action**: Rewrite TS mod04 to use typed client abstractions.

**DIFF-G005: Smoke file structure - TS merged ShopApiSmokeTest/ShopUiSmokeTest**
- **Action**: When DIFF-G004 is fixed, split into Api/Ui files.

**DIFF-G006: Test data - TS uses `"3.5"` vs Java/.NET `"invalid-quantity"`**
- **Action**: When DIFF-G004 is fixed, use `"invalid-quantity"` to match.

---

### 2.4 Mod05 (E2E - Driver)

**DIFF-G007: Architectural mismatch - TS uses Scenario DSL instead of Driver**
- **Action**: Rewrite TS mod05 to use Driver-level abstraction.

**DIFF-G008: File structure - TS missing Base+Api+Ui class hierarchy**
- Java/.NET: `PlaceOrderPositiveBaseTest` + `Api/Ui` subclasses
- TS: Single unified file with `forChannels`
- **Action**: When DIFF-G007 is fixed, adopt Base+Api+Ui pattern.

**DIFF-G009: TS mod05 positive test missing assertions**
- Java/.NET: orderNumber prefix, sku, quantity, unitPrice, totalPrice > 0, status
- TS: Only orderNumberPrefix, status
- **Action**: Add missing assertions (quantity, unitPrice, totalPriceGreaterThanZero).

---

### 2.5 Mod06 (E2E - Channel Driver)

**DIFF-G010: Architectural mismatch - TS uses Scenario DSL instead of Channel Driver**
- **Action**: Rewrite TS mod06 to use Channel Driver abstraction.

**DIFF-G011: TS mod06 positive test missing assertions**
- Same pattern as DIFF-G009.
- **Action**: Add missing assertions.

---

### 2.6 Mod07 (E2E - Use Case DSL)

**DIFF-G012: Architectural mismatch - TS uses Scenario DSL instead of Use-Case DSL**
- Java: `app.shop().placeOrder().orderNumber(...).sku(...).quantity(5).country(...).execute().shouldSucceed()`
- TS: `scenario.given().product()...when().placeOrder()...then().shouldSucceed()`
- **Action**: Rewrite TS mod07 to use Use-Case DSL abstraction.

**DIFF-G013: TS mod07 positive test missing assertions and constants**
- Java/.NET: Explicit constants (SKU, ORDER_NUMBER, COUNTRY), 6 field assertions
- TS: Only orderNumberPrefix and status
- **Action**: Add missing assertions and use explicit test constants.

---

### 2.7 Mod08 (E2E - Scenario DSL)

Architectural layer aligned across all three languages.

**DIFF-G014: TS mod08 has extra negative tests that don't belong**
- Java/.NET: Single `shouldRejectOrderWithNonIntegerQuantity` with value `"3.5"`
- TS: Many additional tests (non-existent SKU, empty SKUs, non-positive quantities, etc.) that belong to mod10
- **Action**: Remove extra tests from TS mod08 negative file. Keep only `shouldRejectOrderWithNonIntegerQuantity`.

---

### 2.8 Mod09 (Smoke - Clock Added)

All consistent across all three languages. No differences.

---

### 2.9 Mod10 (Acceptance - Isolated)

#### Class Coverage

| Test Class | Java | .NET | TS |
|------------|------|------|----|
| PlaceOrderPositiveTest | Y | Y | Y |
| PlaceOrderNegativeTest | Y | Y | Y |
| PlaceOrderPositiveIsolatedTest | Y | Y | Y |
| PlaceOrderNegativeIsolatedTest | Y | Y | Y |

All present.

**DIFF-G015: TS mod10 `shouldApplyFullPriceOnWeekday` missing `.clock().withWeekday()`**
- Java/.NET: `.given()...and().clock().withWeekday()...`
- TS: No `.clock().withWeekday()` in given setup
- **Action**: Add `.and().clock().withWeekday()` to TS.

All other mod10 tests aligned across languages.

---

### 2.10 Mod11 (Contract + E2E)

#### E2E

All consistent: `shouldPlaceOrder` with `scenario.when().placeOrder().then().shouldSucceed()`.

#### Contract - Class Coverage

| Test Class | Java | .NET | TS |
|------------|------|------|----|
| ErpRealContractTest | Y | Y | Y |
| ErpStubContractTest | Y | Y | Y |
| ClockRealContractTest | Y | Y | Y |
| ClockStubContractTest | Y | Y | Y |
| ClockStubContractIsolatedTest | Y | Y | Y |
| TaxRealContractTest | N | N | N |
| TaxStubContractTest | N | N | N |

Tax contract tests missing from all three languages in legacy mod11 (present in latest). Consistent gap.

**DIFF-G016: TS `clock-stub-contract-test` missing `shouldBeAbleToGetTime`**
- Java/.NET: `ClockStubContractTest` inherits `shouldBeAbleToGetTime` from base
- TS: Only has `shouldBeAbleToGetConfiguredTime`
- **Action**: Add `shouldBeAbleToGetTime` to TS `clock-stub-contract-test.spec.ts`.

**DIFF-G017: TS `clock-stub-contract-isolated-test` has extra `shouldBeAbleToGetTime`**
- Java/.NET: Only `shouldBeAbleToGetConfiguredTime` in isolated test
- TS: Has both `shouldBeAbleToGetTime` AND `shouldBeAbleToGetConfiguredTime`
- **Action**: Remove `shouldBeAbleToGetTime` from TS isolated test.

---

## SUMMARY OF REQUIRED CHANGES

### By Language

| Language | Latest Issues | Legacy Issues | Total |
|----------|--------------|---------------|-------|
| **TypeScript** | 12 | 16 | 28 |
| **Java** | 1 (DIFF-L003) | 0 | 1 |
| **.NET** | 1 (DIFF-L003) | 0 | 1 |

### By Area

| Area | Count | DIFF IDs |
|------|-------|----------|
| Architectural mismatches (TS legacy mod03-07) | 5 | DIFF-G001, G004, G007, G010, G012 |
| Extra `.withQuantity(1)` in TS | 4 | DIFF-L004, L005, L006, L007 |
| Missing assertions in TS | 4 | DIFF-G003, G009, G011, G013 |
| Channel scope differences | 3 | DIFF-L008, L009, L010 |
| File structure differences (Api/Ui split) | 3 | DIFF-G002, G005, G008 |
| Missing/extra test methods | 4 | DIFF-L012, L013, G016, G017 |
| Test body logic differences | 3 | DIFF-L001, L011, G015 |
| Test data mismatches | 1 | DIFF-G006 |
| Parameterized test convergence | 1 | DIFF-L003 |
| Extra tests in wrong module | 1 | DIFF-G014 |

### Critical Items

1. **DIFF-G001/G004/G007/G010/G012**: TS legacy mod03-mod07 all use Scenario DSL instead of the expected progressively lower abstraction layers. This is the single most critical issue -- it defeats the course's pedagogical progression.
2. **DIFF-G014**: TS mod08 contains negative tests that belong to mod10, breaking module incrementality.
3. **DIFF-L003**: Negative/zero quantity test structure diverges across all three languages.
4. **DIFF-L008/L009/L010**: TS channel scope mismatch in parameterized tests (runs all rows on UI instead of first-row-only).
