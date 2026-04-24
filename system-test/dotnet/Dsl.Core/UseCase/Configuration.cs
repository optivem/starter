using Dsl.Port;
using Dsl.Core.Shared;

namespace Dsl.Core;

public class Configuration
{
    private readonly string shopUiBaseUrl;
    private readonly string shopApiBaseUrl;
    private readonly string erpBaseUrl;
    private readonly string taxBaseUrl;
    private readonly string clockBaseUrl;
    private readonly ExternalSystemMode externalSystemMode;
    private readonly ChannelMode channelMode;

    public Configuration(string shopUiBaseUrl, string shopApiBaseUrl, string erpBaseUrl, string taxBaseUrl,
        string clockBaseUrl, ExternalSystemMode externalSystemMode, ChannelMode channelMode = ChannelMode.Dynamic)
    {
        this.shopUiBaseUrl = shopUiBaseUrl;
        this.shopApiBaseUrl = shopApiBaseUrl;
        this.erpBaseUrl = erpBaseUrl;
        this.taxBaseUrl = taxBaseUrl;
        this.clockBaseUrl = clockBaseUrl;
        this.externalSystemMode = externalSystemMode;
        this.channelMode = channelMode;
    }

    public string MyShopUiBaseUrl => shopUiBaseUrl;
    public string MyShopApiBaseUrl => shopApiBaseUrl;
    public string ErpBaseUrl => erpBaseUrl;
    public string TaxBaseUrl => taxBaseUrl;
    public string ClockBaseUrl => clockBaseUrl;
    public ExternalSystemMode ExternalSystemMode => externalSystemMode;
    public ChannelMode ChannelMode => channelMode;
}
