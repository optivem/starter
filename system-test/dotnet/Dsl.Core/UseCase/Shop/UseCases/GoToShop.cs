using Driver.Port.Shop;
using Dsl.Core.Shop.UseCases.Base;
using Driver.Port.Shop.Dtos.Error;
using Common;
using Dsl.Core.Shared;

namespace Dsl.Core.Shop.UseCases;

public class GoToShop : BaseShopCommand<VoidValue, VoidVerification>
{
    public GoToShop(IShopDriver driver, UseCaseContext context)
        : base(driver, context)
    {
    }

    public override async Task<ShopUseCaseResult<VoidValue, VoidVerification>> Execute()
    {
        var result = await _driver.GoToShopAsync();

        return new ShopUseCaseResult<VoidValue, VoidVerification>(
            result,
            _context,
            (response, ctx) => new VoidVerification(response, ctx));
    }
}



