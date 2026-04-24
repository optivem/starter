using Driver.Port.MyShop;
using Dsl.Core.MyShop.UseCases.Base;
using Driver.Port.MyShop.Dtos;
using Driver.Port.MyShop.Dtos.Error;
using Dsl.Core.Shared;
using Common;

namespace Dsl.Core.MyShop.UseCases;

public class PublishCoupon : BaseMyShopUseCase<VoidValue, VoidVerification>
{
    private string? _couponCodeParamAlias;
    private string? _discountRate;
    private string? _validFrom;
    private string? _validTo;
    private string? _usageLimit;

    public PublishCoupon(IMyShopDriver driver, UseCaseContext context)
        : base(driver, context)
    {
    }

    public PublishCoupon CouponCode(string? couponCodeParamAlias)
    {
        _couponCodeParamAlias = couponCodeParamAlias;
        return this;
    }

    public PublishCoupon DiscountRate(decimal discountRate)
    {
        _discountRate = discountRate.ToString();
        return this;
    }

    public PublishCoupon DiscountRate(string? discountRate)
    {
        _discountRate = discountRate;
        return this;
    }

    public PublishCoupon ValidFrom(string? validFrom)
    {
        _validFrom = validFrom;
        return this;
    }

    public PublishCoupon ValidTo(string? validTo)
    {
        _validTo = validTo;
        return this;
    }

    public PublishCoupon UsageLimit(string? usageLimit)
    {
        _usageLimit = usageLimit;
        return this;
    }

    public override async Task<MyShopUseCaseResult<VoidValue, VoidVerification>> Execute()
    {
        var couponCode = _context.GetParamValue(_couponCodeParamAlias);

        var request = new PublishCouponRequest
        {
            Code = couponCode,
            DiscountRate = string.IsNullOrWhiteSpace(_discountRate) ? null : _discountRate,
            ValidFrom = string.IsNullOrWhiteSpace(_validFrom) ? null : _validFrom,
            ValidTo = string.IsNullOrWhiteSpace(_validTo) ? null : _validTo,
            UsageLimit = string.IsNullOrWhiteSpace(_usageLimit) ? null : _usageLimit
        };

        var result = await _driver.PublishCouponAsync(request);

        return new MyShopUseCaseResult<VoidValue, VoidVerification>(
            result,
            _context,
            (response, ctx) => new VoidVerification(response, ctx));
    }
}
