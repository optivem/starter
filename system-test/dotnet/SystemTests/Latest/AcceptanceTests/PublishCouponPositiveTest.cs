using SystemTests.Latest.AcceptanceTests.Base;
using Dsl.Core.Shop;
using Dsl.Port.Then.Steps;
using Optivem.Testing;

namespace SystemTests.Latest.AcceptanceTests;

public class PublishCouponPositiveTest : BaseAcceptanceTest
{
    [Theory]
    [ChannelData(ChannelType.UI, ChannelType.API)]
    public async Task ShouldBeAbleToPublishValidCoupon(Channel channel)
    {
        await Scenario(channel)
            .When().PublishCoupon()
                .WithCouponCode("SUMMER2025")
                .WithDiscountRate(0.15m)
                .WithValidFrom("2024-06-01T00:00:00Z")
                .WithValidTo("2024-08-31T23:59:59Z")
                .WithUsageLimit(100)
            .Then().ShouldSucceed();
    }

    [Theory]
    [ChannelData(ChannelType.UI, ChannelType.API)]
    public async Task ShouldBeAbleToPublishCouponWithEmptyOptionalFields(Channel channel)
    {
        await Scenario(channel)
            .When().PublishCoupon()
                .WithCouponCode("SUMMER2025")
                .WithDiscountRate(0.15m)
                .WithValidFrom(null)
                .WithValidTo(null)
                .WithUsageLimit(null)
            .Then().ShouldSucceed();
    }

    [Theory]
    [ChannelData(ChannelType.UI, ChannelType.API)]
    public async Task ShouldBeAbleToCorrectlySaveCoupon(Channel channel)
    {
        await Scenario(channel)
            .When().PublishCoupon()
                .WithCouponCode("SUMMER2025")
                .WithDiscountRate(0.15m)
                .WithValidFrom("2024-06-01T00:00:00Z")
                .WithValidTo("2024-08-31T23:59:00Z")
                .WithUsageLimit(100)
            .Then().ShouldSucceed()
            .And().Coupon("SUMMER2025")
            .HasDiscountRate(0.15m)
            .IsValidFrom("2024-06-01T00:00:00Z")
            .IsValidTo("2024-08-31T23:59:00Z")
            .HasUsageLimit(100)
            .HasUsedCount(0);
    }
}
