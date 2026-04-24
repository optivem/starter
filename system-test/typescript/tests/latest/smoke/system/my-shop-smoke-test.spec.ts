import { test, forChannels, ChannelType } from '../fixtures.js';

forChannels(ChannelType.UI, ChannelType.API)(() => {
    test('shouldBeAbleToGoToMyShop', async ({ scenario }) => {
        await scenario.assume().myShop().shouldBeRunning();
    });
});
