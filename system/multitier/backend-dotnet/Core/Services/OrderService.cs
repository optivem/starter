using Microsoft.EntityFrameworkCore;
using Optivem.Shop.Backend.Core.Dtos;
using Optivem.Shop.Backend.Core.Entities;
using Optivem.Shop.Backend.Core.Exceptions;
using Optivem.Shop.Backend.Core.Services.External;
using Optivem.Shop.Backend.Data;

namespace Optivem.Shop.Backend.Core.Services;

public class OrderService
{
    private static readonly int RestrictedMonth = 12;
    private static readonly int RestrictedDay = 31;
    private static readonly TimeOnly RestrictedTimeStart = new(23, 59);

    private readonly AppDbContext _dbContext;
    private readonly ErpGateway _erpGateway;
    private readonly ClockGateway _clockGateway;

    public OrderService(AppDbContext dbContext, ErpGateway erpGateway, ClockGateway clockGateway)
    {
        _dbContext = dbContext;
        _erpGateway = erpGateway;
        _clockGateway = clockGateway;
    }

    public async Task<PlaceOrderResponse> PlaceOrderAsync(PlaceOrderRequest request)
    {
        var sku = request.Sku!;
        var quantity = request.Quantity!.Value;

        var orderTimestamp = await _clockGateway.GetCurrentTimeAsync();

        var utcTime = orderTimestamp.ToUniversalTime();

        if (utcTime.Month == RestrictedMonth && utcTime.Day == RestrictedDay)
        {
            var currentTime = TimeOnly.FromDateTime(utcTime);
            if (currentTime >= RestrictedTimeStart)
            {
                throw new ValidationException("Orders cannot be placed between 23:59 and 00:00 on December 31st");
            }
        }

        var unitPrice = await GetUnitPriceAsync(sku);
        var promotion = await _erpGateway.GetPromotionDetailsAsync();
        var discountFactor = promotion.PromotionActive ? promotion.Discount : 1.0m;
        var totalPrice = unitPrice * quantity * discountFactor;

        var orderNumber = GenerateOrderNumber();

        var order = new Order
        {
            OrderNumber = orderNumber,
            OrderTimestamp = orderTimestamp,
            Sku = sku,
            Quantity = quantity,
            UnitPrice = unitPrice,
            TotalPrice = totalPrice,
            Status = OrderStatus.PLACED
        };

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();

        return new PlaceOrderResponse { OrderNumber = orderNumber };
    }

    private async Task<decimal> GetUnitPriceAsync(string sku)
    {
        var productDetails = await _erpGateway.GetProductDetailsAsync(sku);
        if (productDetails == null)
        {
            throw new ValidationException("sku", $"Product does not exist for SKU: {sku}");
        }

        return productDetails.Price;
    }

    public async Task<BrowseOrderHistoryResponse> BrowseOrderHistoryAsync(string? orderNumberFilter)
    {
        List<Order> orders;

        if (string.IsNullOrWhiteSpace(orderNumberFilter))
        {
            orders = await _dbContext.Orders
                .OrderByDescending(o => o.OrderTimestamp)
                .ToListAsync();
        }
        else
        {
            var filter = orderNumberFilter.Trim();
            orders = await _dbContext.Orders
                .Where(o => EF.Functions.ILike(o.OrderNumber, $"%{filter}%"))
                .OrderByDescending(o => o.OrderTimestamp)
                .ToListAsync();
        }

        var items = orders.Select(order => new BrowseOrderHistoryItemResponse
        {
            OrderNumber = order.OrderNumber,
            OrderTimestamp = order.OrderTimestamp,
            Sku = order.Sku,
            Quantity = order.Quantity,
            TotalPrice = order.TotalPrice,
            Status = order.Status
        }).ToList();

        return new BrowseOrderHistoryResponse { Orders = items };
    }

    public async Task<ViewOrderDetailsResponse> GetOrderAsync(string orderNumber)
    {
        var order = await _dbContext.Orders
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);

        if (order == null)
        {
            throw new NotExistValidationException($"Order {orderNumber} does not exist.");
        }

        return new ViewOrderDetailsResponse
        {
            OrderNumber = order.OrderNumber,
            OrderTimestamp = order.OrderTimestamp,
            Sku = order.Sku,
            Quantity = order.Quantity,
            UnitPrice = order.UnitPrice,
            TotalPrice = order.TotalPrice,
            Status = order.Status
        };
    }

    private static string GenerateOrderNumber()
    {
        return $"ORD-{Guid.NewGuid().ToString().ToUpper()}";
    }
}
