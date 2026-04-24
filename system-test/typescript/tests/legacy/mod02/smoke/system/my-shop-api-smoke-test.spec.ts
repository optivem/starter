import { test, expect } from '@playwright/test';
import { getMyShopApiBaseUrl } from '../../base/BaseRawTest.js';

test('shouldBeAbleToGoToMyShop', async () => {
    const response = await fetch(`${getMyShopApiBaseUrl()}/health`);
    expect(response.status).toBe(200);
});
