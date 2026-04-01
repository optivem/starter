using Common;
using Driver.Port.External.Erp.Dtos;
using Driver.Port.Shop.Dtos;
using SystemTests.Commons.Constants;
using SystemTests.Legacy.Mod05.E2eTests.Base;
using Shouldly;
using Xunit;

namespace SystemTests.Legacy.Mod05.E2eTests;

public abstract class ViewOrderPositiveBaseTest : BaseE2eTest
{
    [Fact]
    public async Task ShouldViewPlacedOrder()
    {
        var sku = CreateUniqueSku(Defaults.SKU);
        (await _erpDriver!.ReturnsProductAsync(new ReturnsProductRequest { Sku = sku, Price = "25.00" })).ShouldBeSuccess();

        var placeOrderRequest = new PlaceOrderRequest { Sku = sku, Quantity = "4" };
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













