# System Test Comparison — Action Plan (2026-04-16)

**Source:** [TEST-COMPARISON-2026-04-16.md](../reports/TEST-COMPARISON-2026-04-16.md)  
**Date:** 2026-04-16

---

## Priority 1: Missing/Wrong Assertions (Latest)

- [ ] Step 1 (DIFF-L1): **`shouldBeAbleToPlaceOrderForValidInput`** — TS missing entire given-setup and when-params. Java/.NET have `.given().product().withSku("ABC").withUnitPrice(20.00).and().country().withCode("US").withTaxRate(0.10)` and `.when().placeOrder().withSku("ABC").withQuantity(5).withCountry("US")`. TS just has `.when().placeOrder().then().shouldSucceed()`. | TS |

- [ ] Step 2 (DIFF-L8): **`couponUsageCountHasBeenIncrementedAfterItsBeenUsed`** — TS asserts on wrong entity (order instead of coupon). Should be `.and().coupon(code).hasUsedCount(1)` not `.and().order().hasAppliedCouponCode(code)`. | TS |

- [ ] Step 3 (DIFF-L19): **`CancelOrderNegativeIsolatedTest`** — TS missing `.and().order().hasStatus(OrderStatus.PLACED)` after `.shouldFail().errorMessage(BLACKOUT_ERROR)`. Java/.NET have it. | TS |

- [ ] Step 4 (DIFF-L5): **`subtotalPriceShouldBeCalculatedAsTheBasePriceMinusDiscountAmountWhenWeHaveCoupon`** — TS missing `.hasAppliedCoupon()` and `.hasDiscountRate(0.15)` assertions. Java/.NET have them. | TS |

- [ ] Step 5 (DIFF-L6): **`totalPriceShouldBeSubtotalPricePlusTaxAmount`** — TS missing `.hasTaxRate(taxRate)` assertion. Java/.NET have it. | TS |

- [ ] Step 6 (DIFF-L3): **`discountRateShouldBeNotAppliedWhenThereIsNoCoupon`** — TS missing `.withCouponCode(null)` on the when-step. Java/.NET have it. | TS |

---

## Priority 2: Systematic DSL Naming (Latest)

- [ ] Step 7 (DIFF-L4): **`hasAppliedCouponCode` vs `hasAppliedCoupon`** — TS uses `hasAppliedCouponCode` everywhere, Java/.NET use `hasAppliedCoupon`. Rename in TS DSL source + all test files. | TS |

- [ ] Step 8 (DIFF-L2): **`discountRateShouldNotBeAppliedWhenThereIsNoCoupon`** — TS name differs from Java/.NET `discountRateShouldBeNotAppliedWhenThereIsNoCoupon`. | TS |

---

## Priority 3: Test Data Mismatches (Latest)

- [ ] Step 9 (DIFF-L21): **`cannotPublishCouponWithZeroOrNegativeDiscount`** — TS coupon code `'INVALID'`, Java/.NET `'INVALID-COUPON'`. | TS |

- [ ] Step 10 (DIFF-L22): **`cannotPublishCouponWithZeroOrNegativeUsageLimit`** — TS `.withDiscountRate(0.1)`, Java/.NET `.withDiscountRate(0.15)`. | TS |

- [ ] Step 11 (DIFF-L11–L15): **Multiple negative tests** — TS adds `.withQuantity(1)` that Java/.NET don't have. Affected: `shouldRejectOrderWithNonExistentSku`, `shouldRejectOrderWithEmptySku`, `shouldRejectOrderWithInvalidCountry`, `cannotPlaceOrderWithNonExistentCoupon`, `shouldRejectOrderWithEmptyCountry`. | TS vs Java/.NET — decide direction |

---

## Priority 4: Contract Test Fixes (Latest)

- [ ] Step 12 (DIFF-L24): **`ClockStubContractTest.shouldBeAbleToGetTime`** — TS uses `.withTime()` (no argument), Java/.NET use `.withTime("2024-01-02T09:00:00Z")`. | TS |

- [ ] Step 13 (DIFF-L25): **`ClockStubContractIsolatedTest`** — TS has extra `shouldBeAbleToGetTime` that Java/.NET don't have. Remove from TS. | TS |

- [ ] Step 14 (DIFF-L26): **`TaxStubContractTest`** — TS type inconsistency: real uses string `'0.09'`, stub uses number `0.09`. Make consistent. | TS |

---

## Priority 5: Structural Convergence (Latest)

- [ ] Step 15 (DIFF-L7): **`totalPriceShouldBeSubtotalPricePlusTaxAmount`** — TS given-order is `product` then `country`, Java/.NET is `country` then `product`. Reorder TS. | TS |

- [ ] Step 16 (DIFF-L9): **`shouldRejectOrderWithInvalidQuantity`** — Java/.NET have standalone test, TS folds it into parameterized `shouldRejectOrderWithNonIntegerQuantity`. | TS vs Java/.NET — decide direction |

- [ ] Step 17 (DIFF-L10): **Negative/zero quantity tests** — Java has separate tests, .NET/TS use parameterized `shouldRejectOrderWithNonPositiveQuantity`. | Java — recommended: adopt parameterized approach |

- [ ] Step 18 (DIFF-L23): **`ViewOrderNegativeTest` channel scope** — TS only runs API, Java/.NET run API + UI for first row. | TS |

---

## Priority 6: Review Items (Latest)

- [ ] Step 19 (DIFF-L20): **`shouldBeAbleToPublishCouponWithEmptyOptionalFields`** — TS uses `undefined`, Java/.NET use `null`. May be intentional language difference. | Review |

---

## Priority 7: Legacy Fixes

- [ ] Step 20 (DIFF-G3): **mod03 `PlaceOrderNegative`** — TS uses `'3.5'` as invalid quantity, Java/.NET use `"invalid-quantity"`. | TS |

- [ ] Step 21 (DIFF-G5): **mod05 `PlaceOrderPositive`** — TS missing assertions: `hasQuantity`, `hasUnitPrice`, `hasTotalPriceGreaterThanZero`. | TS |

- [ ] Step 22 (DIFF-G8, G9): **mod08 `PlaceOrderPositive`** — TS splits into two tests, Java/.NET have one combined. TS also missing assertions. | TS vs Java/.NET — decide structure |

- [ ] Step 23 (DIFF-G16): **mod11 `BaseClockContractTest.shouldBeAbleToGetTime`** — Java uses `.withTime()` (no argument), .NET/TS use `.withTime("2024-01-02T09:00:00Z")`. | Java |

- [ ] Step 24 (DIFF-G17): **mod11 `ClockStubContractIsolatedTest`** — TS has extra `shouldBeAbleToGetTime` (same as latest Step 13). | TS |

- [ ] Step 25 (DIFF-G7): **mod07 base test class name** — Java `BaseUseCaseDslTest` vs .NET `BaseSystemDslTest`. | .NET |

---

## Structural Observations (No Action)

- **DIFF-G1, G2, G4**: TS legacy mod03/mod04 uses unified channel-aware tests while Java/.NET use separate Api/Ui classes. TS is structurally ahead.
- **DIFF-G6**: Java/.NET mod07 use use-case DSL while TS uses scenario DSL. TS is at a later DSL stage.
- **DIFF-G12, G13**: mod10 isolated tests are consistent across all languages.
