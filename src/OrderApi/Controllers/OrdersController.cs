using Common.Contracts.Events;
using Common.Contracts.Models;
using Microsoft.AspNetCore.Mvc;
using OrderApi.Models;

namespace OrderApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderCreatingService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderCreatingService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            _logger.LogWarning("CreateOrderRequest is null");
            return BadRequest("Request cannot be null");
        }

        var correlationId = Guid.NewGuid();
        _logger.LogInformation("Generate CorrelationID to create order: {CorrelationId}", correlationId);

        try
        {
            var dto = new OrderDto
            {
                Operation = request.Operation,
                InstrumentId = request.InstrumentId,
                TradeMode = request.TradeMode,
                ClientOrderId = request.ClientOrderId,
                Side = request.Side,
                OrderType = request.OrderType,
                Price = request.Price,
                Size = request.Size
            };

            var model = new OrderCreateEvent
            {
                CorrelationId = Guid.NewGuid(),
                OrderModel = dto
            };

            var orderId = await _orderService.CreateOrderAsync(model, cancellationToken);

            return Ok(new CreateOrderResponse { OrderId = orderId });
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning($"Order creation timed out for request: {correlationId}");
            return StatusCode(StatusCodes.Status504GatewayTimeout);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error creating order for request: {correlationId}");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}