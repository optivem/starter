using Dsl.Port.When.Steps.Base;

namespace Dsl.Port.When.Steps;

public interface IWhenCancelOrder : IWhenStep
{
    IWhenCancelOrder WithOrderNumber(string? orderNumber);
}
