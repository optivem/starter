using Common;
using SystemTests.Legacy.Mod04.Base;
using Xunit;

namespace SystemTests.Legacy.Mod04.SmokeTests.System;

public class ShopApiSmokeTest : BaseClientTest
{
    public override Task InitializeAsync()
    {
        SetUpShopApiClient();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task ShouldBeAbleToGoToShop()
    {
        var result = await _shopApiClient!.Health().CheckHealthAsync();
        result.ShouldBeSuccess();
    }
}










