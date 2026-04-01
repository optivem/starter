using Dsl.Port.Assume;
using Dsl.Port.Given;
using Dsl.Port.When;

namespace Dsl.Port;

public interface IScenarioDsl
{
    IAssumeStage Assume();

    IGivenStage Given();

    IWhenStage When();
}
