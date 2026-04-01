using SystemTests.Latest.AcceptanceTests.Base;
using Dsl.Core.Shop;
using Driver.Port.Shop.Dtos;
using Dsl.Port.Then.Steps;
using Optivem.Testing;

namespace SystemTests.Latest.AcceptanceTests;

public class PlaceOrderPositiveTest : BaseAcceptanceTest
{
    [Theory]
    [ChannelData(ChannelType.UI, ChannelType.API)]
    public async Task ShouldBeAbleToPlaceOrderForValidInput(Channel channel)
    {
        await Scenario(channel)
            .Given().Product().WithSku("ABC").WithUnitPrice(20.00m)
            .When().PlaceOrder().WithSku("ABC").WithQuantity(5)
            .Then().ShouldSucceed();
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

    [Theory]
    [ChannelData(ChannelType.UI, ChannelType.API)]
    public async Task OrderPrefixShouldBeORD(Channel channel)
    {
        await Scenario(channel)
            .When().PlaceOrder()
            .Then().ShouldSucceed()
            .And().Order()
            .HasOrderNumberPrefix("ORD-");
    }

    [Theory]
    [ChannelData(ChannelType.UI, ChannelType.API)]
    public async Task ShouldCalculateTotalPriceAsProductOfUnitPriceAndQuantity(Channel channel)
    {
        await Scenario(channel)
            .Given().Product().WithUnitPrice(20.00m)
            .When().PlaceOrder().WithQuantity(5)
            .Then().ShouldSucceed()
            .And().Order()
            .HasTotalPrice(100.00m);
    }

    [Theory]
    [ChannelData(ChannelType.UI, ChannelType.API)]
    [ChannelInlineData("20.00", 5, "100.00")]
    [ChannelInlineData("10.00", 3, "30.00")]
    [ChannelInlineData("15.50", 4, "62.00")]
    [ChannelInlineData("99.99", 1, "99.99")]
    public async Task ShouldPlaceOrderWithCorrectTotalPriceParameterized(Channel channel, string unitPrice, int quantity, string totalPrice)
    {
        await Scenario(channel)
            .Given().Product().WithUnitPrice(unitPrice)
            .When().PlaceOrder().WithQuantity(quantity)
            .Then().ShouldSucceed()
            .And().Order()
            .HasTotalPrice(totalPrice);
    }
}
