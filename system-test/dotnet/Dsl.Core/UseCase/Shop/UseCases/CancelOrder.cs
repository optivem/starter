using Driver.Port.Shop;
using Dsl.Core.Shop.UseCases.Base;
using Driver.Port.Shop.Dtos.Error;
using Common;
using Dsl.Core.Shared;

namespace Dsl.Core.Shop.UseCases;

public class CancelOrder : BaseShopCommand<VoidValue, VoidVerification>
{
    private string? _orderNumberResultAlias;

    public CancelOrder(IShopDriver driver, UseCaseContext context)
        : base(driver, context)
    {
    }

    public CancelOrder OrderNumber(string? orderNumberResultAlias)
    {
        _orderNumberResultAlias = orderNumberResultAlias;
        return this;
    }

    public override async Task<ShopUseCaseResult<VoidValue, VoidVerification>> Execute()
    {
        var orderNumber = _context.GetResultValue(_orderNumberResultAlias);
        var result = await _driver.CancelOrderAsync(orderNumber);

        return new ShopUseCaseResult<VoidValue, VoidVerification>(
            result,
            _context,
            (response, ctx) => new VoidVerification(response, ctx));
    }
}
