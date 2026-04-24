using Dsl.Core.MyShop;
using SystemTests.Commons.Constants;
using SystemTests.Legacy.Mod07.E2eTests.Base;
using Optivem.Testing;
using Xunit;

namespace SystemTests.Legacy.Mod07.E2eTests;

public class PlaceOrderNegativeTest : BaseE2eTest
{
    [Theory]
    [ChannelData(ChannelType.UI, ChannelType.API)]
    public async Task ShouldRejectOrderWithNonIntegerQuantity(Channel channel)
    {
        var shop = await _app.MyShop(channel);
        (await shop.PlaceOrder()
            .Sku(Defaults.SKU)
            .Quantity("3.5")
            .Execute())
            .ShouldFail()
            .ErrorMessage("The request contains one or more validation errors")
            .FieldErrorMessage("quantity", "Quantity must be an integer");
    }
}
