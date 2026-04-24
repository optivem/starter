using SystemTests.Legacy.Mod02.Base;
using Xunit;

namespace SystemTests.Legacy.Mod02.SmokeTests.System;

public class MyShopApiSmokeTest : BaseRawTest
{
    private const string HealthEndpoint = "/health";

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        SetUpMyShopHttpClient();
    }

    [Fact]
    public async Task ShouldBeAbleToGoToMyShop()
    {
        var uri = new Uri(_configuration.MyShopApiBaseUrl + HealthEndpoint);
        var request = new HttpRequestMessage(HttpMethod.Get, uri);

        var response = await _shopApiHttpClient!.SendAsync(request);

        Assert.Equal(200, (int)response.StatusCode);
    }
}











