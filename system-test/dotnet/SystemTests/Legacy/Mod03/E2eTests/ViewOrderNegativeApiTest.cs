using System.Text.Json;
using SystemTests.Legacy.Mod03.E2eTests.Base;
using Shouldly;
using Xunit;

namespace SystemTests.Legacy.Mod03.E2eTests;

public class ViewOrderNegativeApiTest : BaseE2eTest
{
    protected override Task SetShopRawAsync()
    {
        SetUpShopHttpClient();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task ShouldNotBeAbleToViewNonExistentOrder()
    {
        const string orderNumber = "NON-EXISTENT-ORDER-99999";
        var uri = new Uri(_configuration.ShopApiBaseUrl + $"/api/orders/{orderNumber}");
        var response = await _shopApiHttpClient!.GetAsync(uri);
        ((int)response.StatusCode).ShouldBe(404);
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("detail").GetString().ShouldBe("Order NON-EXISTENT-ORDER-99999 does not exist.");
    }
}











