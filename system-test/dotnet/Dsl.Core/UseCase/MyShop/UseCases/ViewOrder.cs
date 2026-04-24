using Driver.Port.MyShop;
using Dsl.Core.MyShop.UseCases.Base;
using Dsl.Core.Shared;
using Driver.Port.MyShop.Dtos;
using Driver.Port.MyShop.Dtos.Error;

namespace Dsl.Core.MyShop.UseCases;

public class ViewOrder : BaseMyShopUseCase<ViewOrderResponse, ViewOrderVerification>
{
    private string? _orderNumberResultAlias;

    public ViewOrder(IMyShopDriver driver, UseCaseContext context)
        : base(driver, context)
    {
    }

    public ViewOrder OrderNumber(string? orderNumberResultAlias)
    {
        _orderNumberResultAlias = orderNumberResultAlias;
        return this;
    }

    public override async Task<MyShopUseCaseResult<ViewOrderResponse, ViewOrderVerification>> Execute()
    {
        var orderNumber = _context.GetResultValue(_orderNumberResultAlias);

        var result = await _driver.ViewOrderAsync(orderNumber);

        return new MyShopUseCaseResult<ViewOrderResponse, ViewOrderVerification>(
            result,
            _context,
            (response, ctx) => new ViewOrderVerification(response, ctx));
    }
}



