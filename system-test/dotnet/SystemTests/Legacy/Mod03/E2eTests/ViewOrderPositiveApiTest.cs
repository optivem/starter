using System.Net;
using System.Text;
using System.Text.Json;
using SystemTests.Commons.Constants;
using SystemTests.Legacy.Mod03.E2eTests.Base;
using Shouldly;
using Xunit;

namespace SystemTests.Legacy.Mod03.E2eTests;

public class ViewOrderPositiveApiTest : BaseE2eTest
{
    protected override Task SetShopRawAsync()
    {
        SetUpShopHttpClient();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task ShouldViewPlacedOrder()
    {
        var sku = CreateUniqueSku(Defaults.SKU);
        await CreateProductViaErpAsync(sku, "20.00");

        var placeOrderJson = $$"""{"sku":"{{sku}}","quantity":"5"}""";
        var orderNumber = await PlaceOrderViaApiAsync(placeOrderJson);

        var uri = new Uri(_configuration.ShopApiBaseUrl + $"/api/orders/{orderNumber}");
        var response = await _shopApiHttpClient!.GetAsync(uri);
        ((int)response.StatusCode).ShouldBe((int)HttpStatusCode.OK);

        var bodyStr = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(bodyStr);
        var order = doc.RootElement;
        order.GetProperty("orderNumber").GetString().ShouldBe(orderNumber);
        order.GetProperty("sku").GetString().ShouldBe(sku);
        order.GetProperty("quantity").GetInt32().ShouldBe(5);
        order.GetProperty("unitPrice").GetDecimal().ShouldBe(20.00m);
        order.GetProperty("totalPrice").GetDecimal().ShouldBe(100.00m);
        order.GetProperty("status").GetString().ShouldBe("PLACED");
    }

    private async Task CreateProductViaErpAsync(string sku, string price)
    {
        var json = $$"""{"id":"{{sku}}","title":"Test Product","description":"Test Description","category":"Test Category","brand":"Test Brand","price":"{{price}}"}""";
        var uri = new Uri(_configuration.ErpBaseUrl + "/api/products");
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _erpHttpClient!.PostAsync(uri, content);
        ((int)response.StatusCode).ShouldBe(201);
    }

    private async Task<string> PlaceOrderViaApiAsync(string placeOrderJson)
    {
        var uri = new Uri(_configuration.ShopApiBaseUrl + "/api/orders");
        var content = new StringContent(placeOrderJson, Encoding.UTF8, "application/json");
        var response = await _shopApiHttpClient!.PostAsync(uri, content);
        ((int)response.StatusCode).ShouldBe(201);
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        return doc.RootElement.GetProperty("orderNumber").GetString()!;
    }
}











