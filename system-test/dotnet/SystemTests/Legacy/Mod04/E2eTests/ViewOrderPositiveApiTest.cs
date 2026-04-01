using Common;
using Driver.Adapter.External.Erp.Client.Dtos;
using Driver.Port.Shop.Dtos;
using SystemTests.Commons.Constants;
using SystemTests.Legacy.Mod04.E2eTests.Base;
using SystemTests.Legacy.Mod04.Base;
using Shouldly;
using Xunit;

namespace SystemTests.Legacy.Mod04.E2eTests;

public class ViewOrderPositiveApiTest : BaseE2eTest
{
    protected override Task SetShopClientAsync()
    {
        SetUpShopApiClient();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task ShouldViewPlacedOrder()
    {
        var sku = CreateUniqueSku(Defaults.SKU);
        (await _erpClient!.CreateProductAsync(new ExtCreateProductRequest { Id = sku, Title = "Test", Description = "Test", Category = "Test", Brand = "Test", Price = "25.00" })).ShouldBeSuccess();

        var placeOrderRequest = new PlaceOrderRequest { Sku = sku, Quantity = "4" };
        var placeOrderResult = await _shopApiClient!.Orders().PlaceOrderAsync(placeOrderRequest);
        placeOrderResult.ShouldBeSuccess();

        var orderNumber = placeOrderResult.Value!.OrderNumber;

        var viewOrderResult = await _shopApiClient.Orders().ViewOrderAsync(orderNumber);
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















