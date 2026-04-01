using System.Text;
using System.Text.RegularExpressions;
using SystemTests.Commons.Constants;
using SystemTests.Legacy.Mod03.E2eTests.Base;
using Shouldly;
using Xunit;

namespace SystemTests.Legacy.Mod03.E2eTests;

public class ViewOrderPositiveUiTest : BaseE2eTest
{
    protected override async Task SetShopRawAsync()
    {
        await SetUpShopBrowserAsync();
    }

    [Fact]
    public async Task ShouldViewPlacedOrder()
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











