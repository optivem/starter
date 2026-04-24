import { test, forChannels, ChannelType } from '../../base/BaseScenarioDslTest.js';

forChannels(ChannelType.UI, ChannelType.API)(() => {
    test('shouldBeAbleToGoToMyShop', async ({ scenario }) => {
        await scenario.assume().myShop().shouldBeRunning();
    });
});
