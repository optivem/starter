using Dsl.Core.External.Clock.UseCases.Base;
using Driver.Port.External.Clock;
using Driver.Port.External.Clock.Dtos;
using Common;
using Dsl.Core.Shared;

namespace Dsl.Core.External.Clock.UseCases;

public class GoToClock : BaseClockCommand<VoidValue, VoidVerification>
{
    public GoToClock(IClockDriver driver, UseCaseContext context)
        : base(driver, context)
    {
    }

    public override async Task<ClockUseCaseResult<VoidValue, VoidVerification>> Execute()
    {
        var result = await _driver.GoToClockAsync();

        return new ClockUseCaseResult<VoidValue, VoidVerification>(
            result,
            _context,
            (response, ctx) => new VoidVerification(response, ctx));
    }
}



