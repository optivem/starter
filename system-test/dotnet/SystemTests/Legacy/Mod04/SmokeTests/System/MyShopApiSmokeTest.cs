using Common;
using SystemTests.Legacy.Mod04.Base;
using Xunit;

namespace SystemTests.Legacy.Mod04.SmokeTests.System;

public class MyShopApiSmokeTest : BaseClientTest
{
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        SetUpMyShopApiClient();
    }

    [Fact]
    public async Task ShouldBeAbleToGoToMyShop()
    {
        var result = await _shopApiClient!.Health().CheckHealthAsync();
        result.ShouldBeSuccess();
    }
}










