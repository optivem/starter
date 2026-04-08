import { loadConfiguration } from '../../../../../config/configuration-loader';

describe('Tax Smoke Test', () => {
  it('shouldBeAbleToGoToTax', async () => {
    const config = loadConfiguration();

    const response = await fetch(`${config.externalSystems.tax.url}/health`);

    expect(response.status).toBe(200);
  });
});
