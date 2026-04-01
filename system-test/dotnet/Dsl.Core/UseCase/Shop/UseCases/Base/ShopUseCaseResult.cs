using Common;
using Dsl.Core.Shared;
using Driver.Port.Shop.Dtos.Error;

namespace Dsl.Core.Shop.UseCases.Base;

public class ShopUseCaseResult<TSuccessResponse, TSuccessVerification>
    : UseCaseResult<TSuccessResponse, SystemError, TSuccessVerification, SystemErrorFailureVerification>
    where TSuccessVerification : ResponseVerification<TSuccessResponse>
{
    public ShopUseCaseResult(
        Result<TSuccessResponse, SystemError> result,
        UseCaseContext context,
        Func<TSuccessResponse, UseCaseContext, TSuccessVerification> verificationFactory)
        : base(result, context, verificationFactory, (error, ctx) => new SystemErrorFailureVerification(error, ctx))
    {
    }
}



