using Common;
using Driver.Port.Shop.Dtos;
using SystemTests.Commons.Constants;
using SystemTests.Legacy.Mod05.E2eTests.Base;
using Shouldly;
using Xunit;

namespace SystemTests.Legacy.Mod05.E2eTests;

public abstract class PlaceOrderNegativeBaseTest : BaseE2eTest
{
    [Theory]
    [InlineData("3.5")]
    [InlineData("lala")]
    public async Task ShouldRejectOrderWithNonIntegerQuantity(string nonIntegerQuantity)
    {
        var request = new PlaceOrderRequest
        {
            Sku = CreateUniqueSku(Defaults.SKU),
            Quantity = nonIntegerQuantity,
            Country = Defaults.COUNTRY
        };

        var result = await _shopDriver!.PlaceOrderAsync(request);

        result.ShouldBeFailure();
        var error = result.Error;
        error.Message.ShouldBe("The request contains one or more validation errors");
        error.Fields.ShouldNotBeNull();
        error.Fields!.ShouldContain(f => f.Field == "quantity" && f.Message == "Quantity must be an integer");
    }
}
