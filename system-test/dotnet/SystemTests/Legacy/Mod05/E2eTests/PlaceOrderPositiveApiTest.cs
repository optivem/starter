using SystemTests.Legacy.Mod05.Base;

namespace SystemTests.Legacy.Mod05.E2eTests;

public class PlaceOrderPositiveApiTest : PlaceOrderPositiveBaseTest
{
    protected override Task SetShopDriverAsync()
    {
        SetUpShopApiDriver();
        return Task.CompletedTask;
    }
}











