using Dsl.Port;

namespace SystemTests.Legacy.Mod11.ExternalSystemContractTests.Clock;

public class ClockRealContractTest : BaseClockContractTest
{
    protected override ExternalSystemMode? FixedExternalSystemMode => ExternalSystemMode.Real;
}












