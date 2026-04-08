import { chromium, Browser } from 'playwright';
import { createScenario, Channel, ExternalSystemMode } from '../../../src/test-setup';
import { OrderStatus } from '../../../src/common/dtos';

const channel = (process.env.CHANNEL?.toLowerCase() || 'api') as Channel;
const externalSystemMode = (process.env.EXTERNAL_SYSTEM_MODE?.toLowerCase() || 'stub') as ExternalSystemMode;

const timesOutsideBlackout = [
  '2024-12-31T21:59:59Z',
  '2024-12-31T22:30:01Z',
  '2024-12-31T10:00:00Z',
  '2025-01-01T22:15:00Z',
];

describe('CancelOrder Positive Isolated Test', () => {
  let browser: Browser;

  beforeAll(async () => {
    if (channel === 'ui') {
      browser = await chromium.launch();
    }
  });

  afterAll(async () => {
    await browser?.close();
  });

  it.each(timesOutsideBlackout)(
    `shouldBeAbleToCancelOrderOutsideOfBlackoutPeriod31stDecBetween2200And2230_${channel.toUpperCase()}_%s`,
    async (time) => {
      const scenario = createScenario({ channel, externalSystemMode, browser });
      try {
        await scenario
          .given()
          .clock()
          .withTime(time)
          .and()
          .order()
          .withStatus(OrderStatus.PLACED)
          .when()
          .cancelOrder()
          .then()
          .shouldSucceed();
      } finally {
        await scenario.close();
      }
    },
  );
});
