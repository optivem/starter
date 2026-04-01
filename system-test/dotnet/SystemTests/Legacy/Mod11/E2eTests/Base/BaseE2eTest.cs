using Dsl.Port;
using Dsl.Core.Shared;
using SystemTests.Legacy.Mod11.Base;

namespace SystemTests.Legacy.Mod11.E2eTests.Base;

public abstract class BaseE2eTest : BaseScenarioDslTest
{
    protected override ExternalSystemMode? GetFixedExternalSystemMode()
    {
        return ExternalSystemMode.Real;
    }
}












