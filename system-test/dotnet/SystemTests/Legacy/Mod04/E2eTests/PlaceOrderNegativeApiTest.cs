using Common;
using Driver.Port.MyShop.Dtos;
using SystemTests.Commons.Constants;
using SystemTests.Legacy.Mod04.E2eTests.Base;
using Shouldly;
using Xunit;

namespace SystemTests.Legacy.Mod04.E2eTests;

public class PlaceOrderNegativeApiTest : BaseE2eTest
{
    protected override Task SetMyShopClientAsync()
    {
        SetUpMyShopApiClient();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task ShouldRejectOrderWithNonIntegerQuantity()
    {
        var request = new PlaceOrderRequest { Sku = CreateUniqueSku(Defaults.SKU), Quantity = "invalid-quantity" };
        var result = await _shopApiClient!.Orders().PlaceOrderAsync(request);

        result.ShouldBeFailure();
        result.Error.Detail.ShouldBe("The request contains one or more validation errors");
        result.Error.Errors.ShouldNotBeNull();
        result.Error.Errors.ShouldContain(e => e.Field == "quantity" && e.Message == "Quantity must be an integer");
    }
}
