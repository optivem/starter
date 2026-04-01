using Dsl.Port;
using Dsl.Core.Shared;
using SystemTests.Legacy.Mod06.Base;
using Dsl.Core.Shop;

namespace SystemTests.Legacy.Mod06.E2eTests.Base;

public abstract class BaseE2eTest : BaseChannelDriverTest
{
    protected override ExternalSystemMode? GetFixedExternalSystemMode()
    {
        return ExternalSystemMode.Real;
    }

    protected static string CreateUniqueSku(string baseSku)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        return $"{baseSku}-{suffix}";
    }
}













