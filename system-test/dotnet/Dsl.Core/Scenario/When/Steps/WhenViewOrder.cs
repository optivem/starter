using Driver.Adapter;
using Dsl.Core.Scenario.When.Steps.Base;
using Dsl.Port;
using Dsl.Port.When.Steps;
using Driver.Port.MyShop.Dtos;
using Dsl.Core.MyShop.UseCases;
using Optivem.Testing;
using static Dsl.Core.Scenario.ScenarioDefaults;

namespace Dsl.Core.Scenario.When.Steps;

public class ViewOrder : BaseWhen<ViewOrderResponse, ViewOrderVerification>, IWhenViewOrder
{
    private string? _orderNumber;

    public ViewOrder(UseCaseDsl app, ScenarioDsl scenario, Func<Task> ensureGiven) : base(app, scenario, ensureGiven)
    {
        WithOrderNumber(DefaultOrderNumber);
    }

    public ViewOrder WithOrderNumber(string? orderNumber)
    {
        _orderNumber = orderNumber;
        return this;
    }

    IWhenViewOrder IWhenViewOrder.WithOrderNumber(string? orderNumber) => WithOrderNumber(orderNumber);

    protected override async Task<ExecutionResult<ViewOrderResponse, ViewOrderVerification>> Execute(UseCaseDsl app)
    {
        var shop = await app.MyShop(ChannelMode.Dynamic, Channel);
        var result = await shop.ViewOrder()
            .OrderNumber(_orderNumber)
            .Execute();

        return new ExecutionResultBuilder<ViewOrderResponse, ViewOrderVerification>(result)
            .OrderNumber(_orderNumber)
            .Build();
    }
}



