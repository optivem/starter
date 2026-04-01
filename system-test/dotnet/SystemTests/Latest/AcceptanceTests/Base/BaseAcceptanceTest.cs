using Dsl.Port;
using Dsl.Core.Shared;
using SystemTests.Latest.Base;

namespace SystemTests.Latest.AcceptanceTests.Base;

public abstract class BaseAcceptanceTest : BaseScenarioDslTest
{
    protected override ExternalSystemMode? GetFixedExternalSystemMode()
    {
        return ExternalSystemMode.Stub;
    }
}












