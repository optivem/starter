using Dsl.Port.Then.Steps;

namespace Dsl.Port.Then;

public interface IThenResultStage
{
    IThenSuccess ShouldSucceed();

    IThenFailure ShouldFail();
}
