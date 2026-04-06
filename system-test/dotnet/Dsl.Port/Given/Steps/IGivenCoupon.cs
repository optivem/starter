using Dsl.Port.Given.Steps.Base;

namespace Dsl.Port.Given.Steps;

public interface IGivenCoupon : IGivenStep
{
    IGivenCoupon WithCouponCode(string? couponCode);

    IGivenCoupon WithDiscountRate(string? discountRate);

    IGivenCoupon WithDiscountRate(decimal? discountRate);

    IGivenCoupon WithValidFrom(string? validFrom);

    IGivenCoupon WithValidTo(string? validTo);

    IGivenCoupon WithUsageLimit(string? usageLimit);

    IGivenCoupon WithUsageLimit(int? usageLimit);
}
