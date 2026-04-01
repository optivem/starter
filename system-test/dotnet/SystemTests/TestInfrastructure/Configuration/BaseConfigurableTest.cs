using Dsl.Port;
using Dsl.Core.Shared;
using Dsl.Core;

namespace SystemTests.TestInfrastructure.Configuration;

public abstract class BaseConfigurableTest
{
    protected virtual Environment? GetFixedEnvironment()
    {
        return null;
    }

    protected virtual ExternalSystemMode? GetFixedExternalSystemMode()
    {
        return null;
    }

    protected Dsl.Core.Configuration LoadConfiguration()
    {
        var fixedEnvironment = GetFixedEnvironment();
        var fixedExternalSystemMode = GetFixedExternalSystemMode();

        var environment = PropertyLoader.GetEnvironment(fixedEnvironment);
        var externalSystemMode = PropertyLoader.GetExternalSystemMode(fixedExternalSystemMode);

        return SystemConfigurationLoader.Load(environment, externalSystemMode);
    }
}












