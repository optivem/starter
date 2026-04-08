namespace Dsl.Port.Then.Steps;

public interface IThenFailureAnd
{
    IThenOrder Order(string orderNumber);

    IThenOrder Order();

    Task<IThenClock> Clock();

    Task<IThenProduct> Product(string skuAlias);

    IThenCoupon Coupon(string couponCode);

    IThenCoupon Coupon();
}
