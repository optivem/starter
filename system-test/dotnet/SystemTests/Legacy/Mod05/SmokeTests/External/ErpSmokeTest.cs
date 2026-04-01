using Common;
using SystemTests.Legacy.Mod05.Base;
using Shouldly;
using Xunit;

namespace SystemTests.Legacy.Mod05.SmokeTests.External;

public class ErpSmokeTest : BaseDriverTest
{
    public override Task InitializeAsync()
    {
        SetUpExternalDrivers();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task ShouldBeAbleToGoToErp()
    {
        var result = await _erpDriver!.GoToErpAsync();
        result.ShouldBeSuccess();
    }
}










