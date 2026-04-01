using Driver.Port.External.Clock.Dtos;
using Common;
using Dsl.Core.Shared;

namespace Dsl.Core.External.Clock.UseCases.Base;

public class ClockUseCaseResult<TSuccessResponse, TSuccessVerification>
    : UseCaseResult<TSuccessResponse, ClockErrorResponse, TSuccessVerification, ClockErrorVerification>
    where TSuccessVerification : ResponseVerification<TSuccessResponse>
{
    public ClockUseCaseResult(
        Result<TSuccessResponse, ClockErrorResponse> result,
        UseCaseContext context,
        Func<TSuccessResponse, UseCaseContext, TSuccessVerification> verificationFactory)
        : base(result, context, verificationFactory, (error, ctx) => new ClockErrorVerification(error, ctx))
    {
    }
}



