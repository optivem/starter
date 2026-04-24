using SystemTests.Legacy.Mod10.AcceptanceTests.Base;
using Dsl.Core.MyShop;
using Driver.Port.MyShop.Dtos;
using Optivem.Testing;
using Xunit;

namespace SystemTests.Legacy.Mod10.AcceptanceTests;

public class PlaceOrderPositiveTest : BaseAcceptanceTest
{
    [Theory]
    [ChannelData(ChannelType.UI, ChannelType.API)]
    public async Task OrderNumberShouldStartWithORD(Channel channel)
    {
        await Scenario(channel)
            .When().PlaceOrder()
            .Then().ShouldSucceed()
            .And().Order()
                .HasOrderNumberPrefix("ORD-");
    }

    [Theory]
    [ChannelData(ChannelType.UI, ChannelType.API)]
    public async Task OrderStatusShouldBePlacedAfterPlacingOrder(Channel channel)
    {
        await Scenario(channel)
            .When().PlaceOrder()
            .Then().ShouldSucceed()
            .And().Order()
                .HasStatus(OrderStatus.Placed);
    }
}
