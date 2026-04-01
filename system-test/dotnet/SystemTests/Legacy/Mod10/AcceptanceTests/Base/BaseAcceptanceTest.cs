using Dsl.Port;
using Dsl.Core.Shared;
using SystemTests.Legacy.Mod10.Base;

namespace SystemTests.Legacy.Mod10.AcceptanceTests.Base;

public abstract class BaseAcceptanceTest : BaseScenarioDslTest
{
    protected override ExternalSystemMode? GetFixedExternalSystemMode()
    {
        return ExternalSystemMode.Stub;
    }
}












