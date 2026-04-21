using Driver.Port.Shop;
using Dsl.Core.Shop.UseCases.Base;
using Driver.Port.Shop.Dtos.Error;
using Common;
using Dsl.Core.Shared;

namespace Dsl.Core.Shop.UseCases;

public class DeliverOrder : BaseShopUseCase<VoidValue, VoidVerification>
{
    private string? _orderNumberResultAlias;

    public DeliverOrder(IShopDriver driver, UseCaseContext context)
        : base(driver, context)
    {
    }

    public DeliverOrder OrderNumber(string? orderNumberResultAlias)
    {
        _orderNumberResultAlias = orderNumberResultAlias;
        return this;
    }

    public override async Task<ShopUseCaseResult<VoidValue, VoidVerification>> Execute()
    {
        var orderNumber = _context.GetResultValue(_orderNumberResultAlias);
        var result = await _driver.DeliverOrderAsync(orderNumber);

        return new ShopUseCaseResult<VoidValue, VoidVerification>(
            result,
            _context,
            (response, ctx) => new VoidVerification(response, ctx));
    }
}
