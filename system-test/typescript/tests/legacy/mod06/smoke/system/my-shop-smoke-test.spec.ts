import { test, expect, forChannels, ChannelType } from '../fixtures.js';

forChannels(ChannelType.UI, ChannelType.API)(() => {
    test('shouldBeAbleToGoToMyShop', async ({ myShopDriver }) => {
        const result = await myShopDriver.goToMyShop();
        expect(result.success).toBe(true);
    });
});
