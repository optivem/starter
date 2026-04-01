using Dsl.Port;
using Dsl.Core.Shared;
using SystemTests.Latest.Base;

namespace SystemTests.Latest.ExternalSystemContractTests.Base;

public abstract class BaseExternalSystemContractTest : BaseScenarioDslTest
{
    protected abstract ExternalSystemMode? FixedExternalSystemMode { get; }

    protected sealed override ExternalSystemMode? GetFixedExternalSystemMode() => FixedExternalSystemMode;
}













