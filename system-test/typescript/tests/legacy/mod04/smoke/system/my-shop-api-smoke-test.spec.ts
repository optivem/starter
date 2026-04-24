import { apiTest as test, expect } from '../fixtures.js';

test('shouldBeAbleToGoToMyShop', async ({ myShopApiClient }) => {
    const result = await myShopApiClient.health().checkHealth();
    expect(result.success).toBe(true);
});
