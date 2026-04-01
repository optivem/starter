using Common;
using SystemTests.Legacy.Mod06.Base;
using Dsl.Core.Shop;
using Optivem.Testing;
using Shouldly;
using Xunit;

namespace SystemTests.Legacy.Mod06.SmokeTests.System;

public class ShopSmokeTest : BaseChannelDriverTest
{
    [Theory]
    [ChannelData(ChannelType.UI, ChannelType.API)]
    public async Task ShouldBeAbleToGoToShop(Channel channel)
    {
        await SetChannelAsync(channel);

        var result = await _shopDriver!.GoToShopAsync();
        result.ShouldBeSuccess();
    }
}











