using Common;

namespace Dsl.Core.Shared;

public class UseCaseResult<TSuccessResponse, TFailureResponse, TSuccessVerification, TFailureVerification>
{
    private readonly Result<TSuccessResponse, TFailureResponse> _result;
    private readonly UseCaseContext _context;
    private readonly Func<TSuccessResponse, UseCaseContext, TSuccessVerification> _successVerificationFactory;
    private readonly Func<TFailureResponse, UseCaseContext, TFailureVerification> _failureVerificationFactory;

    public UseCaseResult(
        Result<TSuccessResponse, TFailureResponse> result,
        UseCaseContext context,
        Func<TSuccessResponse, UseCaseContext, TSuccessVerification> successVerificationFactory,
        Func<TFailureResponse, UseCaseContext, TFailureVerification> failureVerificationFactory)
    {
        _result = result;
        _context = context;
        _successVerificationFactory = successVerificationFactory;
        _failureVerificationFactory = failureVerificationFactory;
    }

    public TSuccessVerification ShouldSucceed()
    {
        _result.ShouldBeSuccess();
        return _successVerificationFactory(_result.Value, _context);
    }

    public TFailureVerification ShouldFail()
    {
        _result.ShouldBeFailure();
        return _failureVerificationFactory(_result.Error, _context);
    }
}
