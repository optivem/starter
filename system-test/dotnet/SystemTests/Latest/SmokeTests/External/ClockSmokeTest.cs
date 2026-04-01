using SystemTests.Latest.Base;
using Xunit;

namespace SystemTests.Latest.SmokeTests.External;

public class ClockSmokeTest : BaseScenarioDslTest
{
    [Fact]
    public async Task ShouldBeAbleToGoToClock()
    {
        await Scenario().Assume().Clock().ShouldBeRunning();
    }
}










