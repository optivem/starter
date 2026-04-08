import { createScenario, Channel, ExternalSystemMode } from '../../../src/test-setup';

const channel = (process.env.CHANNEL?.toLowerCase() || 'api') as Channel;
const externalSystemMode = (process.env.EXTERNAL_SYSTEM_MODE?.toLowerCase() || 'stub') as ExternalSystemMode;

describe('PublishCoupon Negative Test', () => {
  const emptyCodes = ['', '   '];
  const nonPositiveDiscountRates = [0.0, -0.01, -0.15];
  const aboveOneDiscountRates = [1.01, 2.0];

  if (channel === 'api') {
    emptyCodes.forEach((code) => {
      it(`shouldRejectCouponWithBlankCode_API_"${code}"`, async () => {
        const scenario = createScenario({ channel: 'api', externalSystemMode });
        try {
          await scenario
            .when()
            .publishCoupon()
            .withCouponCode(code)
            .withDiscountRate(0.1)
            .then()
            .shouldFail()
            .errorMessage('The request contains one or more validation errors')
            .fieldErrorMessage('code', 'Coupon code must not be blank');
        } finally {
          await scenario.close();
        }
      });
    });
  }

  nonPositiveDiscountRates.forEach((discountRate) => {
    it(`cannotPublishCouponWithZeroOrNegativeDiscount_${channel.toUpperCase()}_${discountRate}`, async () => {
      const scenario = createScenario({ channel, externalSystemMode });
      try {
        await scenario
          .when()
          .publishCoupon()
          .withCouponCode('INVALID')
          .withDiscountRate(discountRate)
          .then()
          .shouldFail()
          .errorMessage('The request contains one or more validation errors')
          .fieldErrorMessage('discountRate', 'Discount rate must be greater than 0.00');
      } finally {
        await scenario.close();
      }
    });
  });

  aboveOneDiscountRates.forEach((discountRate) => {
    it(`cannotPublishCouponWithDiscountGreaterThan100percent_${channel.toUpperCase()}_${discountRate}`, async () => {
      const scenario = createScenario({ channel, externalSystemMode });
      try {
        await scenario
          .when()
          .publishCoupon()
          .withCouponCode('INVALID')
          .withDiscountRate(discountRate)
          .then()
          .shouldFail()
          .errorMessage('The request contains one or more validation errors')
          .fieldErrorMessage('discountRate', 'Discount rate must be at most 1.00');
      } finally {
        await scenario.close();
      }
    });
  });

  it(`cannotPublishCouponWithDuplicateCouponCode_${channel.toUpperCase()}`, async () => {
    const dupCode = 'EXISTING-COUPON';
    const scenario = createScenario({ channel, externalSystemMode });
    try {
      await scenario
        .given()
        .coupon()
        .withCouponCode(dupCode)
        .withDiscountRate(0.1)
        .when()
        .publishCoupon()
        .withCouponCode(dupCode)
        .withDiscountRate(0.2)
        .then()
        .shouldFail()
        .errorMessage('The request contains one or more validation errors')
        .fieldErrorMessage('couponCode', `Coupon code ${dupCode} already exists`);
    } finally {
      await scenario.close();
    }
  });

  const nonPositiveUsageLimits = [0, -1, -100];

  nonPositiveUsageLimits.forEach((usageLimit) => {
    it(`cannotPublishCouponWithZeroOrNegativeUsageLimit_${channel.toUpperCase()}_${usageLimit}`, async () => {
      const scenario = createScenario({ channel, externalSystemMode });
      try {
        await scenario
          .when()
          .publishCoupon()
          .withCouponCode('INVALID-LIMIT')
          .withDiscountRate(0.1)
          .withUsageLimit(usageLimit)
          .then()
          .shouldFail()
          .errorMessage('The request contains one or more validation errors')
          .fieldErrorMessage('usageLimit', 'Usage limit must be positive');
      } finally {
        await scenario.close();
      }
    });
  });
});
