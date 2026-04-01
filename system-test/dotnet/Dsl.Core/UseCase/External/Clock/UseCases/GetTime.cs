using Dsl.Core.External.Clock.UseCases.Base;
using Driver.Port.External.Clock;
using Driver.Port.External.Clock.Dtos;
using Dsl.Core.Shared;

namespace Dsl.Core.External.Clock.UseCases;

public class GetTime : BaseClockCommand<GetTimeResponse, GetTimeVerification>
{
    public GetTime(IClockDriver driver, UseCaseContext context)
        : base(driver, context)
    {
    }

    public override async Task<ClockUseCaseResult<GetTimeResponse, GetTimeVerification>> Execute()
    {
        var result = await _driver.GetTimeAsync();

        return new ClockUseCaseResult<GetTimeResponse, GetTimeVerification>(
            result,
            _context,
            (response, ctx) => new GetTimeVerification(response, ctx));
    }
}



