using Driver.Port.External.Clock;
using Driver.Port.External.Clock.Dtos;
using Dsl.Core.Shared;

namespace Dsl.Core.External.Clock.UseCases.Base;

public abstract class BaseClockCommand<TResponse, TVerification>
    where TVerification : ResponseVerification<TResponse>
{
    protected readonly IClockDriver _driver;
    protected readonly UseCaseContext _context;

    protected BaseClockCommand(IClockDriver driver, UseCaseContext context)
    {
        _driver = driver;
        _context = context;
    }

    public abstract Task<ClockUseCaseResult<TResponse, TVerification>> Execute();
}



