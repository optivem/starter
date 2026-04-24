using Common;
using SystemTests.Legacy.Mod05.Base;
using Shouldly;
using Xunit;

namespace SystemTests.Legacy.Mod05.SmokeTests.System;

public abstract class MyShopBaseSmokeTest : BaseDriverTest
{
    protected abstract Task SetUpMyShopDriverAsync();

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await SetUpMyShopDriverAsync();
    }

    [Fact]
    public async Task ShouldBeAbleToGoToMyShop()
    {
        var result = await _shopDriver!.GoToMyShopAsync();
        result.ShouldBeSuccess();
    }
}










