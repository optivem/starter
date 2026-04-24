using Dsl.Port.Then.Steps;
using Dsl.Core.Shared;
using Dsl.Core.Scenario;
using Dsl.Core.MyShop.UseCases.Base;

namespace Dsl.Core.Scenario.Then;

public class ThenFailureCoupon<TSuccessResponse, TSuccessVerification>
    : BaseThenResultCoupon<TSuccessResponse, TSuccessVerification, ThenFailureCoupon<TSuccessResponse, TSuccessVerification>>, IThenCoupon
    where TSuccessVerification : ResponseVerification<TSuccessResponse>
{
    private readonly List<Action<SystemErrorFailureVerification>> _failureAssertions;

    internal ThenFailureCoupon(
        ThenStage<TSuccessResponse, TSuccessVerification> thenClause,
        List<Action<SystemErrorFailureVerification>> failureAssertions,
        Func<Task<string>> couponCodeFactory)
        : base(thenClause, couponCodeFactory)
    {
        _failureAssertions = failureAssertions;
    }

    protected override Task RunPrelude(ExecutionResult<TSuccessResponse, TSuccessVerification> result)
    {
        if (result.Result == null)
            throw new InvalidOperationException("Cannot verify failure: no operation was executed");
        var failureVerification = result.Result.ShouldFail();
        foreach (var assertion in _failureAssertions)
        {
            assertion(failureVerification);
        }
        return Task.CompletedTask;
    }
}
