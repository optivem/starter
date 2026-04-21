using Driver.Port.External.Erp;
using Driver.Port.External.Erp.Dtos.Error;
using Dsl.Core.Shared;

namespace Dsl.Core.External.Erp.UseCases.Base;

public abstract class BaseErpUseCase<TResponse, TVerification>
    where TVerification : ResponseVerification<TResponse>
{
    protected readonly IErpDriver _driver;
    protected readonly UseCaseContext _context;

    protected BaseErpUseCase(IErpDriver driver, UseCaseContext context)
    {
        _driver = driver;
        _context = context;
    }

    public abstract Task<ErpUseCaseResult<TResponse, TVerification>> Execute();
}



