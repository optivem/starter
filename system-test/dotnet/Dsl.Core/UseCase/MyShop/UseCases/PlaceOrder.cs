using Driver.Port.MyShop;
using Dsl.Core.MyShop.UseCases.Base;
using Driver.Port.MyShop.Dtos;
using Driver.Port.MyShop.Dtos.Error;
using Dsl.Core.Shared;

namespace Dsl.Core.MyShop.UseCases;

public class PlaceOrder : BaseMyShopUseCase<PlaceOrderResponse, PlaceOrderVerification>
{
    private string? _orderNumberResultAlias;
    private string? _skuParamAlias;
    private string? _quantity;
    private string? _country;
    private string? _couponCode;

    public PlaceOrder(IMyShopDriver driver, UseCaseContext context)
        : base(driver, context)
    {
    }

    public PlaceOrder OrderNumber(string? orderNumberResultAlias)
    {
        _orderNumberResultAlias = orderNumberResultAlias;
        return this;
    }

    public PlaceOrder Sku(string? skuParamAlias)
    {
        _skuParamAlias = skuParamAlias;
        return this;
    }

    public PlaceOrder Quantity(string? quantity)
    {
        _quantity = quantity;
        return this;
    }

    public PlaceOrder Quantity(int quantity)
    {
        return Quantity(quantity.ToString());
    }

    public PlaceOrder Country(string? country)
    {
        _country = country;
        return this;
    }

    public PlaceOrder CouponCode(string? couponCode)
    {
        _couponCode = couponCode;
        return this;
    }

    public override async Task<MyShopUseCaseResult<PlaceOrderResponse, PlaceOrderVerification>> Execute()
    {
        var sku = _context.GetParamValue(_skuParamAlias);
        var country = _context.GetParamValueOrLiteral(_country);
        var couponCode = _context.GetParamValue(_couponCode);

        var request = new PlaceOrderRequest
        {
            Sku = sku,
            Quantity = _quantity,
            Country = country,
            CouponCode = couponCode,
        };

        var result = await _driver.PlaceOrderAsync(request);

        if (_orderNumberResultAlias != null)
        {
            if (result.IsSuccess)
            {
                var orderNumber = result.Value.OrderNumber;
                _context.SetResultEntry(_orderNumberResultAlias, orderNumber);
            }
            else
            {
                _context.SetResultEntryFailed(_orderNumberResultAlias, result.Error.ToString());
            }
        }

        return new MyShopUseCaseResult<PlaceOrderResponse, PlaceOrderVerification>(
            result,
            _context,
            (response, ctx) => new PlaceOrderVerification(response, ctx));
    }
}



