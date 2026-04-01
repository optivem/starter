import { loadConfiguration } from '../../../../config/configuration-loader';

describe('ERP Smoke Test', () => {
  const config = loadConfiguration();

  it('shouldBeAbleToGoToErp', async () => {
    const response = await fetch(`${config.externalSystems.erp.url}/health`);
    expect(response.status).toBe(200);
  });
});
