using Dsl.Core.Scenario.Then;
using Dsl.Core.Scenario.When;
using Dsl.Port.Given;
using Dsl.Port.Given.Steps.Base;
using Dsl.Port.Then;
using Dsl.Port.When;
using Driver.Adapter;
using Optivem.Testing;

namespace Dsl.Core.Scenario.Given;

public abstract class BaseGiven : IGivenStep
{
    private readonly GivenStage _givenClause;

    protected BaseGiven(GivenStage givenClause)
    {
        _givenClause = givenClause;
    }

    public GivenStage And()
    {
        return _givenClause;
    }

    IGivenStage IGivenStep.And() => And();

    public WhenStage When()
    {
        return _givenClause.When();
    }

    IWhenStage IGivenStep.When() => When();

    public ThenStageBase Then()
    {
        return _givenClause.Then();
    }

    IThenStage IGivenStep.Then() => Then();

    internal abstract Task Execute(UseCaseDsl app);

    protected Channel? Channel => _givenClause.Channel;
}


