import { chromium, Browser } from 'playwright';
import { createScenario, Channel, ExternalSystemMode } from '../../../src/test-setup';
import { OrderStatus } from '../../../src/common/dtos';

const channel = (process.env.CHANNEL?.toLowerCase() || 'api') as Channel;
const externalSystemMode = (process.env.EXTERNAL_SYSTEM_MODE?.toLowerCase() || 'stub') as ExternalSystemMode;

const timesInsideBlackout = [
  '2024-12-31T22:00:00Z',
  '2026-12-31T22:00:01Z',
  '2025-12-31T22:15:00Z',
  '2028-12-31T22:29:59Z',
  '2021-12-31T22:30:00Z',
];

const BLACKOUT_ERROR = 'Order cancellation is not allowed on December 31st between 22:00 and 23:00';

describe('CancelOrder Negative Isolated Test', () => {
  let browser: Browser;

  beforeAll(async () => {
    if (channel === 'ui') {
      browser = await chromium.launch();
    }
  });

  afterAll(async () => {
    await browser?.close();
  });

  it.each(timesInsideBlackout)(
    `cannotCancelAnOrderOn31stDecBetween2200And2230_${channel.toUpperCase()}_%s`,
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
          .shouldFail()
          .errorMessage(BLACKOUT_ERROR);
      } finally {
        await scenario.close();
      }
    },
  );
});
