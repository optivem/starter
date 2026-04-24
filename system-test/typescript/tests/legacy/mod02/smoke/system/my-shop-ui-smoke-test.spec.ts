import { test, expect } from '@playwright/test';
import { getMyShopUiBaseUrl, setUpMyShopBrowser, tearDownMyShopBrowser, type MyShopBrowser } from '../../base/BaseRawTest.js';

test('shouldBeAbleToGoToMyShop', async () => {
    let myShopBrowser: MyShopBrowser | null = null;
    try {
        myShopBrowser = await setUpMyShopBrowser();
        const response = await myShopBrowser.page.goto(getMyShopUiBaseUrl());

        expect(response?.status()).toBe(200);

        const contentType = response?.headers()['content-type'];
        expect(contentType).toBeDefined();
        expect(contentType).toContain('text/html');

        const pageContent = await myShopBrowser.page.content();
        expect(pageContent).toContain('<html');
        expect(pageContent).toContain('</html>');
    } finally {
        await tearDownMyShopBrowser(myShopBrowser);
    }
});
