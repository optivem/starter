using Dsl.Core.Shared;

namespace Dsl.Core.Scenario.Then;

public abstract class BaseThenAnd<TSuccessResponse, TSuccessVerification, TOrderAssertion>
    where TSuccessVerification : ResponseVerification<TSuccessResponse>
    where TOrderAssertion : BaseThenResultOrder<TSuccessResponse, TSuccessVerification, TOrderAssertion>
{
    protected readonly ThenStage<TSuccessResponse, TSuccessVerification> _thenClause;

    protected BaseThenAnd(ThenStage<TSuccessResponse, TSuccessVerification> thenClause)
    {
        _thenClause = thenClause;
    }

    protected abstract TOrderAssertion CreateOrderAssertion(Func<Task<string>> orderNumberFactory);

    public TOrderAssertion Order(string orderNumber)
    {
        return CreateOrderAssertion(() => Task.FromResult(orderNumber));
    }

    public TOrderAssertion Order()
    {
        return CreateOrderAssertion(async () =>
        {
            var result = await _thenClause.GetExecutionResult();
            return result.OrderNumber ?? throw new InvalidOperationException("Cannot verify order: no order number available from the executed operation");
        });
    }
}

