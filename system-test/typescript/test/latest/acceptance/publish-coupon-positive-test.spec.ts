import { randomUUID } from 'node:crypto';
import { createScenario, Channel, ExternalSystemMode } from '../../../src/test-setup';

const channel = (process.env.CHANNEL?.toLowerCase() || 'api') as Channel;
const externalSystemMode = (process.env.EXTERNAL_SYSTEM_MODE?.toLowerCase() || 'stub') as ExternalSystemMode;

function uniqueCode(prefix: string): string {
  return `${prefix}-${randomUUID().slice(0, 8)}`;
}

describe('PublishCoupon Positive Test', () => {
  it(`shouldBeAbleToPublishValidCoupon_${channel.toUpperCase()}`, async () => {
    const scenario = createScenario({ channel, externalSystemMode });
    try {
      await scenario
        .when()
        .publishCoupon()
        .withCode(uniqueCode('SAVE10'))
        .withDiscountRate(0.1)
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
        .withCode(uniqueCode('MINIMAL'))
        .withDiscountRate(0.05)
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
        .withCode(uniqueCode('FULL'))
        .withDiscountRate(0.15)
        .withValidFrom('2025-01-01T00:00:00Z')
        .withValidTo('2025-12-31T23:59:59Z')
        .withUsageLimit(100)
        .then()
        .shouldSucceed();
    } finally {
      await scenario.close();
    }
  });
});
