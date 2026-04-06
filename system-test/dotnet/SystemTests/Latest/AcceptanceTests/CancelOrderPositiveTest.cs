using SystemTests.Latest.AcceptanceTests.Base;
using Dsl.Core.Shop;
using Driver.Port.Shop.Dtos;
using Optivem.Testing;

namespace SystemTests.Latest.AcceptanceTests;

public class CancelOrderPositiveTest : BaseAcceptanceTest
{
    [Theory]
    [ChannelData(ChannelType.UI, ChannelType.API)]
    public async Task ShouldHaveCancelledStatusWhenCancelled(Channel channel)
    {
        await Scenario(channel)
            .Given().Order()
            .When().CancelOrder()
            .Then().ShouldSucceed()
            .And().Order()
            .HasStatus(OrderStatus.Cancelled);
    }
}
