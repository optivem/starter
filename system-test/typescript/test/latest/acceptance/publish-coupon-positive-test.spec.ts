import { createScenario, Channel, ExternalSystemMode } from '../../../src/test-setup';

const channel = (process.env.CHANNEL?.toLowerCase() || 'api') as Channel;
const externalSystemMode = (process.env.EXTERNAL_SYSTEM_MODE?.toLowerCase() || 'stub') as ExternalSystemMode;

describe('PublishCoupon Positive Test', () => {
  it(`shouldBeAbleToPublishValidCoupon_${channel.toUpperCase()}`, async () => {
    const scenario = createScenario({ channel, externalSystemMode });
    try {
      await scenario
        .when()
        .publishCoupon()
        .withCouponCode('SUMMER2025')
        .withDiscountRate(0.15)
        .withValidFrom('2024-06-01T00:00:00Z')
        .withValidTo('2024-08-31T23:59:59Z')
        .withUsageLimit(100)
        .then()
        .shouldSucceed();
    } finally {
      await scenario.close();
    }
  });

  it(`shouldBeAbleToPublishCouponWithEmptyOptionalFields_${channel.toUpperCase()}`, async () => {
    const scenario = createScenario({ channel, externalSystemMode });
    try {
      await scenario
        .when()
        .publishCoupon()
        .withCouponCode('SUMMER2025')
        .withDiscountRate(0.15)
        .withValidFrom(undefined)
        .withValidTo(undefined)
        .withUsageLimit(undefined)
        .then()
        .shouldSucceed();
    } finally {
      await scenario.close();
    }
  });

  it(`shouldBeAbleToCorrectlySaveCoupon_${channel.toUpperCase()}`, async () => {
    const scenario = createScenario({ channel, externalSystemMode });
    try {
      await scenario
        .when()
        .publishCoupon()
        .withCouponCode('SUMMER2025')
        .withDiscountRate(0.15)
        .withValidFrom('2024-06-01T00:00:00Z')
        .withValidTo('2024-08-31T23:59:00Z')
        .withUsageLimit(100)
        .then()
        .shouldSucceed();
    } finally {
      await scenario.close();
    }
  });

  if (channel === 'api') {
    it('shouldPublishCouponSuccessfully_API', async () => {
      const scenario = createScenario({ channel: 'api', externalSystemMode });
      try {
        await scenario
          .when()
          .publishCoupon()
          .withCouponCode('SAVE10')
          .withDiscountRate(0.1)
          .then()
          .shouldSucceed();
      } finally {
        await scenario.close();
      }
    });
  }
});
