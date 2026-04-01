using Dsl.Core.External.Clock.UseCases.Base;
using Driver.Port.External.Clock;
using Driver.Port.External.Clock.Dtos;
using Common;
using Dsl.Core.Shared;

namespace Dsl.Core.External.Clock.UseCases;

public class ReturnsTime : BaseClockCommand<VoidValue, VoidVerification>
{
    private string? _time;

    public ReturnsTime(IClockDriver driver, UseCaseContext context)
        : base(driver, context)
    {
    }

    public ReturnsTime Time(string? time)
    {
        _time = time;
        return this;
    }

    public override async Task<ClockUseCaseResult<VoidValue, VoidVerification>> Execute()
    {
        var request = new ReturnsTimeRequest
        {
            Time = _time
        };

        var result = await _driver.ReturnsTimeAsync(request);

        return new ClockUseCaseResult<VoidValue, VoidVerification>(
            result,
            _context,
            (response, ctx) => new VoidVerification(response, ctx));
    }
}



