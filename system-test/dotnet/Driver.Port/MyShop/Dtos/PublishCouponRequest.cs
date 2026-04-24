namespace Driver.Port.MyShop.Dtos;

public class PublishCouponRequest
{
    public string? Code { get; set; }
    public string? DiscountRate { get; set; }
    public string? ValidFrom { get; set; }
    public string? ValidTo { get; set; }
    public string? UsageLimit { get; set; }
}
