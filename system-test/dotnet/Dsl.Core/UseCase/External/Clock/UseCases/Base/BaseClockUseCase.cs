using Driver.Port.External.Clock;
using Driver.Port.External.Clock.Dtos;
using Dsl.Core.Shared;

namespace Dsl.Core.External.Clock.UseCases.Base;

public abstract class BaseClockUseCase<TResponse, TVerification>
    where TVerification : ResponseVerification<TResponse>
{
    protected readonly IClockDriver _driver;
    protected readonly UseCaseContext _context;

    protected BaseClockUseCase(IClockDriver driver, UseCaseContext context)
    {
        _driver = driver;
        _context = context;
    }

    public abstract Task<ClockUseCaseResult<TResponse, TVerification>> Execute();
}



