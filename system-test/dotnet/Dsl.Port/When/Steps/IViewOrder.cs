using Dsl.Port.When.Steps.Base;

namespace Dsl.Port.When.Steps;

public interface IViewOrder : IWhenStep
{
    IViewOrder WithOrderNumber(string? orderNumber);
}
