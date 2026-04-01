using SystemTests.Latest.Base;
using Xunit;

namespace SystemTests.Latest.SmokeTests.External;

public class ErpSmokeTest : BaseScenarioDslTest
{
    [Fact]
    public async Task ShouldBeAbleToGoToErp()
    {
        await Scenario().Assume().Erp().ShouldBeRunning();
    }
}










