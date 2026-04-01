using SystemTests.Legacy.Mod09.Base;
using Xunit;

namespace SystemTests.Legacy.Mod09.SmokeTests.External;

public class ErpSmokeTest : BaseScenarioDslTest
{
    [Fact]
    public async Task ShouldBeAbleToGoToErp()
    {
        await Scenario().Assume().Erp().ShouldBeRunning();
    }
}










