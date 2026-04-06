import { createScenario, ExternalSystemMode } from '../../../../src/test-setup';

const externalSystemMode = (process.env.EXTERNAL_SYSTEM_MODE?.toLowerCase() || 'stub') as ExternalSystemMode;

describe('Tax Stub Contract Test', () => {
  const isStub = externalSystemMode === 'stub';

  (isStub ? it : it.skip)('shouldBeAbleToGetConfiguredTaxRate', async () => {
    const scenario = createScenario({ channel: 'api', externalSystemMode: 'stub' });
    try {
      await scenario
        .given()
        .country()
        .withCode('LALA')
        .withTaxRate('0.23')
        .then()
        .country('LALA')
        .hasCountry('LALA')
        .hasTaxRate(0.23);
    } finally {
      await scenario.close();
    }
  });
});
