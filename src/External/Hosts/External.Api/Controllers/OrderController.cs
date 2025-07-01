using Common.Contracts.Events;
using External.AppServices.Services;
using External.Contracts.Requests;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace OrderService.Api.Controllers
{
    /// <summary>
    /// Контроллер для управления заказами.
    /// </summary>
    [ApiController]
    [Route("api/orders")]
    public class OrderController : ControllerBase
    {
        private readonly ICreateOrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            ICreateOrderService orderService,
            ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrderAsync([Required] CreateOrderRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                _logger.LogWarning("CreateOrderRequest is null");
                return BadRequest("Request cannot be null");
            }

            var correlationId = Guid.NewGuid();
            _logger.LogInformation("Received request to create order, correlation ID: {CorrelationId}", correlationId);

            try
            {
                var model = new CreateOrderEvent
                {
                    CorrelationId = correlationId,
                    Description = request.Description
                };

                var orderId = await _orderService.CreateOrderAsync(model, cancellationToken);
                return Accepted(new CreateOrderResponse { OrderId = orderId });
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
}