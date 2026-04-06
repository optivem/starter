using Driver.Port.External.Tax.Dtos;
using Dsl.Core.Shared;
using Common;
using Shouldly;

namespace Dsl.Core.External.Tax.UseCases;

public class GetTaxVerification : ResponseVerification<GetTaxResponse>
{
    public GetTaxVerification(GetTaxResponse response, UseCaseContext context)
        : base(response, context)
    {
    }

    public GetTaxVerification Country(string expectedCountryAlias)
    {
        var expectedCountry = Context.GetParamValueOrLiteral(expectedCountryAlias);
        var actualCountry = Response.Country;
        actualCountry.ShouldBe(expectedCountry, $"Expected country to be '{expectedCountry}', but was '{actualCountry}'");
        return this;
    }

    public GetTaxVerification TaxRate(decimal expectedTaxRate)
    {
        var actualTaxRate = Response.TaxRate;
        actualTaxRate.ShouldBe(expectedTaxRate, $"Expected tax rate to be {expectedTaxRate}, but was {actualTaxRate}");
        return this;
    }

    public GetTaxVerification TaxRate(string expectedTaxRate)
    {
        return TaxRate(Converter.ToDecimal(expectedTaxRate)!.Value);
    }

    public GetTaxVerification TaxRateIsPositive()
    {
        var actualTaxRate = Response.TaxRate;
        actualTaxRate.ShouldBeGreaterThan(0m, $"Expected tax rate to be positive, but was {actualTaxRate}");
        return this;
    }
}
