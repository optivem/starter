using Dsl.Core.Scenario.Then.Steps;
using Dsl.Port.Then;
using Dsl.Port.Then.Steps;
using Optivem.Testing;

namespace Dsl.Core.Scenario.Then;

public class ThenStageBase : IThenStage
{
    protected readonly UseCaseDsl _app;
    private readonly Func<Task>? _setup;
    private bool _setupCompleted;

    public ThenStageBase(UseCaseDsl app, Func<Task>? setup = null)
    {
        _app = app;
        _setup = setup;
        _setupCompleted = false;
    }

    public async Task<IThenClock> Clock()
    {
        await EnsureSetup();
        var verification = (await _app.Clock().GetTime().Execute()).ShouldSucceed();
        return new Steps.ThenClock(_app, verification);
    }

    public async Task<IThenProduct> Product(string skuAlias)
    {
        await EnsureSetup();
        var verification = (await _app.Erp().GetProduct().Sku(skuAlias).Execute()).ShouldSucceed();
        return new Steps.ThenProduct(_app, verification);
    }

    public async Task<IThenCountry> Country(string countryAlias)
    {
        await EnsureSetup();
        var verification = (await _app.Tax().GetTaxRate().Country(countryAlias).Execute()).ShouldSucceed();
        return new Steps.ThenCountry(_app, verification);
    }

    protected async Task EnsureSetup()
    {
        if (!_setupCompleted && _setup != null)
        {
            await _setup();
            _setupCompleted = true;
        }
    }
}
