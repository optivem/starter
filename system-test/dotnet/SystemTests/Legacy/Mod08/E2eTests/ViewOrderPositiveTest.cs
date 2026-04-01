using Dsl.Core.Shop;
using Driver.Port.Shop.Dtos;
using SystemTests.Commons.Constants;
using SystemTests.Legacy.Mod08.E2eTests.Base;
using Optivem.Testing;
using Xunit;

namespace SystemTests.Legacy.Mod08.E2eTests;

public class ViewOrderPositiveTest : BaseE2eTest
{
    [Theory]
    [ChannelData(ChannelType.UI, ChannelType.API)]
    public async Task ShouldViewPlacedOrder(Channel channel)
    {
        await Scenario(channel)
            .Given().Product()
                .WithSku(Defaults.SKU)
                .WithUnitPrice("25.00")
            .And().Order()
                .WithOrderNumber(Defaults.ORDER_NUMBER)
                .WithSku(Defaults.SKU)
                .WithQuantity(4)
            .When().ViewOrder()
                .WithOrderNumber(Defaults.ORDER_NUMBER)
            .Then().ShouldSucceed()
            .And().Order(Defaults.ORDER_NUMBER)
                .HasSku(Defaults.SKU)
                .HasQuantity(4)
                .HasUnitPrice(25.00m)
                .HasTotalPrice("100.00")
                .HasStatus(OrderStatus.Placed);
    }
}













