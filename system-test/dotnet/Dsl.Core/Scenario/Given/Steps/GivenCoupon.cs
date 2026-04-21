using Common;
using Dsl.Port.Given.Steps;
using Dsl.Core.Scenario.Given;
using static Dsl.Core.Scenario.ScenarioDefaults;

namespace Dsl.Core.Scenario.Given;

public class GivenCoupon : BaseGiven, IGivenCoupon
{
    private string? _couponCode;
    private string? _discountRate;
    private string? _validFrom;
    private string? _validTo;
    private string? _usageLimit;

    public GivenCoupon(GivenStage givenClause) : base(givenClause)
    {
        WithCouponCode(DefaultCouponCode);
        WithDiscountRate(DefaultDiscountRate);
        WithValidFrom(Empty);
        WithValidTo(Empty);
        WithUsageLimit(Empty);
    }

    public GivenCoupon WithCouponCode(string? couponCode)
    {
        _couponCode = couponCode;
        return this;
    }

    IGivenCoupon IGivenCoupon.WithCouponCode(string? couponCode) => WithCouponCode(couponCode);

    public GivenCoupon WithDiscountRate(string? discountRate)
    {
        _discountRate = discountRate;
        return this;
    }

    IGivenCoupon IGivenCoupon.WithDiscountRate(string? discountRate) => WithDiscountRate(discountRate);

    public GivenCoupon WithDiscountRate(decimal? discountRate)
    {
        _discountRate = Converter.FromDecimal(discountRate);
        return this;
    }

    IGivenCoupon IGivenCoupon.WithDiscountRate(decimal? discountRate) => WithDiscountRate(discountRate);

    public GivenCoupon WithValidFrom(string? validFrom)
    {
        _validFrom = validFrom;
        return this;
    }

    IGivenCoupon IGivenCoupon.WithValidFrom(string? validFrom) => WithValidFrom(validFrom);

    public GivenCoupon WithValidTo(string? validTo)
    {
        _validTo = validTo;
        return this;
    }

    IGivenCoupon IGivenCoupon.WithValidTo(string? validTo) => WithValidTo(validTo);

    public GivenCoupon WithUsageLimit(string? usageLimit)
    {
        _usageLimit = usageLimit;
        return this;
    }

    IGivenCoupon IGivenCoupon.WithUsageLimit(string? usageLimit) => WithUsageLimit(usageLimit);

    public GivenCoupon WithUsageLimit(int? usageLimit)
    {
        return WithUsageLimit(Converter.FromInteger(usageLimit));
    }

    IGivenCoupon IGivenCoupon.WithUsageLimit(int? usageLimit) => WithUsageLimit(usageLimit);

    internal override async Task Execute(UseCaseDsl app)
    {
        var shop = await app.Shop(Channel);
        (await shop.PublishCoupon()
            .CouponCode(_couponCode)
            .DiscountRate(_discountRate)
            .ValidFrom(_validFrom)
            .ValidTo(_validTo)
            .UsageLimit(_usageLimit)
            .Execute())
            .ShouldSucceed();
    }
}
