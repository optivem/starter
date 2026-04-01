using Driver.Adapter.Shop.Ui.Client.Pages;
using SystemTests.Legacy.Mod04.E2eTests.Base;
using SystemTests.Legacy.Mod04.Base;
using Shouldly;
using Xunit;

namespace SystemTests.Legacy.Mod04.E2eTests;

public class ViewOrderNegativeUiTest : BaseE2eTest
{
    protected override Task SetShopClientAsync()
    {
        return SetUpShopUiClientAsync();
    }

    [Fact]
    public async Task ShouldNotBeAbleToViewNonExistentOrder()
    {
        var orderNumber = "NON-EXISTENT-ORDER-99999";
        var homePage = await _shopUiClient!.OpenHomePageAsync();
        var orderHistoryPage = await homePage.ClickOrderHistoryAsync();
        await orderHistoryPage.InputOrderNumberAsync(orderNumber);
        await orderHistoryPage.ClickSearchAsync();
        (await orderHistoryPage.WaitForOrderRowAsync(orderNumber, 3000)).ShouldBeFalse();
    }
}













