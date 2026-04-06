using System.Runtime.CompilerServices;

namespace Dsl.Port.Then.Steps;

public interface IThenCoupon
{
    IThenCoupon HasDiscountRate(decimal discountRate);

    IThenCoupon IsValidFrom(string validFrom);

    IThenCoupon IsValidTo(string validTo);

    IThenCoupon HasUsageLimit(int usageLimit);

    IThenCoupon HasUsedCount(int expectedUsedCount);

    TaskAwaiter GetAwaiter();
}
