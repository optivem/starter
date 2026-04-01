using SystemTests.Legacy.Mod05.Base;

namespace SystemTests.Legacy.Mod05.E2eTests;

public class PlaceOrderPositiveUiTest : PlaceOrderPositiveBaseTest
{
    protected override Task SetShopDriverAsync()
    {
        return SetUpShopUiDriverAsync();
    }
}











