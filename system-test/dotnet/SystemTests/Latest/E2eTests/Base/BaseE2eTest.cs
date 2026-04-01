using Dsl.Port;
using Dsl.Core.Shared;
using SystemTests.Latest.Base;

namespace SystemTests.Latest.E2eTests.Base;

public abstract class BaseE2eTest : BaseScenarioDslTest
{
    protected override ExternalSystemMode? GetFixedExternalSystemMode()
    {
        return ExternalSystemMode.Real;
    }
}












