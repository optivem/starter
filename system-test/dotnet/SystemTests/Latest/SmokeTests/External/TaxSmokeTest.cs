using SystemTests.Latest.Base;
using Xunit;

namespace SystemTests.Latest.SmokeTests.External;

public class TaxSmokeTest : BaseScenarioDslTest
{
    [Fact]
    public async Task ShouldBeAbleToGoToTax()
    {
        await Scenario().Assume().Tax().ShouldBeRunning();
    }
}
