using SystemTests.Legacy.Mod09.Base;
using Xunit;

namespace SystemTests.Legacy.Mod09.SmokeTests.External;

public class ClockSmokeTest : BaseScenarioDslTest
{
    [Fact]
    public async Task ShouldBeAbleToGoToClock()
    {
        await Scenario().Assume().Clock().ShouldBeRunning();
    }
}










