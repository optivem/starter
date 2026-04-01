using Dsl.Core.Shop;
using Driver.Port.Shop.Dtos;
using SystemTests.Legacy.Mod08.E2eTests.Base;
using Optivem.Testing;
using Xunit;

namespace SystemTests.Legacy.Mod08.E2eTests;

public class PlaceOrderPositiveTest : BaseE2eTest
{
    [Theory]
    [ChannelData(ChannelType.UI, ChannelType.API)]
    public async Task ShouldPlaceOrderWithCorrectSubtotalPrice(Channel channel)
    {
        await Scenario(channel)
            .Given().Product()
                .WithUnitPrice("20.00")
            .When().PlaceOrder()
                .WithQuantity(5)
            .Then().ShouldSucceed()
            .And().Order()
                    .HasTotalPrice("100.00");
    }

    [Theory]
    [ChannelData(ChannelType.UI, ChannelType.API)]
    [ChannelInlineData("20.00", "5", "100.00")]
    [ChannelInlineData("10.00", "3", "30.00")]
    [ChannelInlineData("15.50", "4", "62.00")]
    [ChannelInlineData("99.99", "1", "99.99")]
    public async Task ShouldPlaceOrderWithCorrectSubtotalPriceParameterized(Channel channel, string unitPrice, string quantity, string subtotalPrice)
    {
        await Scenario(channel)
            .Given().Product()
                .WithUnitPrice(unitPrice)
            .When().PlaceOrder()
                .WithQuantity(quantity)
            .Then().ShouldSucceed()
            .And().Order()
                    .HasTotalPrice(subtotalPrice);
    }

    [Theory]
    [ChannelData(ChannelType.UI, ChannelType.API)]
    public async Task ShouldPlaceOrder(Channel channel)
    {
        await Scenario(channel)
            .Given().Product()
                .WithUnitPrice("20.00")
            .When().PlaceOrder()
                .WithQuantity(5)
            .Then().ShouldSucceed()
            .And().Order()
                    .HasOrderNumberPrefix("ORD-")
                    .HasQuantity(5)
                    .HasUnitPrice(20.00m)
                    .HasTotalPrice("100.00")
                    .HasStatus(OrderStatus.Placed);
    }
}













