import { chromium, Browser } from 'playwright';
import { createScenario, Channel, ExternalSystemMode } from '../../../src/test-setup';

const channel = (process.env.CHANNEL?.toLowerCase() || 'api') as Channel;
const externalSystemMode = (process.env.EXTERNAL_SYSTEM_MODE?.toLowerCase() || 'stub') as ExternalSystemMode;

describe('PlaceOrder Negative Isolated Test', () => {
  let browser: Browser;

  beforeAll(async () => {
    if (channel === 'ui') {
      browser = await chromium.launch();
    }
  });

  afterAll(async () => {
    await browser?.close();
  });

  it(`cannotPlaceOrderWithExpiredCoupon_${channel.toUpperCase()}`, async () => {
    const scenario = createScenario({ channel, externalSystemMode, browser });
    try {
      await scenario
        .given()
        .clock()
        .withTime('2023-09-01T12:00:00Z')
        .and()
        .coupon()
        .withCode('SUMMER2023')
        .withDiscountRate(0.15)
        .withValidFrom('2023-06-01T00:00:00Z')
        .withValidTo('2023-08-31T23:59:59Z')
        .when()
        .placeOrder()
        .withCouponCode('SUMMER2023')
        .then()
        .shouldFail()
        .errorMessage('The request contains one or more validation errors')
        .fieldErrorMessage('couponCode', 'Coupon code SUMMER2023 has expired');
    } finally {
      await scenario.close();
    }
  });

  it(`shouldRejectOrderPlacedAtYearEnd_${channel.toUpperCase()}`, async () => {
    const scenario = createScenario({ channel, externalSystemMode, browser });
    try {
      await scenario
        .given()
        .clock()
        .withTime('2026-12-31T23:59:30Z')
        .when()
        .placeOrder()
        .then()
        .shouldFail()
        .errorMessage('Orders cannot be placed between 23:59 and 00:00 on December 31st');
    } finally {
      await scenario.close();
    }
  });
});
