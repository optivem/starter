using Dsl.Port;

namespace SystemTests.Latest.ExternalSystemContractTests.Clock;

public class ClockStubContractTest : BaseClockContractTest
{
    protected override ExternalSystemMode? FixedExternalSystemMode => ExternalSystemMode.Stub;

    [Fact]
    public async Task ShouldBeAbleToGetConfiguredTime()
    {
        (await Scenario()
            .Given().Clock().WithTime("2024-01-02T09:00:00Z")
            .Then().Clock())
            .HasTime("2024-01-02T09:00:00Z");
    }
}












