using SystemTests.Latest.Base;
using Optivem.Testing;
using Xunit;

namespace SystemTests.Latest.SmokeTests.System;

public class ShopSmokeTest : BaseScenarioDslTest
{
    [Theory]
    [ChannelData(ChannelType.UI, ChannelType.API)]
    public async Task ShouldBeAbleToGoToShop(Channel channel)
    {
        await Scenario(channel).Assume().Shop().ShouldBeRunning();
    }
}











