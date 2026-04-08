using Common;
using Dsl.Core.Shop;
using Driver.Port.Shop.Dtos;
using SystemTests.Commons.Constants;
using SystemTests.Legacy.Mod06.E2eTests.Base;
using Optivem.Testing;
using Shouldly;
using Xunit;

namespace SystemTests.Legacy.Mod06.E2eTests;

public class PlaceOrderNegativeTest : BaseE2eTest
{
    [Theory]
    [ChannelData(ChannelType.UI, ChannelType.API)]
    public async Task ShouldRejectOrderWithNonIntegerQuantity(Channel channel)
    {
        await SetChannelAsync(channel);

        var request = new PlaceOrderRequest
        {
            Sku = CreateUniqueSku(Defaults.SKU),
            Quantity = "3.5",
            Country = Defaults.COUNTRY,
        };

        var result = await _shopDriver!.PlaceOrderAsync(request);

        result.ShouldBeFailure();
        var error = result.Error;
        error.Message.ShouldBe("The request contains one or more validation errors");
        error.Fields.ShouldContain(f => f.Field == "quantity" && f.Message == "Quantity must be an integer");
    }
}
