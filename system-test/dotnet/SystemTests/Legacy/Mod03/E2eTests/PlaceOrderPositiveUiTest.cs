using System.Text;
using System.Text.RegularExpressions;
using SystemTests.Commons.Constants;
using SystemTests.Legacy.Mod03.E2eTests.Base;
using Shouldly;
using Xunit;

namespace SystemTests.Legacy.Mod03.E2eTests;

public class PlaceOrderPositiveUiTest : BaseE2eTest
{
    protected override async Task SetShopRawAsync()
    {
        await SetUpShopBrowserAsync();
    }

    [Fact]
    public async Task ShouldPlaceOrderWithCorrectTotalPrice()
    {
        var sku = CreateUniqueSku(Defaults.SKU);
        await CreateProductViaErpAsync(sku, "20.00");

        await shopUiPage!.GotoAsync(_configuration.ShopUiBaseUrl);
        await shopUiPage.Locator("a[href='/shop']").ClickAsync();
        await shopUiPage.Locator("[aria-label=\"SKU\"]").FillAsync(sku);
        await shopUiPage.Locator("[aria-label=\"Quantity\"]").FillAsync("5");

        await shopUiPage.Locator("[aria-label=\"Place Order\"]").ClickAsync();

        var successMessageText = await shopUiPage.Locator("[role='alert']").TextContentAsync();
        var match = Regex.Match(successMessageText ?? "", @"Success! Order has been created with Order Number ([\w-]+)");
        match.Success.ShouldBeTrue();
        var orderNumber = match.Groups[1].Value;

        await shopUiPage.GotoAsync(_configuration.ShopUiBaseUrl);
        await shopUiPage.Locator("a[href='/order-history']").ClickAsync();
        await shopUiPage.Locator("[aria-label='Order Number']").FillAsync(orderNumber);
        await shopUiPage.Locator("[aria-label='Refresh Order List']").ClickAsync();

        var rowSelector = $"xpath=//tr[contains(., '{orderNumber}')]";
        await shopUiPage.Locator(rowSelector).WaitForAsync(new() { State = Microsoft.Playwright.WaitForSelectorState.Visible });
        (await shopUiPage.Locator(rowSelector).IsVisibleAsync()).ShouldBeTrue();

        var viewDetailsSelector = $"xpath=//tr[contains(., '{orderNumber}')]//a[contains(text(), 'View Details')]";
        await shopUiPage.Locator(viewDetailsSelector).ClickAsync();

        var totalPriceText = await shopUiPage.Locator("[aria-label='Display Total Price']").TextContentAsync();
        var totalPriceValue = decimal.Parse((totalPriceText ?? "").Replace("$", ""));
        totalPriceValue.ShouldBe(100.00m);
    }

    [Theory]
    [InlineData("20.00", "5", "100.00")]
    [InlineData("10.00", "3", "30.00")]
    [InlineData("15.50", "4", "62.00")]
    [InlineData("99.99", "1", "99.99")]
    public async Task ShouldPlaceOrderWithCorrectTotalPriceParameterized(string unitPrice, string quantity, string expectedTotalPrice)
    {
        var sku = CreateUniqueSku(Defaults.SKU);
        await CreateProductViaErpAsync(sku, unitPrice);

        await shopUiPage!.GotoAsync(_configuration.ShopUiBaseUrl);
        await shopUiPage.Locator("a[href='/shop']").ClickAsync();
        await shopUiPage.Locator("[aria-label=\"SKU\"]").FillAsync(sku);
        await shopUiPage.Locator("[aria-label=\"Quantity\"]").FillAsync(quantity);

        await shopUiPage.Locator("[aria-label=\"Place Order\"]").ClickAsync();

        var successMessageText = await shopUiPage.Locator("[role='alert']").TextContentAsync();
        var match = Regex.Match(successMessageText ?? "", @"Success! Order has been created with Order Number ([\w-]+)");
        match.Success.ShouldBeTrue();
        var orderNumber = match.Groups[1].Value;

        await shopUiPage.GotoAsync(_configuration.ShopUiBaseUrl);
        await shopUiPage.Locator("a[href='/order-history']").ClickAsync();
        await shopUiPage.Locator("[aria-label='Order Number']").FillAsync(orderNumber);
        await shopUiPage.Locator("[aria-label='Refresh Order List']").ClickAsync();

        var rowSelector = $"xpath=//tr[contains(., '{orderNumber}')]";
        await shopUiPage.Locator(rowSelector).WaitForAsync(new() { State = Microsoft.Playwright.WaitForSelectorState.Visible });
        var viewDetailsSelector = $"xpath=//tr[contains(., '{orderNumber}')]//a[contains(text(), 'View Details')]";
        await shopUiPage.Locator(viewDetailsSelector).ClickAsync();

        var totalPriceText = await shopUiPage.Locator("[aria-label='Display Total Price']").TextContentAsync();
        var totalPriceValue = decimal.Parse((totalPriceText ?? "").Replace("$", ""));
        totalPriceValue.ShouldBe(decimal.Parse(expectedTotalPrice));
    }

    [Fact]
    public async Task ShouldPlaceOrder()
    {
        var sku = CreateUniqueSku(Defaults.SKU);
        await CreateProductViaErpAsync(sku, "20.00");

        await shopUiPage!.GotoAsync(_configuration.ShopUiBaseUrl);
        await shopUiPage.Locator("a[href='/shop']").ClickAsync();
        await shopUiPage.Locator("[aria-label=\"SKU\"]").FillAsync(sku);
        await shopUiPage.Locator("[aria-label=\"Quantity\"]").FillAsync("5");

        await shopUiPage.Locator("[aria-label=\"Place Order\"]").ClickAsync();

        var successMessageText = await shopUiPage.Locator("[role='alert']").TextContentAsync();
        var match = Regex.Match(successMessageText ?? "", @"Success! Order has been created with Order Number ([\w-]+)");
        match.Success.ShouldBeTrue();
        var orderNumber = match.Groups[1].Value;
        orderNumber.ShouldStartWith("ORD-");

        await shopUiPage.GotoAsync(_configuration.ShopUiBaseUrl);
        await shopUiPage.Locator("a[href='/order-history']").ClickAsync();
        await shopUiPage.Locator("[aria-label='Order Number']").FillAsync(orderNumber);
        await shopUiPage.Locator("[aria-label='Refresh Order List']").ClickAsync();

        var rowSelector = $"xpath=//tr[contains(., '{orderNumber}')]";
        await shopUiPage.Locator(rowSelector).WaitForAsync(new() { State = Microsoft.Playwright.WaitForSelectorState.Visible });
        var viewDetailsSelector = $"xpath=//tr[contains(., '{orderNumber}')]//a[contains(text(), 'View Details')]";
        await shopUiPage.Locator(viewDetailsSelector).ClickAsync();

        (await shopUiPage.Locator("[aria-label='Display Order Number']").TextContentAsync()).ShouldBe(orderNumber);
        (await shopUiPage.Locator("[aria-label='Display SKU']").TextContentAsync()).ShouldBe(sku);
        int.Parse(await shopUiPage.Locator("[aria-label='Display Quantity']").TextContentAsync() ?? "0").ShouldBe(5);
        decimal.Parse((await shopUiPage.Locator("[aria-label='Display Unit Price']").TextContentAsync() ?? "").Replace("$", "")).ShouldBe(20.00m);
        decimal.Parse((await shopUiPage.Locator("[aria-label='Display Total Price']").TextContentAsync() ?? "").Replace("$", "")).ShouldBe(100.00m);
        (await shopUiPage.Locator("[aria-label='Display Status']").TextContentAsync()).ShouldBe("PLACED");
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











