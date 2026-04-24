namespace SystemTests.Legacy.Mod05.SmokeTests.System;

public class MyShopApiSmokeTest : MyShopBaseSmokeTest
{
    protected override Task SetUpMyShopDriverAsync()
    {
        SetUpMyShopApiDriver();
        return Task.CompletedTask;
    }
}









