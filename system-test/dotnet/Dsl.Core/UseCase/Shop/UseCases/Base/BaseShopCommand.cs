using Driver.Port.Shop;
using Dsl.Core.Shared;
using Driver.Port.Shop.Dtos.Error;

namespace Dsl.Core.Shop.UseCases.Base;

public abstract class BaseShopCommand<TResponse, TVerification>
    where TVerification : ResponseVerification<TResponse>
{
    protected readonly IShopDriver _driver;
    protected readonly UseCaseContext _context;

    protected BaseShopCommand(IShopDriver driver, UseCaseContext context)
    {
        _driver = driver;
        _context = context;
    }

    public abstract Task<ShopUseCaseResult<TResponse, TVerification>> Execute();
}



