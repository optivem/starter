using Driver.Port.External.Clock.Dtos;
using Dsl.Core.Shared;

namespace Dsl.Core.External.Clock.UseCases.Base;

public class ClockErrorVerification : ResponseVerification<ClockErrorResponse>
{
    public ClockErrorVerification(ClockErrorResponse error, UseCaseContext context)
        : base(error, context)
    {
    }
}



