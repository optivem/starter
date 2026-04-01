using SystemTests.Legacy.Mod03.Base;
using Xunit;

namespace SystemTests.Legacy.Mod03.SmokeTests.External;

public class ErpSmokeTest : BaseRawTest
{
    private const string HealthEndpoint = "/health";

    public override Task InitializeAsync()
    {
        SetUpExternalHttpClients();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task ShouldBeAbleToGoToErp()
    {
        var uri = new Uri(_configuration.ErpBaseUrl + HealthEndpoint);
        var request = new HttpRequestMessage(HttpMethod.Get, uri);

        var response = await _erpHttpClient!.SendAsync(request);

        Assert.Equal(200, (int)response.StatusCode);
    }
}











