using Driver.Port.External.Erp;
using Driver.Port.External.Erp.Dtos;
using Driver.Port.External.Erp.Dtos.Error;
using Dsl.Core.External.Erp.UseCases.Base;
using Common;
using Dsl.Core.Shared;

namespace Dsl.Core.External.Erp.UseCases;

public class ReturnsPromotion : BaseErpCommand<VoidValue, VoidVerification>
{
    private bool _promotionActive;
    private string? _discount;

    public ReturnsPromotion(IErpDriver driver, UseCaseContext context)
        : base(driver, context)
    {
    }

    public ReturnsPromotion WithActive(bool promotionActive)
    {
        _promotionActive = promotionActive;
        return this;
    }

    public ReturnsPromotion WithDiscount(string? discount)
    {
        _discount = discount;
        return this;
    }

    public ReturnsPromotion WithDiscount(decimal discount)
    {
        return WithDiscount(discount.ToString());
    }

    public override async Task<ErpUseCaseResult<VoidValue, VoidVerification>> Execute()
    {
        var request = new ReturnsPromotionRequest
        {
            PromotionActive = _promotionActive,
            Discount = _discount
        };

        var result = await _driver.ReturnsPromotionAsync(request);

        return new ErpUseCaseResult<VoidValue, VoidVerification>(
            result,
            _context,
            (response, ctx) => new VoidVerification(response, ctx));
    }
}
