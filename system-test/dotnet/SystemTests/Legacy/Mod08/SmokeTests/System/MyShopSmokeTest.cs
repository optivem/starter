using SystemTests.Legacy.Mod08.Base;
using Optivem.Testing;
using Xunit;

namespace SystemTests.Legacy.Mod08.SmokeTests.System;

public class MyShopSmokeTest : BaseScenarioDslTest
{
    [Theory]
    [ChannelData(ChannelType.UI, ChannelType.API)]
    public async Task ShouldBeAbleToGoToMyShop(Channel channel)
    {
        await Scenario(channel)
            .Assume().MyShop().ShouldBeRunning();
    }
}
