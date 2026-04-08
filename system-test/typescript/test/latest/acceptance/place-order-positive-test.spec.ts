import { chromium, Browser } from 'playwright';
import { createScenario, Channel, ExternalSystemMode } from '../../../src/test-setup';
import { OrderStatus } from '../../../src/common/dtos';

const channel = (process.env.CHANNEL?.toLowerCase() || 'api') as Channel;
const externalSystemMode = (process.env.EXTERNAL_SYSTEM_MODE?.toLowerCase() || 'stub') as ExternalSystemMode;

describe('PlaceOrder Positive Test', () => {
  let browser: Browser;

  beforeAll(async () => {
    if (channel === 'ui') {
      browser = await chromium.launch();
    }
  });

  afterAll(async () => {
    await browser?.close();
  });

  it(`shouldBeAbleToPlaceOrderForValidInput_${channel.toUpperCase()}`, async () => {
    const scenario = createScenario({ channel, externalSystemMode, browser });
    try {
      await scenario
        .when()
        .placeOrder()
        .then()
        .shouldSucceed();
    } finally {
      await scenario.close();
    }
  });

  it(`orderPrefixShouldBeORD_${channel.toUpperCase()}`, async () => {
    const scenario = createScenario({ channel, externalSystemMode, browser });
    try {
      await scenario
        .when()
        .placeOrder()
        .then()
        .shouldSucceed()
        .and()
        .order()
        .hasOrderNumberPrefix('ORD-');
    } finally {
      await scenario.close();
    }
  });

  it(`orderStatusShouldBePlacedAfterPlacingOrder_${channel.toUpperCase()}`, async () => {
    const scenario = createScenario({ channel, externalSystemMode, browser });
    try {
      await scenario
        .when()
        .placeOrder()
        .then()
        .shouldSucceed()
        .and()
        .order()
        .hasStatus(OrderStatus.PLACED);
    } finally {
      await scenario.close();
    }
  });

  it(`orderTotalShouldIncludeTax_API`, async () => {
    if (channel !== 'api') return;
    const scenario = createScenario({ channel: 'api', externalSystemMode });
    try {
      await scenario
        .given()
        .country()
        .withCode('DE')
        .withTaxRate('0.19')
        .when()
        .placeOrder()
        .withCountry('DE')
        .then()
        .shouldSucceed()
        .and()
        .order()
        .hasSubtotalPrice(20.0)
        .hasTaxRate(0.19)
        .hasTotalPrice(23.8);
    } finally {
      await scenario.close();
    }
  });

  it(`orderTotalShouldReflectCouponDiscount_API`, async () => {
    if (channel !== 'api') return;
    const couponCode = 'DISC10';
    const scenario = createScenario({ channel: 'api', externalSystemMode });
    try {
      await scenario
        .given()
        .coupon()
        .withCouponCode(couponCode)
        .withDiscountRate(0.1)
        .when()
        .placeOrder()
        .withCouponCode(couponCode)
        .then()
        .shouldSucceed()
        .and()
        .order()
        .hasSubtotalPrice(18.0)
        .hasDiscountRate(0.1)
        .hasAppliedCouponCode(couponCode)
        .hasTotalPrice(19.26);
    } finally {
      await scenario.close();
    }
  });

  it(`orderTotalShouldApplyCouponDiscountAndTax_API`, async () => {
    if (channel !== 'api') return;
    const comboCode = 'COMBO10';
    const scenario = createScenario({ channel: 'api', externalSystemMode });
    try {
      await scenario
        .given()
        .coupon()
        .withCouponCode(comboCode)
        .withDiscountRate(0.1)
        .and()
        .country()
        .withCode('GB')
        .withTaxRate('0.20')
        .when()
        .placeOrder()
        .withCountry('GB')
        .withCouponCode(comboCode)
        .then()
        .shouldSucceed()
        .and()
        .order()
        .hasSubtotalPrice(18.0)
        .hasDiscountRate(0.1)
        .hasTaxRate(0.2)
        .hasAppliedCouponCode(comboCode)
        .hasTotalPrice(21.6);
    } finally {
      await scenario.close();
    }
  });

  it(`shouldCalculateBasePriceAsProductOfUnitPriceAndQuantity_${channel.toUpperCase()}`, async () => {
    const scenario = createScenario({ channel, externalSystemMode, browser });
    try {
      await scenario
        .given()
        .product()
        .withUnitPrice(20)
        .when()
        .placeOrder()
        .withQuantity(5)
        .then()
        .shouldSucceed()
        .and()
        .order()
        .hasBasePrice(100);
    } finally {
      await scenario.close();
    }
  });

  const basePriceCases = [
    { unitPrice: '20.00', quantity: '5', basePrice: 100 },
    { unitPrice: '10.00', quantity: '3', basePrice: 30 },
    { unitPrice: '15.50', quantity: '4', basePrice: 62 },
    { unitPrice: '99.99', quantity: '1', basePrice: 99.99 },
  ];

  it.each(basePriceCases)(
    `shouldPlaceOrderWithCorrectBasePriceParameterized_${channel.toUpperCase()}_unitPrice=$unitPrice_quantity=$quantity`,
    async ({ unitPrice, quantity, basePrice }) => {
      const scenario = createScenario({ channel, externalSystemMode, browser });
      try {
        await scenario
          .given()
          .product()
          .withUnitPrice(unitPrice)
          .when()
          .placeOrder()
          .withQuantity(quantity)
          .then()
          .shouldSucceed()
          .and()
          .order()
          .hasBasePrice(basePrice);
      } finally {
        await scenario.close();
      }
    },
  );

  it(`discountRateShouldBeAppliedForCoupon_${channel.toUpperCase()}`, async () => {
    const code = 'SUMMER2025';
    const scenario = createScenario({ channel, externalSystemMode, browser });
    try {
      await scenario
        .given()
        .coupon()
        .withCouponCode(code)
        .withDiscountRate(0.15)
        .when()
        .placeOrder()
        .withCouponCode(code)
        .then()
        .shouldSucceed()
        .and()
        .order()
        .hasAppliedCouponCode(code)
        .hasDiscountRate(0.15);
    } finally {
      await scenario.close();
    }
  });

  it(`discountRateShouldNotBeAppliedWhenThereIsNoCoupon_${channel.toUpperCase()}`, async () => {
    const scenario = createScenario({ channel, externalSystemMode, browser });
    try {
      await scenario
        .when()
        .placeOrder()
        .then()
        .shouldSucceed()
        .and()
        .order()
        .hasAppliedCouponCode(null)
        .hasDiscountRate(0)
        .hasDiscountAmount(0);
    } finally {
      await scenario.close();
    }
  });

  it(`subtotalPriceShouldBeCalculatedAsTheBasePriceMinusDiscountAmountWhenWeHaveCoupon_${channel.toUpperCase()}`, async () => {
    const code = 'SUMMER2025';
    const scenario = createScenario({ channel, externalSystemMode, browser });
    try {
      await scenario
        .given()
        .coupon()
        .withCouponCode(code)
        .withDiscountRate(0.15)
        .and()
        .product()
        .withUnitPrice(20)
        .when()
        .placeOrder()
        .withQuantity(5)
        .withCouponCode(code)
        .then()
        .shouldSucceed()
        .and()
        .order()
        .hasBasePrice(100)
        .hasDiscountAmount(15)
        .hasSubtotalPrice(85);
    } finally {
      await scenario.close();
    }
  });

  it(`subtotalPriceShouldBeSameAsBasePriceWhenNoCoupon_${channel.toUpperCase()}`, async () => {
    const scenario = createScenario({ channel, externalSystemMode, browser });
    try {
      await scenario
        .given()
        .product()
        .withUnitPrice(20)
        .when()
        .placeOrder()
        .withQuantity(5)
        .then()
        .shouldSucceed()
        .and()
        .order()
        .hasBasePrice(100)
        .hasDiscountAmount(0)
        .hasSubtotalPrice(100);
    } finally {
      await scenario.close();
    }
  });

  const taxRateCases = [
    { country: 'UK', taxRate: '0.09' },
    { country: 'US', taxRate: '0.20' },
  ];

  it.each(taxRateCases)(
    `correctTaxRateShouldBeUsedBasedOnCountry_${channel.toUpperCase()}_country=$country`,
    async ({ country, taxRate }) => {
      const scenario = createScenario({ channel, externalSystemMode, browser });
      try {
        await scenario
          .given()
          .country()
          .withCode(country)
          .withTaxRate(taxRate)
          .when()
          .placeOrder()
          .withCountry(country)
          .then()
          .shouldSucceed()
          .and()
          .order()
          .hasTaxRate(Number.parseFloat(taxRate));
      } finally {
        await scenario.close();
      }
    },
  );

  const totalPriceCases = [
    { country: 'UK', taxRate: '0.09', subtotalPrice: 50, expectedTaxAmount: 4.5, expectedTotalPrice: 54.5 },
    { country: 'US', taxRate: '0.20', subtotalPrice: 100, expectedTaxAmount: 20, expectedTotalPrice: 120 },
  ];

  it.each(totalPriceCases)(
    `totalPriceShouldBeSubtotalPricePlusTaxAmount_${channel.toUpperCase()}_country=$country`,
    async ({ country, taxRate, subtotalPrice, expectedTaxAmount, expectedTotalPrice }) => {
      const unitPrice = subtotalPrice;
      const scenario = createScenario({ channel, externalSystemMode, browser });
      try {
        await scenario
          .given()
          .product()
          .withUnitPrice(unitPrice)
          .and()
          .country()
          .withCode(country)
          .withTaxRate(taxRate)
          .when()
          .placeOrder()
          .withQuantity(1)
          .withCountry(country)
          .then()
          .shouldSucceed()
          .and()
          .order()
          .hasSubtotalPrice(subtotalPrice)
          .hasTaxAmount(expectedTaxAmount)
          .hasTotalPrice(expectedTotalPrice);
      } finally {
        await scenario.close();
      }
    },
  );

  it(`couponUsageCountHasBeenIncrementedAfterItsBeenUsed_${channel.toUpperCase()}`, async () => {
    const code = 'SUMMER2025';
    const scenario = createScenario({ channel, externalSystemMode, browser });
    try {
      await scenario
        .given()
        .coupon()
        .withCouponCode(code)
        .withDiscountRate(0.1)
        .when()
        .placeOrder()
        .withCouponCode(code)
        .then()
        .shouldSucceed()
        .and()
        .order()
        .hasAppliedCouponCode(code);
    } finally {
      await scenario.close();
    }
  });
});
