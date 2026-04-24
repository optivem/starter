using Driver.Adapter.Shared.Client.Http;
using Driver.Adapter.MyShop.Api.Client.Dtos.Errors;
using Driver.Adapter.MyShop.Api.Client.Controllers;

namespace Driver.Adapter.MyShop.Api.Client;

public class MyShopApiClient : IDisposable
{
    private readonly JsonHttpClient<ProblemDetailResponse> _httpClient;
    private readonly HealthController _healthController;
    private readonly OrderController _orderController;
    private readonly CouponController _couponController;
    private bool _disposed;

    public MyShopApiClient(string baseUrl)
    {
        _httpClient = new JsonHttpClient<ProblemDetailResponse>(baseUrl);
        _healthController = new HealthController(_httpClient);
        _orderController = new OrderController(_httpClient);
        _couponController = new CouponController(_httpClient);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
            _httpClient?.Dispose();
        _disposed = true;
    }

    public HealthController Health() => _healthController;

    public OrderController Orders() => _orderController;

    public CouponController Coupons() => _couponController;
}
