using Driver.Port.External.Tax;
using Driver.Port.External.Tax.Dtos.Error;
using Dsl.Core.External.Tax.UseCases.Base;
using Common;
using Dsl.Core.Shared;

namespace Dsl.Core.External.Tax.UseCases;

public class GoToTax : BaseTaxCommand<VoidValue, VoidVerification>
{
    public GoToTax(ITaxDriver driver, UseCaseContext context)
        : base(driver, context)
    {
    }

    public override async Task<TaxUseCaseResult<VoidValue, VoidVerification>> Execute()
    {
        var result = await _driver.GoToTaxAsync();

        return new TaxUseCaseResult<VoidValue, VoidVerification>(
            result,
            _context,
            (response, ctx) => new VoidVerification(response, ctx));
    }
}
