using SystemTests.Latest.AcceptanceTests.Base;
using Dsl.Core.Shop;
using Optivem.Testing;

namespace SystemTests.Latest.AcceptanceTests;

public class BrowseCouponsPositiveTest : BaseAcceptanceTest
{
    [Theory]
    [ChannelData(ChannelType.UI, ChannelType.API)]
    public async Task ShouldBeAbleToBrowseCoupons(Channel channel)
    {
        await Scenario(channel)
            .When().BrowseCoupons()
            .Then().ShouldSucceed();
    }

    [Theory]
    [ChannelData(ChannelType.API)]
    public async Task ShouldReturnPublishedCoupon(Channel channel)
    {
        await Scenario(channel)
            .Given().Coupon().WithCouponCode("BROWSE10").WithDiscountRate(0.1m)
            .When().BrowseCoupons()
            .Then().ShouldSucceed()
            .And().Coupons().ContainsCouponWithCode("BROWSE10");
    }
}
