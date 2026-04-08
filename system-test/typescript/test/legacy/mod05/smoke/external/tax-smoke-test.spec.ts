import { createScenario } from '../../../../../src/test-setup';

describe('Tax Smoke Test', () => {
  it('shouldBeAbleToGoToTax', async () => {
    const scenario = createScenario();
    try {
      await scenario.assume().tax().shouldBeRunning();
    } finally {
      await scenario.close();
    }
  });
});
