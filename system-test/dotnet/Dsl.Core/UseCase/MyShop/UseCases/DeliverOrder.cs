using Driver.Port.MyShop;
using Dsl.Core.MyShop.UseCases.Base;
using Driver.Port.MyShop.Dtos.Error;
using Common;
using Dsl.Core.Shared;

namespace Dsl.Core.MyShop.UseCases;

public class DeliverOrder : BaseMyShopUseCase<VoidValue, VoidVerification>
{
    private string? _orderNumberResultAlias;

    public DeliverOrder(IMyShopDriver driver, UseCaseContext context)
        : base(driver, context)
    {
    }

    public DeliverOrder OrderNumber(string? orderNumberResultAlias)
    {
        _orderNumberResultAlias = orderNumberResultAlias;
        return this;
    }

    public override async Task<MyShopUseCaseResult<VoidValue, VoidVerification>> Execute()
    {
        var orderNumber = _context.GetResultValue(_orderNumberResultAlias);
        var result = await _driver.DeliverOrderAsync(orderNumber);

        return new MyShopUseCaseResult<VoidValue, VoidVerification>(
            result,
            _context,
            (response, ctx) => new VoidVerification(response, ctx));
    }
}
