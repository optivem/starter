using Driver.Adapter.Shared.Client.Playwright;

using Driver.Port.Shop.Dtos;



namespace Driver.Adapter.Shop.Ui.Client.Pages;



public class OrderDetailsPage : BasePage

{

    private const string OrderNumberOutputSelector = "[aria-label='Display Order Number']";

    private const string OrderTimestampOutputSelector = "[aria-label='Display Order Timestamp']";

    private const string SkuOutputSelector = "[aria-label='Display SKU']";

    private const string QuantityOutputSelector = "[aria-label='Display Quantity']";

    private const string UnitPriceOutputSelector = "[aria-label='Display Unit Price']";

    private const string TotalPriceOutputSelector = "[aria-label='Display Total Price']";

    private const string StatusOutputSelector = "[aria-label='Display Status']";



    // Display text constants

    private const string DollarSymbol = "$";



    // Enum parsing constants

    private const bool IgnoreCase = true;



    public OrderDetailsPage(PageClient pageClient) : base(pageClient)

    {

    }



    public async Task<bool> IsLoadedSuccessfullyAsync()

    {

        return await PageClient.IsVisibleAsync(OrderNumberOutputSelector);

    }



    public async Task<string> GetOrderNumberAsync()

    {

        return await PageClient.ReadTextContentAsync(OrderNumberOutputSelector);

    }



    public async Task<DateTimeOffset> GetOrderTimestampAsync()

    {

        var textContent = await PageClient.ReadTextContentAsync(OrderTimestampOutputSelector);

        return DateTimeOffset.Parse(textContent, System.Globalization.CultureInfo.InvariantCulture);

    }



    public async Task<string> GetSkuAsync()

    {

        return await PageClient.ReadTextContentAsync(SkuOutputSelector);

    }



    public async Task<int> GetQuantityAsync()

    {

        var textContent = await PageClient.ReadTextContentAsync(QuantityOutputSelector);

        return int.Parse(textContent);

    }



    public async Task<decimal> GetUnitPriceAsync()

    {

        return await ReadTextMoneyAsync(UnitPriceOutputSelector);

    }



    public async Task<decimal> GetTotalPriceAsync()

    {

        return await ReadTextMoneyAsync(TotalPriceOutputSelector);

    }



    public async Task<OrderStatus> GetStatusAsync()

    {

        var status = await PageClient.ReadTextContentAsync(StatusOutputSelector);

        return Enum.Parse<OrderStatus>(status, IgnoreCase);

    }



    private async Task<decimal> ReadTextMoneyAsync(string selector)

    {

        var textContent = await PageClient.ReadTextContentAsync(selector);

        var cleaned = textContent.Replace(DollarSymbol, "").Trim();

        return decimal.Parse(cleaned);

    }



}







