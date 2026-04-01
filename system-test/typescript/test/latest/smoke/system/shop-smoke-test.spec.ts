import { loadConfiguration } from '../../../../config/configuration-loader';

describe('Shop Smoke Test', () => {
  const config = loadConfiguration();

  it('shouldBeAbleToGoToShop_API', async () => {
    const response = await fetch(`${config.shop.backendApiUrl}/health`);
    expect(response.status).toBe(200);
    const body = await response.json();
    expect(body.status).toBe('UP');
  });

  it('shouldBeAbleToGoToShop_UI', async () => {
    const response = await fetch(config.shop.frontendUrl);
    expect(response.status).toBe(200);
    const body = await response.text();
    expect(body).toContain('<html');
  });
});
