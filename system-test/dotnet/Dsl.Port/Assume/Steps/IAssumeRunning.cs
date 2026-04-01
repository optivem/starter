using Dsl.Port.Assume;

namespace Dsl.Port.Assume.Steps;

public interface IAssumeRunning
{
    Task<IAssumeStage> ShouldBeRunning();
}