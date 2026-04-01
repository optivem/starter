import { loadConfiguration } from '../../../../config/configuration-loader';

describe('Clock Smoke Test', () => {
  const config = loadConfiguration();

  it('shouldBeAbleToGoToClock', async () => {
    const clockUrl = config.externalSystems.clock.url;

    if (clockUrl === 'none') {
      // Clock is not available in real mode (uses system clock)
      return;
    }

    const response = await fetch(`${clockUrl}/health`);
    expect(response.status).toBe(200);
  });
});
