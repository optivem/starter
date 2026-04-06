using Dsl.Core.Scenario;
using Dsl.Core.Scenario.When.Steps.Base;
using Dsl.Port.When.Steps;
using Dsl.Core.Shared;
using Common;
using Driver.Port.Shop.Dtos;
using Dsl.Core.Shop.UseCases;
using Optivem.Testing;
using static Dsl.Core.Gherkin.GherkinDefaults;

namespace Dsl.Core.Scenario.When.Steps;

public class WhenPublishCoupon : BaseWhen<VoidValue, VoidVerification>, IPublishCoupon
{
    private string? _couponCode;
    private string? _discountRate;
    private string? _validFrom;
    private string? _validTo;
    private string? _usageLimit;

    public WhenPublishCoupon(UseCaseDsl app, ScenarioDsl scenario, Func<Task> ensureGiven) : base(app, scenario, ensureGiven)
    {
        WithCouponCode(DefaultCouponCode);
        WithDiscountRate(DefaultDiscountRate);
    }

    public WhenPublishCoupon WithCouponCode(string? couponCode)
    {
        _couponCode = couponCode;
        return this;
    }

    IPublishCoupon IPublishCoupon.WithCouponCode(string? couponCode) => WithCouponCode(couponCode);

    public WhenPublishCoupon WithDiscountRate(string? discountRate)
    {
        _discountRate = discountRate;
        return this;
    }

    IPublishCoupon IPublishCoupon.WithDiscountRate(string? discountRate) => WithDiscountRate(discountRate);

    public WhenPublishCoupon WithDiscountRate(decimal discountRate)
    {
        _discountRate = Converter.FromDecimal(discountRate);
        return this;
    }

    IPublishCoupon IPublishCoupon.WithDiscountRate(decimal discountRate) => WithDiscountRate(discountRate);

    public WhenPublishCoupon WithValidFrom(string? validFrom)
    {
        _validFrom = validFrom;
        return this;
    }

    IPublishCoupon IPublishCoupon.WithValidFrom(string? validFrom) => WithValidFrom(validFrom);

    public WhenPublishCoupon WithValidTo(string? validTo)
    {
        _validTo = validTo;
        return this;
    }

    IPublishCoupon IPublishCoupon.WithValidTo(string? validTo) => WithValidTo(validTo);

    public WhenPublishCoupon WithUsageLimit(string? usageLimit)
    {
        _usageLimit = usageLimit;
        return this;
    }

    IPublishCoupon IPublishCoupon.WithUsageLimit(string? usageLimit) => WithUsageLimit(usageLimit);

    public WhenPublishCoupon WithUsageLimit(int usageLimit)
    {
        _usageLimit = Converter.FromInteger(usageLimit);
        return this;
    }

    IPublishCoupon IPublishCoupon.WithUsageLimit(int usageLimit) => WithUsageLimit(usageLimit);

    protected override async Task<ExecutionResult<VoidValue, VoidVerification>> Execute(UseCaseDsl app)
    {
        var shop = await app.Shop(Channel);
        var result = await shop.PublishCoupon()
            .CouponCode(_couponCode)
            .DiscountRate(_discountRate)
            .ValidFrom(_validFrom)
            .ValidTo(_validTo)
            .UsageLimit(_usageLimit)
            .Execute();

        return new ExecutionResultBuilder<VoidValue, VoidVerification>(result)
            .CouponCode(_couponCode)
            .Build();
    }
}
