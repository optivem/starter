using Dsl.Core.Scenario.Assume;
using Dsl.Core.Scenario.Given;
using Dsl.Core.Scenario.When;
using Dsl.Port;
using Dsl.Port.Assume;
using Dsl.Port.Given;
using Dsl.Port.When;
using Driver.Adapter;
using Optivem.Testing;
using System;

namespace Dsl.Core.Scenario;

public class ScenarioDsl : IScenarioDsl
{
    private readonly Channel? _channel;
    private readonly UseCaseDsl _app;

    private bool _executed = false;

    public ScenarioDsl(Channel? channel, UseCaseDsl app)
    {
        _channel = channel;
        _app = app;
    }

    internal Channel? Channel => _channel;

    public AssumeStage Assume()
    {
        return new AssumeStage(_app, _channel);
    }

    IAssumeStage IScenarioDsl.Assume() => Assume();

    public GivenStage Given()
    {
        EnsureNotExecuted();
        return new GivenStage(_channel, _app, this);
    }

    IGivenStage IScenarioDsl.Given() => Given();

    public WhenStage When()
    {
        EnsureNotExecuted();
        return new WhenStage(_channel, _app, this);
    }

    IWhenStage IScenarioDsl.When() => When();

    public void MarkAsExecuted()
    {
        _executed = true;
    }

    private void EnsureNotExecuted()
    {
        if (_executed)
        {
            throw new InvalidOperationException("Scenario has already been executed. " +
                "Each test method should contain only ONE scenario execution (Given-When-Then). " +
                "Split multiple scenarios into separate test methods.");
        }
    }
}


