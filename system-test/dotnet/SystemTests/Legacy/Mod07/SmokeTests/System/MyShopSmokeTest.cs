using Common;
using SystemTests.Legacy.Mod07.Base;
using Dsl.Core.MyShop;
using Optivem.Testing;
using Shouldly;
using Xunit;

namespace SystemTests.Legacy.Mod07.SmokeTests.System;

public class MyShopSmokeTest : BaseUseCaseDslTest
{
    [Theory]
    [ChannelData(ChannelType.UI, ChannelType.API)]
    public async Task ShouldBeAbleToGoToMyShop(Channel channel)
    {
        (await (await _app.MyShop(channel)).GoToMyShop()
            .Execute())
            .ShouldSucceed();
    }
}











