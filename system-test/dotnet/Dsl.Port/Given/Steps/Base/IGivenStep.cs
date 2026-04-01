using Dsl.Port.Given;
using Dsl.Port.Then;
using Dsl.Port.When;

namespace Dsl.Port.Given.Steps.Base;

public interface IGivenStep
{
    IGivenStage And();

    IWhenStage When();

    IThenStage Then();
}
