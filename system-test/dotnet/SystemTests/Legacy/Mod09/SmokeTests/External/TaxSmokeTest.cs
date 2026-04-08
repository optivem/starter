using SystemTests.Legacy.Mod09.Base;
using Xunit;

namespace SystemTests.Legacy.Mod09.SmokeTests.External;

public class TaxSmokeTest : BaseScenarioDslTest
{
    [Fact]
    public async Task ShouldBeAbleToGoToTax()
    {
        await Scenario().Assume().Tax().ShouldBeRunning();
    }
}
