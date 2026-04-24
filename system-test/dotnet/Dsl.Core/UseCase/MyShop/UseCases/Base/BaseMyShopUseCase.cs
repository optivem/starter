using Driver.Port.MyShop;
using Dsl.Core.Shared;
using Driver.Port.MyShop.Dtos.Error;

namespace Dsl.Core.MyShop.UseCases.Base;

public abstract class BaseMyShopUseCase<TResponse, TVerification>
    where TVerification : ResponseVerification<TResponse>
{
    protected readonly IMyShopDriver _driver;
    protected readonly UseCaseContext _context;

    protected BaseMyShopUseCase(IMyShopDriver driver, UseCaseContext context)
    {
        _driver = driver;
        _context = context;
    }

    public abstract Task<MyShopUseCaseResult<TResponse, TVerification>> Execute();
}



