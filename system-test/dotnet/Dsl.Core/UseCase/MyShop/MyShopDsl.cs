using Driver.Port.MyShop;
using Dsl.Core.MyShop.UseCases;
using Dsl.Core.Shared;

namespace Dsl.Core.MyShop;

public class MyShopDsl : IAsyncDisposable
{
    private readonly IMyShopDriver _driver;
    private readonly UseCaseContext _context;

    private MyShopDsl(IMyShopDriver driver, UseCaseContext context)
    {
        _driver = driver;
        _context = context;
    }

    public static Task<MyShopDsl> CreateAsync(IMyShopDriver driver, UseCaseContext context)
    {
        return Task.FromResult(new MyShopDsl(driver, context));
    }

    public async ValueTask DisposeAsync()
    {
        if (_driver != null)
            await _driver.DisposeAsync();
    }

    public GoToMyShop GoToMyShop() => new(_driver, _context);

    public PlaceOrder PlaceOrder() => new(_driver, _context);

    public CancelOrder CancelOrder() => new(_driver, _context);

    public DeliverOrder DeliverOrder() => new(_driver, _context);

    public ViewOrder ViewOrder() => new(_driver, _context);

    public PublishCoupon PublishCoupon() => new(_driver, _context);

    public BrowseCoupons BrowseCoupons() => new(_driver, _context);
}



