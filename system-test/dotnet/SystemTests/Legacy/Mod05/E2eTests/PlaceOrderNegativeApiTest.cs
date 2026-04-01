using Common;
using Driver.Port.Shop.Dtos;
using SystemTests.Commons.Constants;
using SystemTests.Legacy.Mod06.E2eTests.Helpers;
using SystemTests.Legacy.Mod05.Base;
using Shouldly;
using Xunit;

namespace SystemTests.Legacy.Mod05.E2eTests;

public class PlaceOrderNegativeApiTest : PlaceOrderNegativeBaseTest
{
    protected override Task SetShopDriverAsync()
    {
        SetUpShopApiDriver();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task ShouldRejectOrderWithNullQuantity()
    {
        var request = new PlaceOrderRequest { Sku = CreateUniqueSku(Defaults.SKU), Quantity = null };
        var result = await _shopDriver!.PlaceOrderAsync(request);
        result.ShouldBeFailure();
        result.Error.ShouldHaveMessageAndField("The request contains one or more validation errors", "quantity", "Quantity must not be empty");
    }

    [Fact]
    public async Task ShouldRejectOrderWithNullSku()
    {
        var request = new PlaceOrderRequest { Sku = null, Quantity = Defaults.QUANTITY };
        var result = await _shopDriver!.PlaceOrderAsync(request);
        result.ShouldBeFailure();
        result.Error.ShouldHaveMessageAndField("The request contains one or more validation errors", "sku", "SKU must not be empty");
    }

}














