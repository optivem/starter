using Microsoft.AspNetCore.Mvc;
using Optivem.EShop.Monolith.Core.Dtos;
using Optivem.EShop.Monolith.Core.Services;

namespace Optivem.EShop.Monolith.Controllers;

[ApiController]
public class OrderController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrderController(OrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet("/api/orders")]
    public async Task<IActionResult> BrowseOrderHistory([FromQuery] string? orderNumber)
    {
        var response = await _orderService.BrowseOrderHistoryAsync(orderNumber);
        return Ok(response);
    }

    [HttpPost("/api/orders")]
    public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest request)
    {
        var response = await _orderService.PlaceOrderAsync(request);
        return Created($"/api/orders/{response.OrderNumber}", response);
    }

    [HttpGet("/api/orders/{orderNumber}")]
    public async Task<IActionResult> GetOrder(string orderNumber)
    {
        var response = await _orderService.GetOrderAsync(orderNumber);
        return Ok(response);
    }
}
