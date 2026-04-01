using System.Text;
using SystemTests.Commons.Constants;
using SystemTests.Legacy.Mod03.E2eTests.Base;
using SystemTests.Commons.Providers;
using Shouldly;
using Xunit;

namespace SystemTests.Legacy.Mod03.E2eTests;

public class PlaceOrderNegativeUiTest : BaseE2eTest
{
    protected override async Task SetShopRawAsync()
    {
        await SetUpShopBrowserAsync();
    }

    [Fact]
    public async Task ShouldRejectOrderWithInvalidQuantity()
    {
        await NavigateToNewOrderAndSubmitAsync(CreateUniqueSku(Defaults.SKU), "invalid-quantity");
        await AssertErrorAlertContainsAsync("The request contains one or more validation errors", "quantity", "Quantity must be an integer");
    }

    [Fact]
    public async Task ShouldRejectOrderWithNonExistentSku()
    {
        await NavigateToNewOrderAndSubmitAsync("NON-EXISTENT-SKU-12345", Defaults.QUANTITY);
        await AssertErrorAlertContainsAsync("The request contains one or more validation errors", "sku", "Product does not exist for SKU: NON-EXISTENT-SKU-12345");
    }

    [Fact]
    public async Task ShouldRejectOrderWithNegativeQuantity()
    {
        await NavigateToNewOrderAndSubmitAsync(CreateUniqueSku(Defaults.SKU), "-10");
        await AssertErrorAlertContainsAsync("The request contains one or more validation errors", "quantity", "Quantity must be positive");
    }

    [Fact]
    public async Task ShouldRejectOrderWithZeroQuantity()
    {
        await NavigateToNewOrderAndSubmitAsync(CreateUniqueSku(Defaults.SKU), "0");
        await AssertErrorAlertContainsAsync("The request contains one or more validation errors", "quantity", "Quantity must be positive");
    }

    [Theory]
    [ClassData(typeof(EmptyArgumentsProvider))]
    public async Task ShouldRejectOrderWithEmptySku(string sku)
    {
        await NavigateToNewOrderAndSubmitAsync(sku, Defaults.QUANTITY);
        await AssertErrorAlertContainsAsync("The request contains one or more validation errors", "sku", "SKU must not be empty");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  ")]
    public async Task ShouldRejectOrderWithEmptyQuantity(string emptyQuantity)
    {
        await NavigateToNewOrderAndSubmitAsync(CreateUniqueSku(Defaults.SKU), emptyQuantity);
        await AssertErrorAlertContainsAsync("The request contains one or more validation errors", "quantity", "Quantity must not be empty");
    }

    [Theory]
    [InlineData("3.5")]
    [InlineData("lala")]
    public async Task ShouldRejectOrderWithNonIntegerQuantity(string nonIntegerQuantity)
    {
        await NavigateToNewOrderAndSubmitAsync(CreateUniqueSku(Defaults.SKU), nonIntegerQuantity);
        await AssertErrorAlertContainsAsync("The request contains one or more validation errors", "quantity", "Quantity must be an integer");
    }

    private async Task NavigateToNewOrderAndSubmitAsync(string sku, string quantity)
    {
        await shopUiPage!.GotoAsync(_configuration.ShopUiBaseUrl);
        await shopUiPage.Locator("a[href='/shop']").ClickAsync();
        await shopUiPage.Locator("[aria-label=\"SKU\"]").FillAsync(sku);
        await shopUiPage.Locator("[aria-label=\"Quantity\"]").FillAsync(quantity);
        await shopUiPage.Locator("[aria-label=\"Place Order\"]").ClickAsync();
    }

    private async Task AssertErrorAlertContainsAsync(params string[] expected)
    {
        var errorAlert = shopUiPage!.Locator("[role='alert']");
        await errorAlert.WaitForAsync(new() { State = Microsoft.Playwright.WaitForSelectorState.Visible });
        (await errorAlert.IsVisibleAsync()).ShouldBeTrue();
        var errorText = await errorAlert.TextContentAsync();
        foreach (var text in expected)
            (errorText ?? "").ShouldContain(text);
    }

    private async Task CreateProductViaErpAsync(string sku, string price)
    {
        var json = $$"""{"id":"{{sku}}","title":"Test Product","description":"Test Description","category":"Test Category","brand":"Test Brand","price":"{{price}}"}""";
        var uri = new Uri(_configuration.ErpBaseUrl + "/api/products");
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _erpHttpClient!.PostAsync(uri, content);
        ((int)response.StatusCode).ShouldBe(201);
    }
}











