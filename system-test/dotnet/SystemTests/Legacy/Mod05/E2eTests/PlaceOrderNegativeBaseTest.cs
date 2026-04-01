using Common;
using Driver.Port.Shop.Dtos;
using SystemTests.Commons.Constants;
using SystemTests.Legacy.Mod05.E2eTests.Base;
using Shouldly;
using Xunit;

namespace SystemTests.Legacy.Mod05.E2eTests;

public abstract class PlaceOrderNegativeBaseTest : BaseE2eTest
{
    [Fact]
    public async Task ShouldRejectOrderWithNonIntegerQuantity()
    {
        var request = new PlaceOrderRequest
        {
            Sku = CreateUniqueSku(Defaults.SKU),
            Quantity = "3.5",
        };

        var result = await _shopDriver!.PlaceOrderAsync(request);

        result.ShouldBeFailure();
        var error = result.Error;
        error.Message.ShouldBe("The request contains one or more validation errors");
        error.Fields.ShouldContain(f => f.Field == "quantity" && f.Message == "Quantity must be an integer");
    }
}
