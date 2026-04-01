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

public class PlaceOrderPositiveUiTest : BaseE2eTest
{
    protected override Task SetShopClientAsync()
    {
        return SetUpShopUiClientAsync();
    }

    [Fact]
    public async Task ShouldPlaceOrderWithCorrectTotalPrice()
    {
        var sku = CreateUniqueSku(Defaults.SKU);
        (await _erpClient!.CreateProductAsync(new ExtCreateProductRequest { Id = sku, Title = "Test Product", Description = "Test Description", Category = "Test Category", Brand = "Test Brand", Price = "20.00" })).ShouldBeSuccess();

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
        (await orderDetailsPage.GetTotalPriceAsync()).ShouldBe(100.00m);
    }

    [Theory]
    [InlineData("20.00", "5", "100.00")]
    [InlineData("10.00", "3", "30.00")]
    [InlineData("15.50", "4", "62.00")]
    [InlineData("99.99", "1", "99.99")]
    public async Task ShouldPlaceOrderWithCorrectTotalPriceParameterized(string unitPrice, string quantity, string expectedTotalPrice)
    {
        var sku = CreateUniqueSku(Defaults.SKU);
        (await _erpClient!.CreateProductAsync(new ExtCreateProductRequest { Id = sku, Title = "Test Product", Description = "Test Description", Category = "Test Category", Brand = "Test Brand", Price = unitPrice })).ShouldBeSuccess();

        var homePage = await _shopUiClient!.OpenHomePageAsync();
        var newOrderPage = await homePage.ClickNewOrderAsync();
        await newOrderPage.InputSkuAsync(sku);
        await newOrderPage.InputQuantityAsync(quantity);

        await newOrderPage.ClickPlaceOrderAsync();

        var placeOrderResult = await newOrderPage.GetResultAsync();
        placeOrderResult.ShouldBeSuccess();

        var orderNumber = NewOrderPage.GetOrderNumber(placeOrderResult.Value!);

        var orderHistoryPage = await (await _shopUiClient.OpenHomePageAsync()).ClickOrderHistoryAsync();
        await orderHistoryPage.InputOrderNumberAsync(orderNumber);
        await orderHistoryPage.ClickSearchAsync();
        (await orderHistoryPage.WaitForOrderRowAsync(orderNumber)).ShouldBeTrue();

        var orderDetailsPage = await orderHistoryPage.ClickViewOrderDetailsAsync(orderNumber);
        (await orderDetailsPage.GetTotalPriceAsync()).ShouldBe(decimal.Parse(expectedTotalPrice));
    }

    [Fact]
    public async Task ShouldPlaceOrder()
    {
        var sku = CreateUniqueSku(Defaults.SKU);
        (await _erpClient!.CreateProductAsync(new ExtCreateProductRequest { Id = sku, Title = "Test Product", Description = "Test Description", Category = "Test Category", Brand = "Test Brand", Price = "20.00" })).ShouldBeSuccess();

        var homePage = await _shopUiClient!.OpenHomePageAsync();
        var newOrderPage = await homePage.ClickNewOrderAsync();
        await newOrderPage.InputSkuAsync(sku);
        await newOrderPage.InputQuantityAsync("5");

        await newOrderPage.ClickPlaceOrderAsync();

        var placeOrderResult = await newOrderPage.GetResultAsync();
        placeOrderResult.ShouldBeSuccess();

        var orderNumber = NewOrderPage.GetOrderNumber(placeOrderResult.Value!);
        orderNumber.ShouldStartWith("ORD-");

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















