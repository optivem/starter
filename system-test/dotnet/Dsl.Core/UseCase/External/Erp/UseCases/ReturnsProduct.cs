using Driver.Port.External.Erp;
using Driver.Port.External.Erp.Dtos;
using Driver.Port.External.Erp.Dtos.Error;
using Dsl.Core.External.Erp.UseCases.Base;
using Common;
using Dsl.Core.Shared;

namespace Dsl.Core.External.Erp.UseCases;

public class ReturnsProduct : BaseErpCommand<VoidValue, VoidVerification>
{
    private string? _skuParamAlias;
    private string? _unitPrice;

    public ReturnsProduct(IErpDriver driver, UseCaseContext context)
        : base(driver, context)
    {
    }

    public ReturnsProduct Sku(string? skuParamAlias)
    {
        _skuParamAlias = skuParamAlias;
        return this;
    }

    public ReturnsProduct UnitPrice(string? unitPrice)
    {
        _unitPrice = unitPrice;
        return this;
    }

    public ReturnsProduct UnitPrice(decimal price)
    {
        return UnitPrice(price.ToString());
    }

    public override async Task<ErpUseCaseResult<VoidValue, VoidVerification>> Execute()
    {
        var sku = _context.GetParamValue(_skuParamAlias);

        var request = new ReturnsProductRequest
        {
            Sku = sku,
            Price = _unitPrice
        };

        var result = await _driver.ReturnsProductAsync(request);

        return new ErpUseCaseResult<VoidValue, VoidVerification>(
            result,
            _context,
            (response, ctx) => new VoidVerification(response, ctx));
    }
}



