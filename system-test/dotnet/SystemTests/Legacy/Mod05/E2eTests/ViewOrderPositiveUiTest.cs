using SystemTests.Legacy.Mod05.Base;

namespace SystemTests.Legacy.Mod05.E2eTests;

public class ViewOrderPositiveUiTest : ViewOrderPositiveBaseTest
{
    protected override Task SetShopDriverAsync()
    {
        return SetUpShopUiDriverAsync();
    }
}











