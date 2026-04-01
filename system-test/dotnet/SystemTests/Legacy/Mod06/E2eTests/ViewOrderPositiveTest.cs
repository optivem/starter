using Common;
using Driver.Port.External.Erp.Dtos;
using Dsl.Core.Shop;
using Driver.Port.Shop.Dtos;
using SystemTests.Commons.Constants;
using SystemTests.Legacy.Mod06.E2eTests.Base;
using Optivem.Testing;
using Shouldly;
using Xunit;

namespace SystemTests.Legacy.Mod06.E2eTests;

public class ViewOrderPositiveTest : BaseE2eTest
{
    [Theory]
    [ChannelData(ChannelType.UI, ChannelType.API)]
    public async Task ShouldViewPlacedOrder(Channel channel)
    {
        await SetChannelAsync(channel);

        var sku = CreateUniqueSku(Defaults.SKU);
        (await _erpDriver!.ReturnsProductAsync(new ReturnsProductRequest 
        { 
            Sku = sku, 
            Price = "25.00" 
        })).ShouldBeSuccess();

        var placeOrderRequest = new PlaceOrderRequest
        {
            Sku = sku,
            Quantity = "4"
        };
        var placeOrderResult = await _shopDriver!.PlaceOrderAsync(placeOrderRequest);
        placeOrderResult.ShouldBeSuccess();

        var orderNumber = placeOrderResult.Value.OrderNumber;

        var viewOrderResult = await _shopDriver.ViewOrderAsync(orderNumber);
        viewOrderResult.ShouldBeSuccess();

        var order = viewOrderResult.Value!;
        order.OrderNumber.ShouldBe(orderNumber);
        order.Sku.ShouldBe(sku);
        order.Quantity.ShouldBe(4);
        order.UnitPrice.ShouldBe(25.00m);
        order.TotalPrice.ShouldBe(100.00m);
        order.Status.ShouldBe(OrderStatus.Placed);
    }
}














