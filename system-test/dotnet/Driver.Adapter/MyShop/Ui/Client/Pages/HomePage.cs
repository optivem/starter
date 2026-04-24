using Driver.Adapter.Shared.Client.Http;

using Driver.Adapter.Shared.Client.Playwright;



namespace Driver.Adapter.MyShop.Ui.Client.Pages;



public class HomePage : BasePage

{

    private const string MyShopButtonSelector = "a[href='/new-order']";

    private const string OrderHistoryButtonSelector = "a[href='/order-history']";

    private const string CouponManagementButtonSelector = "a[href='/admin-coupons']";



    public HomePage(PageClient pageClient) : base(pageClient)

    {

    }



    public async Task<NewOrderPage> ClickNewOrderAsync()

    {

        await PageClient.ClickAsync(MyShopButtonSelector);

        return new NewOrderPage(PageClient);

    }



    public async Task<OrderHistoryPage> ClickOrderHistoryAsync()

    {

        await PageClient.ClickAsync(OrderHistoryButtonSelector);

        return new OrderHistoryPage(PageClient);

    }



    public async Task<CouponManagementPage> ClickCouponManagementAsync()

    {

        await PageClient.ClickAsync(CouponManagementButtonSelector);

        return new CouponManagementPage(PageClient);

    }

}





