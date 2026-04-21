using Driver.Port.External.Tax;
using Driver.Port.External.Tax.Dtos.Error;
using Dsl.Core.Shared;

namespace Dsl.Core.External.Tax.UseCases.Base;

public abstract class BaseTaxUseCase<TResponse, TVerification>
    where TVerification : ResponseVerification<TResponse>
{
    protected readonly ITaxDriver _driver;
    protected readonly UseCaseContext _context;

    protected BaseTaxUseCase(ITaxDriver driver, UseCaseContext context)
    {
        _driver = driver;
        _context = context;
    }

    public abstract Task<TaxUseCaseResult<TResponse, TVerification>> Execute();
}
