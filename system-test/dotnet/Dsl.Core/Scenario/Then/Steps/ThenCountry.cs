using Dsl.Core.External.Tax.UseCases;
using Dsl.Port.Then.Steps;
using Driver.Adapter;

namespace Dsl.Core.Scenario.Then.Steps;

public class ThenCountry : IThenCountry
{
    private readonly UseCaseDsl _app;
    private readonly GetTaxVerification _verification;

    public ThenCountry(UseCaseDsl app, GetTaxVerification verification)
    {
        _app = app;
        _verification = verification;
    }

    public IThenCountry HasCountry(string country)
    {
        _verification.Country(country);
        return this;
    }

    public IThenCountry HasTaxRate(decimal taxRate)
    {
        _verification.TaxRate(taxRate);
        return this;
    }

    public IThenCountry HasTaxRateIsPositive()
    {
        _verification.TaxRateIsPositive();
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
