using Common;
using SystemTests.Legacy.Mod04.Base;
using Xunit;

namespace SystemTests.Legacy.Mod04.SmokeTests.External;

public class TaxSmokeTest : BaseClientTest
{
    public override Task InitializeAsync()
    {
        SetUpExternalClients();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task ShouldBeAbleToGoToTax()
    {
        var result = await _taxClient!.CheckHealthAsync();
        result.ShouldBeSuccess();
    }
}
