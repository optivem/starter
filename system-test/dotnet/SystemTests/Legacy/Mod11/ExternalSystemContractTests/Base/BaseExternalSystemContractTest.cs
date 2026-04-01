using Dsl.Port;
using Dsl.Core.Shared;
using SystemTests.Legacy.Mod11.Base;

namespace SystemTests.Legacy.Mod11.ExternalSystemContractTests.Base;

public abstract class BaseExternalSystemContractTest : BaseScenarioDslTest
{
    protected abstract ExternalSystemMode? FixedExternalSystemMode { get; }

    protected sealed override ExternalSystemMode? GetFixedExternalSystemMode() => FixedExternalSystemMode;
}













