import { createScenario } from '../../../../src/test-setup';

describe('Tax Real Contract Test', () => {
  it('shouldBeAbleToGetTaxRate', async () => {
    const scenario = createScenario({ channel: 'api', externalSystemMode: 'real' });
    try {
      await scenario
        .given()
        .country()
        .withCode('US')
        .withTaxRate('0.09')
        .then()
        .country('US')
        .hasTaxRateIsPositive();
    } finally {
      await scenario.close();
    }
  });
});
