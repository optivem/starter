using Dsl.Port.Then.Steps;
using Dsl.Core.Shared;
using Dsl.Core.MyShop.UseCases.Base;

namespace Dsl.Core.Scenario.Then;

public class ThenFailureAnd<TSuccessResponse, TSuccessVerification>
    : BaseThenAnd<TSuccessResponse, TSuccessVerification, ThenFailureOrder<TSuccessResponse, TSuccessVerification>>, IThenFailureAnd
    where TSuccessVerification : ResponseVerification<TSuccessResponse>
{
    private readonly List<Action<SystemErrorFailureVerification>> _failureAssertions;

    internal ThenFailureAnd(
        ThenStage<TSuccessResponse, TSuccessVerification> thenClause,
        List<Action<SystemErrorFailureVerification>> failureAssertions)
        : base(thenClause)
    {
        _failureAssertions = failureAssertions;
    }

    protected override ThenFailureOrder<TSuccessResponse, TSuccessVerification> CreateOrderAssertion(Func<Task<string>> orderNumberFactory)
    {
        return new ThenFailureOrder<TSuccessResponse, TSuccessVerification>(_thenClause, _failureAssertions, orderNumberFactory);
    }

    IThenOrder IThenFailureAnd.Order(string orderNumber) => Order(orderNumber);

    IThenOrder IThenFailureAnd.Order() => Order();

    public async Task<IThenClock> Clock()
    {
        await _thenClause.GetExecutionResult();
        var verification = (await _thenClause.App.Clock().GetTime().Execute()).ShouldSucceed();
        return new Steps.ThenClock(_thenClause.App, verification);
    }

    async Task<IThenClock> IThenFailureAnd.Clock() => await Clock();

    public async Task<IThenProduct> Product(string skuAlias)
    {
        await _thenClause.GetExecutionResult();
        var verification = (await _thenClause.App.Erp().GetProduct().Sku(skuAlias).Execute()).ShouldSucceed();
        return new Steps.ThenProduct(_thenClause.App, verification);
    }

    async Task<IThenProduct> IThenFailureAnd.Product(string skuAlias) => await Product(skuAlias);

    public async Task<IThenCountry> Country(string countryAlias)
    {
        var verification = (await _thenClause.App.Tax().GetTaxRate().Country(countryAlias).Execute()).ShouldSucceed();
        return new Steps.ThenCountry(_thenClause.App, verification);
    }

    async Task<IThenCountry> IThenFailureAnd.Country(string countryAlias) => await Country(countryAlias);

    public ThenFailureCoupon<TSuccessResponse, TSuccessVerification> Coupon(string couponCode)
    {
        return new ThenFailureCoupon<TSuccessResponse, TSuccessVerification>(
            _thenClause, _failureAssertions, () => Task.FromResult(couponCode));
    }

    public ThenFailureCoupon<TSuccessResponse, TSuccessVerification> Coupon()
    {
        return new ThenFailureCoupon<TSuccessResponse, TSuccessVerification>(
            _thenClause, _failureAssertions,
            async () =>
            {
                var result = await _thenClause.GetExecutionResult();
                return result.CouponCode ?? throw new InvalidOperationException("Cannot verify coupon: no coupon code available from the executed operation");
            });
    }

    IThenCoupon IThenFailureAnd.Coupon(string couponCode) => Coupon(couponCode);

    IThenCoupon IThenFailureAnd.Coupon() => Coupon();
}


