using Dsl.Port.When.Steps.Base;

namespace Dsl.Port.When.Steps;

public interface IPublishCoupon : IWhenStep
{
    IPublishCoupon WithCouponCode(string? couponCode);

    IPublishCoupon WithDiscountRate(string? discountRate);

    IPublishCoupon WithDiscountRate(decimal discountRate);

    IPublishCoupon WithValidFrom(string? validFrom);

    IPublishCoupon WithValidTo(string? validTo);

    IPublishCoupon WithUsageLimit(string? usageLimit);

    IPublishCoupon WithUsageLimit(int usageLimit);
}
