using System.Diagnostics.CodeAnalysis;

namespace Dsl.Core.Shared;

[SuppressMessage("Major Code Smell", "S2436:Reduce the number of generic parameters",
    Justification = "Multiple generic parameters are required for DSL pattern to provide type safety across driver, response, and verification layers")]
public abstract class BaseUseCase<TDriver, TSuccessResponse, TFailureResponse, TSuccessVerification, TFailureVerification>
    : IUseCase<Task<UseCaseResult<TSuccessResponse, TFailureResponse, TSuccessVerification, TFailureVerification>>>
{
    protected readonly TDriver _driver;
    protected readonly UseCaseContext _context;

    protected BaseUseCase(TDriver driver, UseCaseContext context)
    {
        _driver = driver;
        _context = context;
    }

    public abstract Task<UseCaseResult<TSuccessResponse, TFailureResponse, TSuccessVerification, TFailureVerification>> Execute();
}
