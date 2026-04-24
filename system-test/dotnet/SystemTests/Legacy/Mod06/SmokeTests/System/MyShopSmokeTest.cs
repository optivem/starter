using Common;
using SystemTests.Legacy.Mod06.Base;
using Dsl.Core.MyShop;
using Optivem.Testing;
using Shouldly;
using Xunit;

namespace SystemTests.Legacy.Mod06.SmokeTests.System;

public class MyShopSmokeTest : BaseChannelDriverTest
{
    [Theory]
    [ChannelData(ChannelType.UI, ChannelType.API)]
    public async Task ShouldBeAbleToGoToMyShop(Channel channel)
    {
        await SetChannelAsync(channel);

        var result = await _shopDriver!.GoToMyShopAsync();
        result.ShouldBeSuccess();
    }
}











