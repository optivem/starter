using Dsl.Core.Scenario.When.Steps.Base;
using Dsl.Port;
using Dsl.Port.When.Steps;
using Dsl.Core.Shared;
using Driver.Adapter;
using Driver.Port.MyShop.Dtos;
using Dsl.Core.MyShop.UseCases;

namespace Dsl.Core.Scenario.When.Steps;

public class WhenBrowseCoupons : BaseWhen<BrowseCouponsResponse, BrowseCouponsVerification>, IWhenBrowseCoupons
{
    public WhenBrowseCoupons(UseCaseDsl app, ScenarioDsl scenario, Func<Task> ensureGiven)
        : base(app, scenario, ensureGiven)
    {
    }

    protected override async Task<ExecutionResult<BrowseCouponsResponse, BrowseCouponsVerification>> Execute(UseCaseDsl app)
    {
        var shop = await app.MyShop(ChannelMode.Dynamic, Channel);
        var result = await shop.BrowseCoupons()
            .Execute();

        return new ExecutionResultBuilder<BrowseCouponsResponse, BrowseCouponsVerification>(result)
            .Build();
    }
}
