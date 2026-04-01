using Common;
using Driver.Port.External.Erp.Dtos;
using Driver.Port.Shop.Dtos;
using SystemTests.Commons.Constants;
using SystemTests.Legacy.Mod05.E2eTests.Base;
using Shouldly;
using Xunit;

namespace SystemTests.Legacy.Mod05.E2eTests;

public abstract class PlaceOrderPositiveBaseTest : BaseE2eTest
{
    [Fact]
    public async Task ShouldPlaceOrderForValidInput()
    {
        // GivenStage
        var sku = CreateUniqueSku(Defaults.SKU);
        (await _erpDriver!.ReturnsProductAsync(new ReturnsProductRequest { Sku = sku, Price = "20.00" })).ShouldBeSuccess();

        // WhenStage
        var placeOrderRequest = new PlaceOrderRequest { Sku = sku, Quantity = "5" };
        var placeOrderResult = await _shopDriver!.PlaceOrderAsync(placeOrderRequest);
        placeOrderResult.ShouldBeSuccess();

        var orderNumber = placeOrderResult.Value.OrderNumber;
        orderNumber.ShouldStartWith("ORD-");

        // ThenStage
        var viewOrderResult = await _shopDriver.ViewOrderAsync(orderNumber);
        viewOrderResult.ShouldBeSuccess();

        var order = viewOrderResult.Value!;
        order.OrderNumber.ShouldBe(orderNumber);
        order.Sku.ShouldBe(sku);
        order.Quantity.ShouldBe(5);
        order.UnitPrice.ShouldBe(20.00m);
        order.TotalPrice.ShouldBeGreaterThan(0);
        order.Status.ShouldBe(OrderStatus.Placed);
    }
}
