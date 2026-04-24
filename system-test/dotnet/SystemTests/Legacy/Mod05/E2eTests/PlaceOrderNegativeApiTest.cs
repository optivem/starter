namespace SystemTests.Legacy.Mod05.E2eTests;

public class PlaceOrderNegativeApiTest : PlaceOrderNegativeBaseTest
{
    protected override Task SetMyShopDriverAsync()
    {
        SetUpMyShopApiDriver();
        return Task.CompletedTask;
    }
}
