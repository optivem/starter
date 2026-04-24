using Common;
using SystemTests.Commons.Constants;
using SystemTests.Legacy.Mod04.E2eTests.Base;
using Shouldly;
using Xunit;

namespace SystemTests.Legacy.Mod04.E2eTests;

public class PlaceOrderNegativeUiTest : BaseE2eTest
{
    protected override Task SetMyShopClientAsync()
    {
        return SetUpMyShopUiClientAsync();
    }

    [Fact]
    public async Task ShouldRejectOrderWithNonIntegerQuantity()
    {
        var homePage = await _shopUiClient!.OpenHomePageAsync();
        var newOrderPage = await homePage.ClickNewOrderAsync();

        await newOrderPage.InputSkuAsync(CreateUniqueSku(Defaults.SKU));
        await newOrderPage.InputQuantityAsync("invalid-quantity");
        await newOrderPage.ClickPlaceOrderAsync();

        var result = await newOrderPage.GetResultAsync();

        result.ShouldBeFailure();
        var error = result.Error;
        error.Message.ShouldBe("The request contains one or more validation errors");
        error.Fields.ShouldContain(f => f.Field == "quantity" && f.Message == "Quantity must be an integer");
    }
}
