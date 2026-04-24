import { uiTest as test, expect } from '../fixtures.js';

test('shouldBeAbleToGoToMyShop', async ({ myShopPage, myShopUiUrl }) => {
    const response = await myShopPage.goto(myShopUiUrl);
    expect(response?.status()).toBe(200);
});
