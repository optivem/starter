using Dsl.Core.External.Clock.UseCases;
using Dsl.Port.Then.Steps;
using Driver.Adapter;

namespace Dsl.Core.Scenario.Then.Steps;

public class ThenClock : IThenClock
{
    private readonly UseCaseDsl _app;
    private readonly GetTimeVerification _verification;

    public ThenClock(UseCaseDsl app, GetTimeVerification verification)
    {
        _app = app;
        _verification = verification;
    }

    public IThenClock HasTime(string time)
    {
        _verification.Time(time);
        return this;
    }

    public IThenClock HasTime()
    {
        _verification.TimeIsNotNull();
        return this;
    }

    public async Task<IThenClock> Clock()
    {
        var verification = (await _app.Clock().GetTime().Execute()).ShouldSucceed();
        return new ThenClock(_app, verification);
    }

    public async Task<IThenProduct> Product(string skuAlias)
    {
        var verification = (await _app.Erp().GetProduct().Sku(skuAlias).Execute()).ShouldSucceed();
        return new ThenProduct(_app, verification);
    }

    public async Task<IThenCountry> Country(string countryAlias)
    {
        var verification = (await _app.Tax().GetTaxRate().Country(countryAlias).Execute()).ShouldSucceed();
        return new ThenCountry(_app, verification);
    }
}
