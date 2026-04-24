using Common;
using Driver.Port.MyShop.Dtos;
using Driver.Port.MyShop.Dtos.Error;

namespace Driver.Port.MyShop;

public interface IMyShopDriver : IAsyncDisposable
{
    Task<Result<VoidValue, SystemError>> GoToMyShopAsync();
    Task<Result<PlaceOrderResponse, SystemError>> PlaceOrderAsync(PlaceOrderRequest request);
    Task<Result<VoidValue, SystemError>> CancelOrderAsync(string? orderNumber);
    Task<Result<VoidValue, SystemError>> DeliverOrderAsync(string? orderNumber);
    Task<Result<ViewOrderResponse, SystemError>> ViewOrderAsync(string? orderNumber);
    Task<Result<VoidValue, SystemError>> PublishCouponAsync(PublishCouponRequest request);
    Task<Result<BrowseCouponsResponse, SystemError>> BrowseCouponsAsync();
}

