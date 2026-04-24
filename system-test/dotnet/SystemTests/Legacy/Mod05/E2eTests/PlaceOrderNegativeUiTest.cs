using SystemTests.Legacy.Mod05.Base;

namespace SystemTests.Legacy.Mod05.E2eTests;

public class PlaceOrderNegativeUiTest : PlaceOrderNegativeBaseTest
{
    protected override Task SetMyShopDriverAsync()
    {
        return SetUpMyShopUiDriverAsync();
    }
}











