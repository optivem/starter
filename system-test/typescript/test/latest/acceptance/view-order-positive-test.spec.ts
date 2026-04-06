import { chromium, Browser } from 'playwright';
import { createScenario, Channel, ExternalSystemMode } from '../../../src/test-setup';

const channel = (process.env.CHANNEL?.toLowerCase() || 'api') as Channel;
const externalSystemMode = (process.env.EXTERNAL_SYSTEM_MODE?.toLowerCase() || 'stub') as ExternalSystemMode;

describe('ViewOrder Positive Test', () => {
  let browser: Browser;

  beforeAll(async () => {
    if (channel === 'ui') {
      browser = await chromium.launch();
    }
  });

  afterAll(async () => {
    await browser?.close();
  });

  it(`shouldBeAbleToViewOrder_${channel.toUpperCase()}`, async () => {
    const scenario = createScenario({ channel, externalSystemMode, browser });
    try {
      await scenario
        .given()
        .order()
        .when()
        .viewOrder()
        .then()
        .shouldSucceed();
    } finally {
      await scenario.close();
    }
  });
});
