using Dsl.Core.Shop;
using SystemTests.Legacy.Mod08.E2eTests.Base;
using Optivem.Testing;
using Xunit;

namespace SystemTests.Legacy.Mod08.E2eTests;

public class PlaceOrderNegativeTest : BaseE2eTest
{
    [Theory]
    [ChannelData(ChannelType.UI, ChannelType.API)]
    public async Task ShouldRejectOrderWithNonIntegerQuantity(Channel channel)
    {
        await Scenario(channel)
            .When().PlaceOrder()
                .WithQuantity("3.5")
            .Then().ShouldFail()
                .ErrorMessage("The request contains one or more validation errors")
                .FieldErrorMessage("quantity", "Quantity must be an integer");
    }
}
