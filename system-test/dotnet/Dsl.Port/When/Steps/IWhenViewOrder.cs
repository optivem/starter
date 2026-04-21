using Dsl.Port.When.Steps.Base;

namespace Dsl.Port.When.Steps;

public interface IWhenViewOrder : IWhenStep
{
    IWhenViewOrder WithOrderNumber(string? orderNumber);
}
