import { createScenario, ExternalSystemMode } from '../../../../src/test-setup';

const externalSystemMode = (process.env.EXTERNAL_SYSTEM_MODE?.toLowerCase() || 'stub') as ExternalSystemMode;

describe('Tax Smoke Test', () => {
  it('shouldBeAbleToGoToTax', async () => {
    const scenario = createScenario({ channel: 'api', externalSystemMode });
    try {
      await scenario.assume().tax().shouldBeRunning();
    } finally {
      await scenario.close();
    }
  });
});
