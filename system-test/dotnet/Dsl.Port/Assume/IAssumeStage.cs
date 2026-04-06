namespace Dsl.Port.Assume;

using Dsl.Port.Assume.Steps;

public interface IAssumeStage
{
    IAssumeRunning Shop();

    IAssumeRunning Erp();

    IAssumeRunning Tax();

    IAssumeRunning Clock();
}
