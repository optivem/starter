using Dsl.Port.Given.Steps;
using Dsl.Core.Scenario.Given;
using static Dsl.Core.Scenario.ScenarioDefaults;

namespace Dsl.Core.Scenario.Given;

public class GivenCountry : BaseGiven, IGivenCountry
{
    private string? _country;
    private string? _taxRate;

    public GivenCountry(GivenStage givenClause)
        : base(givenClause)
    {
        WithCode(DefaultCountry);
        WithTaxRate(DefaultTaxRate);
    }

    public GivenCountry WithCode(string country)
    {
        _country = country;
        return this;
    }

    IGivenCountry IGivenCountry.WithCode(string country) => WithCode(country);

    public GivenCountry WithTaxRate(string taxRate)
    {
        _taxRate = taxRate;
        return this;
    }

    IGivenCountry IGivenCountry.WithTaxRate(string taxRate) => WithTaxRate(taxRate);

    public GivenCountry WithTaxRate(decimal taxRate)
    {
        return WithTaxRate(taxRate.ToString());
    }

    IGivenCountry IGivenCountry.WithTaxRate(decimal taxRate) => WithTaxRate(taxRate);

    internal override async Task Execute(UseCaseDsl app)
    {
        (await app.Tax().ReturnsTaxRate()
            .Country(_country)
            .TaxRate(_taxRate)
            .Execute())
            .ShouldSucceed();
    }
}
