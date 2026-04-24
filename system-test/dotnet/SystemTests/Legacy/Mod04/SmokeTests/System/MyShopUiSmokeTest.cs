using SystemTests.Legacy.Mod04.Base;
using Shouldly;
using Xunit;

namespace SystemTests.Legacy.Mod04.SmokeTests.System;

public class MyShopUiSmokeTest : BaseClientTest
{
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await SetUpMyShopUiClientAsync();
    }

    [Fact]
    public async Task ShouldBeAbleToGoToMyShop()
    {
        await _shopUiClient!.OpenHomePageAsync();
        (await _shopUiClient.IsPageLoadedAsync()).ShouldBeTrue();
    }
}










