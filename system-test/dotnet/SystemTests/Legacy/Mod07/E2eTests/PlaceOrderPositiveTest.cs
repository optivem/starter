using Common;
using Dsl.Core.Shop;
using Driver.Port.Shop.Dtos;
using SystemTests.Commons.Constants;
using SystemTests.Legacy.Mod07.E2eTests.Base;
using Optivem.Testing;
using Shouldly;
using Xunit;

namespace SystemTests.Legacy.Mod07.E2eTests;

public class PlaceOrderPositiveTest : BaseE2eTest
{
    [Theory]
    [ChannelData(ChannelType.UI, ChannelType.API)]
    public async Task ShouldPlaceOrderWithCorrectSubtotalPrice(Channel channel)
    {
        (await _app.Erp()
            .ReturnsProduct()
            .Sku(Defaults.SKU)
            .UnitPrice(20.00m)
            .Execute())
            .ShouldSucceed();

        var shop = await _app.Shop(channel);
        (await shop.PlaceOrder()
            .OrderNumber(Defaults.ORDER_NUMBER)
            .Sku(Defaults.SKU)
            .Quantity(5)
            .Execute())
            .ShouldSucceed();

        (await shop.ViewOrder()
            .OrderNumber(Defaults.ORDER_NUMBER)
            .Execute())
            .ShouldSucceed()
            .TotalPrice(100.00m);
    }

    [Theory]
    [ChannelData(ChannelType.UI, ChannelType.API)]
    [ChannelInlineData("20.00", "5", "100.00")]
    [ChannelInlineData("10.00", "3", "30.00")]
    [ChannelInlineData("15.50", "4", "62.00")]
    [ChannelInlineData("99.99", "1", "99.99")]
    public async Task ShouldPlaceOrderWithCorrectSubtotalPriceParameterized(Channel channel, string unitPrice, string quantity, string subtotalPrice)
    {
        (await _app.Erp()
            .ReturnsProduct()
            .Sku(Defaults.SKU)
            .UnitPrice(unitPrice)
            .Execute())
            .ShouldSucceed();

        var shop = await _app.Shop(channel);
        (await shop.PlaceOrder()
            .OrderNumber(Defaults.ORDER_NUMBER)
            .Sku(Defaults.SKU)
            .Quantity(quantity)
            .Execute())
            .ShouldSucceed();

        (await shop.ViewOrder()
            .OrderNumber(Defaults.ORDER_NUMBER)
            .Execute())
            .ShouldSucceed()
            .TotalPrice(subtotalPrice);
    }

    [Theory]
    [ChannelData(ChannelType.UI, ChannelType.API)]
    public async Task ShouldPlaceOrder(Channel channel)
    {
        (await _app.Erp()
            .ReturnsProduct()
            .Sku(Defaults.SKU)
            .UnitPrice(20.00m)
            .Execute())
            .ShouldSucceed();

        var shop = await _app.Shop(channel);
        (await shop.PlaceOrder()
            .OrderNumber(Defaults.ORDER_NUMBER)
            .Sku(Defaults.SKU)
            .Quantity(5)
            .Execute())
            .ShouldSucceed()
            .OrderNumber(Defaults.ORDER_NUMBER)
            .OrderNumberStartsWith("ORD-");

        (await shop.ViewOrder()
            .OrderNumber(Defaults.ORDER_NUMBER)
            .Execute())
            .ShouldSucceed()
            .OrderNumber(Defaults.ORDER_NUMBER)
            .Sku(Defaults.SKU)
            .Quantity(5)
            .UnitPrice(20.00m)
            .TotalPrice(100.00m)
            .Status(OrderStatus.Placed);
    }
}













