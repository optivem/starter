using Common;
using SystemTests.Legacy.Mod04.Base;
using Xunit;

namespace SystemTests.Legacy.Mod04.SmokeTests.External;

public class ErpSmokeTest : BaseClientTest
{
    public override Task InitializeAsync()
    {
        SetUpExternalClients();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task ShouldBeAbleToGoToErp()
    {
        var result = await _erpClient!.CheckHealthAsync();
        result.ShouldBeSuccess();
    }
}











