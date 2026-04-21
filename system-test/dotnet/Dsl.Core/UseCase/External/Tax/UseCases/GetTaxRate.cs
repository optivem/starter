using Driver.Port.External.Tax;
using Driver.Port.External.Tax.Dtos;
using Driver.Port.External.Tax.Dtos.Error;
using Dsl.Core.External.Tax.UseCases.Base;
using Dsl.Core.Shared;

namespace Dsl.Core.External.Tax.UseCases;

public class GetTaxRate : BaseTaxUseCase<GetTaxResponse, GetTaxVerification>
{
    private string? country;

    public GetTaxRate(ITaxDriver driver, UseCaseContext context)
        : base(driver, context)
    {
    }

    public GetTaxRate Country(string? country)
    {
        this.country = country;
        return this;
    }

    public override async Task<TaxUseCaseResult<GetTaxResponse, GetTaxVerification>> Execute()
    {
        var countryValue = _context.GetParamValueOrLiteral(country);

        var result = await _driver.GetTaxRateAsync(countryValue);

        return new TaxUseCaseResult<GetTaxResponse, GetTaxVerification>(
            result,
            _context,
            (response, ctx) => new GetTaxVerification(response, ctx));
    }
}
