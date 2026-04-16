using Dsl.Core.External.Erp.UseCases;
using Dsl.Port.Then.Steps;
using Driver.Adapter;

namespace Dsl.Core.Scenario.Then.Steps;

public class ThenProduct : IThenProduct
{
    private readonly UseCaseDsl _app;
    private readonly GetProductVerification _verification;

    public ThenProduct(UseCaseDsl app, GetProductVerification verification)
    {
        _app = app;
        _verification = verification;
    }

    public IThenProduct HasSku(string sku)
    {
        _verification.Sku(sku);
        return this;
    }

    public IThenProduct HasPrice(decimal price)
    {
        _verification.Price(price);
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
