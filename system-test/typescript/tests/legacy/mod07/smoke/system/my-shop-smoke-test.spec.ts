import { test, forChannels, ChannelType } from '../fixtures.js';

forChannels(ChannelType.UI, ChannelType.API)(() => {
    test('shouldBeAbleToGoToMyShop', async ({ app }) => {
        (await app.myShop().goToMyShop().execute()).shouldSucceed();
    });
});
