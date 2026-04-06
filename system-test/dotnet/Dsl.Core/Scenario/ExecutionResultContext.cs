namespace Dsl.Core.Scenario;

/// <summary>
/// Context extracted from execution result - order number and coupon code from the executed operation.
/// </summary>
public class ExecutionResultContext
{
    private static readonly ExecutionResultContext _empty = new(null, null);

    public ExecutionResultContext(string? orderNumber, string? couponCode)
    {
        OrderNumber = orderNumber;
        CouponCode = couponCode;
    }

    public static ExecutionResultContext Empty() => _empty;

    public string? OrderNumber { get; }
    public string? CouponCode { get; }
}
