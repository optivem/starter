using Common;
using Driver.Adapter.External.Erp.Client.Dtos;
using Driver.Adapter.Shop.Ui.Client.Pages;
using SystemTests.Commons.Constants;
using SystemTests.Legacy.Mod04.E2eTests.Base;
using SystemTests.Legacy.Mod06.E2eTests.Helpers;
using SystemTests.Commons.Providers;
using SystemTests.Legacy.Mod04.Base;
using Shouldly;
using Xunit;

namespace SystemTests.Legacy.Mod04.E2eTests;

public class PlaceOrderNegativeUiTest : BaseE2eTest
{
    protected override Task SetShopClientAsync()
    {
        return SetUpShopUiClientAsync();
    }

    [Fact]
    public async Task ShouldRejectOrderWithInvalidQuantity()
    {
        var homePage = await _shopUiClient!.OpenHomePageAsync();
        var newOrderPage = await homePage.ClickNewOrderAsync();
        await newOrderPage.InputSkuAsync(CreateUniqueSku(Defaults.SKU));
        await newOrderPage.InputQuantityAsync("invalid-quantity");

        await newOrderPage.ClickPlaceOrderAsync();
        var result = await newOrderPage.GetResultAsync();
        result.ShouldBeFailure();
        result.Error.ShouldHaveMessageAndField("The request contains one or more validation errors", "quantity", "Quantity must be an integer");
    }

    [Fact]
    public async Task ShouldRejectOrderWithNonExistentSku()
    {
        var homePage = await _shopUiClient!.OpenHomePageAsync();
        var newOrderPage = await homePage.ClickNewOrderAsync();
        await newOrderPage.InputSkuAsync("NON-EXISTENT-SKU-12345");
        await newOrderPage.InputQuantityAsync(Defaults.QUANTITY);

        await newOrderPage.ClickPlaceOrderAsync();
        var result = await newOrderPage.GetResultAsync();
        result.ShouldBeFailure();
        result.Error.ShouldHaveMessageAndField("The request contains one or more validation errors", "sku", "Product does not exist for SKU: NON-EXISTENT-SKU-12345");
    }

    [Fact]
    public async Task ShouldRejectOrderWithNegativeQuantity()
    {
        var homePage = await _shopUiClient!.OpenHomePageAsync();
        var newOrderPage = await homePage.ClickNewOrderAsync();
        await newOrderPage.InputSkuAsync(CreateUniqueSku(Defaults.SKU));
        await newOrderPage.InputQuantityAsync("-10");

        await newOrderPage.ClickPlaceOrderAsync();
        var result = await newOrderPage.GetResultAsync();
        result.ShouldBeFailure();
        result.Error.ShouldHaveMessageAndField("The request contains one or more validation errors", "quantity", "Quantity must be positive");
    }

    [Fact]
    public async Task ShouldRejectOrderWithZeroQuantity()
    {
        var homePage = await _shopUiClient!.OpenHomePageAsync();
        var newOrderPage = await homePage.ClickNewOrderAsync();
        await newOrderPage.InputSkuAsync("ANOTHER-SKU-67890");
        await newOrderPage.InputQuantityAsync("0");

        await newOrderPage.ClickPlaceOrderAsync();
        var result = await newOrderPage.GetResultAsync();
        result.ShouldBeFailure();
        result.Error.ShouldHaveMessageAndField("The request contains one or more validation errors", "quantity", "Quantity must be positive");
    }

    [Theory]
    [ClassData(typeof(EmptyArgumentsProvider))]
    public async Task ShouldRejectOrderWithEmptySku(string sku)
    {
        var homePage = await _shopUiClient!.OpenHomePageAsync();
        var newOrderPage = await homePage.ClickNewOrderAsync();
        await newOrderPage.InputSkuAsync(sku);
        await newOrderPage.InputQuantityAsync(Defaults.QUANTITY);

        await newOrderPage.ClickPlaceOrderAsync();
        var result = await newOrderPage.GetResultAsync();
        result.ShouldBeFailure();
        result.Error.ShouldHaveMessageAndField("The request contains one or more validation errors", "sku", "SKU must not be empty");
    }

    [Theory]
    [ClassData(typeof(EmptyArgumentsProvider))]
    public async Task ShouldRejectOrderWithEmptyQuantity(string emptyQuantity)
    {
        var homePage = await _shopUiClient!.OpenHomePageAsync();
        var newOrderPage = await homePage.ClickNewOrderAsync();
        await newOrderPage.InputSkuAsync(CreateUniqueSku(Defaults.SKU));
        await newOrderPage.InputQuantityAsync(emptyQuantity);

        await newOrderPage.ClickPlaceOrderAsync();
        var result = await newOrderPage.GetResultAsync();
        result.ShouldBeFailure();
        result.Error.ShouldHaveMessageAndField("The request contains one or more validation errors", "quantity", "Quantity must not be empty");
    }

    [Theory]
    [InlineData("3.5")]
    [InlineData("lala")]
    public async Task ShouldRejectOrderWithNonIntegerQuantity(string nonIntegerQuantity)
    {
        var homePage = await _shopUiClient!.OpenHomePageAsync();
        var newOrderPage = await homePage.ClickNewOrderAsync();
        await newOrderPage.InputSkuAsync(CreateUniqueSku(Defaults.SKU));
        await newOrderPage.InputQuantityAsync(nonIntegerQuantity);

        await newOrderPage.ClickPlaceOrderAsync();
        var result = await newOrderPage.GetResultAsync();
        result.ShouldBeFailure();
        result.Error.ShouldHaveMessageAndField("The request contains one or more validation errors", "quantity", "Quantity must be an integer");
    }

}













