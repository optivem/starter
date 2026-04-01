using System.Text;
using System.Text.Json;
using SystemTests.Commons.Providers;
using SystemTests.Commons.Constants;
using SystemTests.Legacy.Mod03.E2eTests.Base;
using Shouldly;
using Xunit;

namespace SystemTests.Legacy.Mod03.E2eTests;

public class PlaceOrderNegativeApiTest : BaseE2eTest
{
    protected override Task SetShopRawAsync()
    {
        SetUpShopHttpClient();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task ShouldRejectOrderWithInvalidQuantity()
    {
        var placeOrderJson = $$"""{"sku":"{{CreateUniqueSku(Defaults.SKU)}}","quantity":"invalid-quantity"}""";
        var (statusCode, body) = await PlaceOrderViaApiAsync(placeOrderJson);
        AssertValidationError(statusCode, body, "quantity", "Quantity must be an integer");
    }

    [Fact]
    public async Task ShouldRejectOrderWithNonExistentSku()
    {
        var placeOrderJson = $$"""{"sku":"NON-EXISTENT-SKU-12345","quantity":"{{Defaults.QUANTITY}}"}""";
        var (statusCode, body) = await PlaceOrderViaApiAsync(placeOrderJson);
        AssertValidationError(statusCode, body, "sku", "Product does not exist for SKU: NON-EXISTENT-SKU-12345");
    }

    [Fact]
    public async Task ShouldRejectOrderWithNegativeQuantity()
    {
        var placeOrderJson = $$"""{"sku":"{{CreateUniqueSku(Defaults.SKU)}}","quantity":"-10"}""";
        var (statusCode, body) = await PlaceOrderViaApiAsync(placeOrderJson);
        AssertValidationError(statusCode, body, "quantity", "Quantity must be positive");
    }

    [Fact]
    public async Task ShouldRejectOrderWithZeroQuantity()
    {
        var placeOrderJson = $$"""{"sku":"{{CreateUniqueSku(Defaults.SKU)}}","quantity":"0"}""";
        var (statusCode, body) = await PlaceOrderViaApiAsync(placeOrderJson);
        AssertValidationError(statusCode, body, "quantity", "Quantity must be positive");
    }

    [Fact]
    public async Task ShouldRejectOrderWithEmptySku()
    {
        var placeOrderJson = $$"""{"sku":"","quantity":"{{Defaults.QUANTITY}}"}""";
        var (statusCode, body) = await PlaceOrderViaApiAsync(placeOrderJson);
        AssertValidationError(statusCode, body, "sku", "SKU must not be empty");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  ")]
    public async Task ShouldRejectOrderWithEmptyQuantity(string emptyQuantity)
    {
        var placeOrderJson = $$"""{"sku":"{{CreateUniqueSku(Defaults.SKU)}}","quantity":"{{emptyQuantity}}"}""";
        var (statusCode, body) = await PlaceOrderViaApiAsync(placeOrderJson);
        AssertValidationError(statusCode, body, "quantity", "Quantity must not be empty");
    }

    [Theory]
    [InlineData("3.5")]
    [InlineData("lala")]
    public async Task ShouldRejectOrderWithNonIntegerQuantity(string nonIntegerQuantity)
    {
        var placeOrderJson = $$"""{"sku":"{{CreateUniqueSku(Defaults.SKU)}}","quantity":"{{nonIntegerQuantity}}"}""";
        var (statusCode, body) = await PlaceOrderViaApiAsync(placeOrderJson);
        AssertValidationError(statusCode, body, "quantity", "Quantity must be an integer");
    }

    [Fact]
    public async Task ShouldRejectOrderWithNullQuantity()
    {
        var placeOrderJson = $$"""{"sku":"{{CreateUniqueSku(Defaults.SKU)}}","quantity":null}""";
        var (statusCode, body) = await PlaceOrderViaApiAsync(placeOrderJson);
        AssertValidationError(statusCode, body, "quantity", "Quantity must not be empty");
    }

    [Fact]
    public async Task ShouldRejectOrderWithNullSku()
    {
        var placeOrderJson = $$"""{"sku":null,"quantity":"{{Defaults.QUANTITY}}"}""";
        var (statusCode, body) = await PlaceOrderViaApiAsync(placeOrderJson);
        AssertValidationError(statusCode, body, "sku", "SKU must not be empty");
    }

    private async Task CreateProductViaErpAsync(string sku, string price)
    {
        var json = $$"""{"id":"{{sku}}","title":"Test Product","description":"Test Description","category":"Test Category","brand":"Test Brand","price":"{{price}}"}""";
        var uri = new Uri(_configuration.ErpBaseUrl + "/api/products");
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _erpHttpClient!.PostAsync(uri, content);
        ((int)response.StatusCode).ShouldBe(201);
    }

    private async Task<(int StatusCode, string Body)> PlaceOrderViaApiAsync(string placeOrderJson)
    {
        var uri = new Uri(_configuration.ShopApiBaseUrl + "/api/orders");
        var content = new StringContent(placeOrderJson, Encoding.UTF8, "application/json");
        var response = await _shopApiHttpClient!.PostAsync(uri, content);
        var body = await response.Content.ReadAsStringAsync();
        return ((int)response.StatusCode, body);
    }

    private static void AssertValidationError(int statusCode, string responseBody, string field, string message)
    {
        statusCode.ShouldBe(422);
        using var doc = JsonDocument.Parse(responseBody);
        var root = doc.RootElement;
        root.GetProperty("detail").GetString().ShouldBe("The request contains one or more validation errors");
        var errors = root.GetProperty("errors");
        errors.GetArrayLength().ShouldBeGreaterThan(0);
        var found = false;
        foreach (var err in errors.EnumerateArray())
        {
            if (err.GetProperty("field").GetString() == field && err.GetProperty("message").GetString() == message)
            {
                found = true;
                break;
            }
        }
        found.ShouldBeTrue($"Expected errors to contain field='{field}', message='{message}'");
    }
}











