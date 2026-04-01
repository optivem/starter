using Common;
using Driver.Adapter.External.Erp.Client.Dtos;
using Driver.Adapter.Shop.Ui.Client.Pages;
using Driver.Port.Shop.Dtos;
using SystemTests.Commons.Constants;
using SystemTests.Legacy.Mod04.E2eTests.Base;
using SystemTests.Legacy.Mod04.Base;
using Shouldly;
using Xunit;

namespace SystemTests.Legacy.Mod04.E2eTests;

public class ViewOrderPositiveUiTest : BaseE2eTest
{
    protected override Task SetShopClientAsync()
    {
        return SetUpShopUiClientAsync();
    }

    [Fact]
    public async Task ShouldViewPlacedOrder()
    {
        var sku = CreateUniqueSku(Defaults.SKU);
        (await _erpClient!.CreateProductAsync(new ExtCreateProductRequest { Id = sku, Title = "Test", Description = "Test", Category = "Test", Brand = "Test", Price = "20.00" })).ShouldBeSuccess();

        var homePage = await _shopUiClient!.OpenHomePageAsync();
        var newOrderPage = await homePage.ClickNewOrderAsync();
        await newOrderPage.InputSkuAsync(sku);
        await newOrderPage.InputQuantityAsync("5");
        await newOrderPage.ClickPlaceOrderAsync();

        var placeOrderResult = await newOrderPage.GetResultAsync();
        placeOrderResult.ShouldBeSuccess();

        var orderNumber = NewOrderPage.GetOrderNumber(placeOrderResult.Value!);

        var orderHistoryPage = await (await _shopUiClient.OpenHomePageAsync()).ClickOrderHistoryAsync();
        await orderHistoryPage.InputOrderNumberAsync(orderNumber);
        await orderHistoryPage.ClickSearchAsync();
        (await orderHistoryPage.WaitForOrderRowAsync(orderNumber)).ShouldBeTrue();

        var orderDetailsPage = await orderHistoryPage.ClickViewOrderDetailsAsync(orderNumber);
        (await orderDetailsPage.GetOrderNumberAsync()).ShouldBe(orderNumber);
        (await orderDetailsPage.GetSkuAsync()).ShouldBe(sku);
        (await orderDetailsPage.GetQuantityAsync()).ShouldBe(5);
        (await orderDetailsPage.GetUnitPriceAsync()).ShouldBe(20.00m);
        (await orderDetailsPage.GetTotalPriceAsync()).ShouldBe(100.00m);
        (await orderDetailsPage.GetStatusAsync()).ShouldBe(OrderStatus.Placed);
    }
}















