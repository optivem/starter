using Common;
using Dsl.Core.Shop;
using Driver.Port.Shop.Dtos;
using Driver.Port.Shop;
using Driver.Port.External.Erp.Dtos;
using SystemTests.Commons.Constants;
using SystemTests.Legacy.Mod06.E2eTests.Base;
using Optivem.Testing;
using Shouldly;
using Xunit;

namespace SystemTests.Legacy.Mod06.E2eTests;

public class PlaceOrderPositiveTest : BaseE2eTest
{
    [Theory]
    [ChannelData(ChannelType.UI, ChannelType.API)]
    public async Task ShouldPlaceOrderWithCorrectTotalPrice(Channel channel)
    {
        await SetChannelAsync(channel);

        var sku = CreateUniqueSku(Defaults.SKU);
        (await _erpDriver!.ReturnsProductAsync(new ReturnsProductRequest 
        { 
            Sku = sku, 
            Price = "20.00" 
        }))
            .ShouldBeSuccess();

        var placeOrderRequest = new PlaceOrderRequest 
        { 
            Sku = sku, 
            Quantity = "5", 

        };
        var placeOrderResult = await _shopDriver!.PlaceOrderAsync(placeOrderRequest);
        placeOrderResult.ShouldBeSuccess();

        var orderNumber = placeOrderResult.Value!.OrderNumber;
        var viewOrderResult = await _shopDriver.ViewOrderAsync(orderNumber);
        viewOrderResult.ShouldBeSuccess();
        viewOrderResult.Value!.TotalPrice.ShouldBe(100.00m);
    }

    [Theory]
    [ChannelData(ChannelType.UI, ChannelType.API)]
    [ChannelInlineData("20.00", "5", "100.00")]
    [ChannelInlineData("10.00", "3", "30.00")]
    [ChannelInlineData("15.50", "4", "62.00")]
    [ChannelInlineData("99.99", "1", "99.99")]
    public async Task ShouldPlaceOrderWithCorrectTotalPriceParameterized(Channel channel, string unitPrice, string quantity, string expectedTotalPrice)
    {
        await SetChannelAsync(channel);

        var sku = CreateUniqueSku(Defaults.SKU);
        (await _erpDriver!.ReturnsProductAsync(new ReturnsProductRequest 
        { 
            Sku = sku, 
            Price = unitPrice 
        }))
            .ShouldBeSuccess();

        var placeOrderRequest = new PlaceOrderRequest 
        { 
            Sku = sku, 
            Quantity = quantity, 

        };
        var placeOrderResult = await _shopDriver!.PlaceOrderAsync(placeOrderRequest);
        placeOrderResult.ShouldBeSuccess();

        var orderNumber = placeOrderResult.Value!.OrderNumber;
        var viewOrderResult = await _shopDriver.ViewOrderAsync(orderNumber);
        viewOrderResult.ShouldBeSuccess();
        viewOrderResult.Value!.TotalPrice.ShouldBe(decimal.Parse(expectedTotalPrice));
    }

    [Theory]
    [ChannelData(ChannelType.UI, ChannelType.API)]
    public async Task ShouldPlaceOrder(Channel channel)
    {
        await SetChannelAsync(channel);

        var sku = CreateUniqueSku(Defaults.SKU);
        (await _erpDriver!.ReturnsProductAsync(new ReturnsProductRequest 
        { 
            Sku = sku, 
            Price = "20.00" 
        }))
            .ShouldBeSuccess();

        var placeOrderRequest = new PlaceOrderRequest 
        { 
            Sku = sku, 
            Quantity = "5", 

        };
        var placeOrderResult = await _shopDriver!.PlaceOrderAsync(placeOrderRequest);
        placeOrderResult.ShouldBeSuccess();

        var orderNumber = placeOrderResult.Value!.OrderNumber;
        orderNumber.ShouldStartWith("ORD-");

        var viewOrderResult = await _shopDriver.ViewOrderAsync(orderNumber);
        viewOrderResult.ShouldBeSuccess();

        var order = viewOrderResult.Value!;
        order.OrderNumber.ShouldBe(orderNumber);
        order.Sku.ShouldBe(sku);
        order.Quantity.ShouldBe(5);
        order.UnitPrice.ShouldBe(20.00m);
        order.TotalPrice.ShouldBe(100.00m);
        order.Status.ShouldBe(OrderStatus.Placed);
    }
}














