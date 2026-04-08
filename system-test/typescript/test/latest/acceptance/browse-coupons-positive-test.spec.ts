import { createScenario, Channel, ExternalSystemMode } from '../../../src/test-setup';

const channel = (process.env.CHANNEL?.toLowerCase() || 'api') as Channel;
const externalSystemMode = (process.env.EXTERNAL_SYSTEM_MODE?.toLowerCase() || 'stub') as ExternalSystemMode;

describe('BrowseCoupons Positive Test', () => {
  it(`shouldBeAbleToBrowseCoupons_${channel.toUpperCase()}`, async () => {
    const scenario = createScenario({ channel, externalSystemMode });
    try {
      await scenario
        .when()
        .browseCoupons()
        .then()
        .shouldSucceed();
    } finally {
      await scenario.close();
    }
  });

  it(`publishedCouponShouldAppearInList_${channel.toUpperCase()}`, async () => {
    const couponCode = 'BROWSE10';
    const scenario = createScenario({ channel, externalSystemMode });
    try {
      await scenario
        .given()
        .coupon()
        .withCouponCode(couponCode)
        .withDiscountRate(0.1)
        .when()
        .browseCoupons()
        .then()
        .shouldSucceed()
        .coupons()
        .containsCouponWithCode(couponCode);
    } finally {
      await scenario.close();
    }
  });
});
