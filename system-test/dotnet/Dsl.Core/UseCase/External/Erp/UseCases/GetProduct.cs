using Driver.Port.External.Erp;
using Driver.Port.External.Erp.Dtos;
using Driver.Port.External.Erp.Dtos.Error;
using Dsl.Core.External.Erp.UseCases.Base;
using Dsl.Core.Shared;

namespace Dsl.Core.External.Erp.UseCases;

public class GetProduct : BaseErpCommand<GetProductResponse, GetProductVerification>
{
    private string? _skuParamAlias;

    public GetProduct(IErpDriver driver, UseCaseContext context)
        : base(driver, context)
    {
    }

    public GetProduct Sku(string? skuParamAlias)
    {
        _skuParamAlias = skuParamAlias;
        return this;
    }

    public override async Task<ErpUseCaseResult<GetProductResponse, GetProductVerification>> Execute()
    {
        var sku = _context.GetParamValue(_skuParamAlias);

        var request = new GetProductRequest
        {
            Sku = sku
        };

        var result = await _driver.GetProductAsync(request);

        return new ErpUseCaseResult<GetProductResponse, GetProductVerification>(
            result,
            _context,
            (response, ctx) => new GetProductVerification(response, ctx));
    }
}



