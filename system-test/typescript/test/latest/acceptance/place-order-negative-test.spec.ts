import { randomUUID } from 'node:crypto';
import { chromium, Browser } from 'playwright';
import { createScenario, Channel, ExternalSystemMode } from '../../../src/test-setup';

const channel = (process.env.CHANNEL?.toLowerCase() || 'api') as Channel;
const externalSystemMode = (process.env.EXTERNAL_SYSTEM_MODE?.toLowerCase() || 'stub') as ExternalSystemMode;

describe('PlaceOrder Negative Test', () => {
  let browser: Browser;

  beforeAll(async () => {
    if (channel === 'ui') {
      browser = await chromium.launch();
    }
  });

  afterAll(async () => {
    await browser?.close();
  });

  const nonIntegerQuantities = ['3.5', 'lala', 'invalid-quantity'];
  const emptySkus = ['', '   '];
  const nonPositiveQuantities = ['-10', '-1', '0'];
  const emptyQuantities = ['', '   '];

  nonIntegerQuantities.forEach((qty) => {
    it(`shouldRejectOrderWithNonIntegerQuantity_${channel.toUpperCase()}_${qty}`, async () => {
      const scenario = createScenario({ channel, externalSystemMode, browser });
      try {
        await scenario
          .when()
          .placeOrder()
          .withQuantity(qty)
          .then()
          .shouldFail()
          .errorMessage('The request contains one or more validation errors')
          .fieldErrorMessage('quantity', 'Quantity must be an integer');
      } finally {
        await scenario.close();
      }
    });
  });

  it(`shouldRejectOrderForNonExistentProduct_${channel.toUpperCase()}`, async () => {
    const scenario = createScenario({ channel, externalSystemMode, browser });
    try {
      await scenario
        .when()
        .placeOrder()
        .withSku('NON-EXISTENT-SKU-12345')
        .withQuantity(1)
        .then()
        .shouldFail()
        .errorMessage('The request contains one or more validation errors')
        .fieldErrorMessage('sku', 'Product does not exist for SKU: NON-EXISTENT-SKU-12345');
    } finally {
      await scenario.close();
    }
  });

  emptySkus.forEach((sku) => {
    it(`shouldRejectOrderWithEmptySku_${channel.toUpperCase()}_"${sku}"`, async () => {
      const scenario = createScenario({ channel, externalSystemMode, browser });
      try {
        await scenario
          .when()
          .placeOrder()
          .withSku(sku)
          .withQuantity(1)
          .then()
          .shouldFail()
          .errorMessage('The request contains one or more validation errors')
          .fieldErrorMessage('sku', 'SKU must not be empty');
      } finally {
        await scenario.close();
      }
    });
  });

  nonPositiveQuantities.forEach((qty) => {
    it(`shouldRejectOrderWithNonPositiveQuantity_${channel.toUpperCase()}_${qty}`, async () => {
      const scenario = createScenario({ channel, externalSystemMode, browser });
      try {
        await scenario
          .when()
          .placeOrder()
          .withQuantity(qty)
          .then()
          .shouldFail()
          .errorMessage('The request contains one or more validation errors')
          .fieldErrorMessage('quantity', 'Quantity must be positive');
      } finally {
        await scenario.close();
      }
    });
  });

  emptyQuantities.forEach((qty) => {
    it(`shouldRejectOrderWithEmptyQuantity_${channel.toUpperCase()}_"${qty}"`, async () => {
      const scenario = createScenario({ channel, externalSystemMode, browser });
      try {
        await scenario
          .when()
          .placeOrder()
          .withQuantity(qty)
          .then()
          .shouldFail()
          .errorMessage('The request contains one or more validation errors')
          .fieldErrorMessage('quantity', 'Quantity must not be empty');
      } finally {
        await scenario.close();
      }
    });
  });

  const emptyCountries = ['', '   '];

  emptyCountries.forEach((country) => {
    it(`shouldRejectOrderWithEmptyCountry_${channel.toUpperCase()}_"${country}"`, async () => {
      const scenario = createScenario({ channel, externalSystemMode, browser });
      try {
        await scenario
          .when()
          .placeOrder()
          .withQuantity(1)
          .withCountry(country)
          .then()
          .shouldFail()
          .errorMessage('The request contains one or more validation errors')
          .fieldErrorMessage('country', 'Country must not be empty');
      } finally {
        await scenario.close();
      }
    });
  });

  it(`shouldRejectOrderWithInvalidCountry_${channel.toUpperCase()}`, async () => {
    const scenario = createScenario({ channel, externalSystemMode, browser });
    try {
      await scenario
        .when()
        .placeOrder()
        .withQuantity(1)
        .withCountry('XX')
        .then()
        .shouldFail()
        .errorMessage('The request contains one or more validation errors')
        .fieldErrorMessage('country', 'Country does not exist: XX');
    } finally {
      await scenario.close();
    }
  });

  it(`cannotPlaceOrderWithNonExistentCoupon_${channel.toUpperCase()}`, async () => {
    const scenario = createScenario({ channel, externalSystemMode, browser });
    try {
      await scenario
        .when()
        .placeOrder()
        .withQuantity(1)
        .withCouponCode('NON-EXISTENT-COUPON')
        .then()
        .shouldFail()
        .errorMessage('The request contains one or more validation errors')
        .fieldErrorMessage('couponCode', 'Coupon code NON-EXISTENT-COUPON does not exist');
    } finally {
      await scenario.close();
    }
  });

  it(`cannotPlaceOrderWithCouponThatHasExceededUsageLimit_${channel.toUpperCase()}`, async () => {
    const scenario = createScenario({ channel, externalSystemMode, browser });
    try {
      const couponCode = `LIMITED-${randomUUID().slice(0, 8)}`;

      // First place an order with the coupon to exhaust usage
      await scenario
        .given()
        .coupon()
        .withCode(couponCode)
        .withDiscountRate(0.1)
        .withUsageLimit(1)
        .when()
        .placeOrder()
        .withQuantity(1)
        .withCouponCode(couponCode)
        .then()
        .shouldSucceed();

      // Now try to use same coupon again — should fail
      const scenario2 = createScenario({ channel: 'api', externalSystemMode });
      try {
        await scenario2
          .when()
          .placeOrder()
          .withQuantity(1)
          .withCouponCode(couponCode)
          .then()
          .shouldFail()
          .errorMessage('The request contains one or more validation errors')
          .fieldErrorMessage('couponCode', `Coupon code ${couponCode} has exceeded its usage limit`);
      } finally {
        await scenario2.close();
      }
    } finally {
      await scenario.close();
    }
  });

  // API-only test: null quantity
  if (channel === 'api') {
    it('shouldRejectOrderWithNullQuantity_API', async () => {
      const scenario = createScenario({ channel: 'api', externalSystemMode });
      try {
        await scenario
          .when()
          .placeOrder()
          .withQuantity(null)
          .then()
          .shouldFail()
          .errorMessage('The request contains one or more validation errors')
          .fieldErrorMessage('quantity', 'Quantity must not be empty');
      } finally {
        await scenario.close();
      }
    });
  }
});
