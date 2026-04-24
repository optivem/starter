using Driver.Port.MyShop;
using Driver.Port.MyShop.Dtos;
using Driver.Port.MyShop.Dtos.Error;
using Dsl.Core.Shared;
using Dsl.Core.MyShop.UseCases.Base;

namespace Dsl.Core.MyShop.UseCases;

public class BrowseCoupons : BaseMyShopUseCase<BrowseCouponsResponse, BrowseCouponsVerification>
{
    public BrowseCoupons(IMyShopDriver driver, UseCaseContext context)
        : base(driver, context)
    {
    }

    public override async Task<MyShopUseCaseResult<BrowseCouponsResponse, BrowseCouponsVerification>> Execute()
    {
        var result = await _driver.BrowseCouponsAsync();

        return new MyShopUseCaseResult<BrowseCouponsResponse, BrowseCouponsVerification>(
            result,
            _context,
            (response, ctx) => new BrowseCouponsVerification(response, ctx));
    }
}
