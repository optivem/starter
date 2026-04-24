namespace SystemTests.Legacy.Mod05.SmokeTests.System;

public class MyShopUiSmokeTest : MyShopBaseSmokeTest
{
    protected override async Task SetUpMyShopDriverAsync()
    {
        await SetUpMyShopUiDriverAsync();
    }
}









