using Dsl.Port.When.Steps.Base;

namespace Dsl.Port.When.Steps;

public interface ICancelOrder : IWhenStep
{
    ICancelOrder WithOrderNumber(string? orderNumber);
}
