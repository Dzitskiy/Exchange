using Common.Contracts.Events;
using Microsoft.AspNetCore.Mvc;
using OrderApi.Services;

namespace OrderApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IKafkaOrderService _orderService;

    public OrdersController(IKafkaOrderService orderService)
        => _orderService = orderService;

    [HttpPost]
    public async Task<IActionResult> CreateOrder(CreateOrderRequest request)
    {
        try
        {
            var model = new CreateOrderEvent
            {
                CorrelationId = Guid.NewGuid(),
                Description = request.Description
            };

            var orderId = await _orderService.CreateOrderAsync(model);
            return Ok(new { OrderId = orderId });
        }
        catch (OperationCanceledException)
        {
            return StatusCode(504, "Request timed out");
        }
    }
}

/// <summary>
/// Модель запроса для создания нового заказа.
/// </summary>
public class CreateOrderRequest
{
    /// <summary>
    /// Описание заказа.
    /// </summary>
    public string Description { get; set; }
}