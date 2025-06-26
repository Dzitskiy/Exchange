using Common.Contracts.Events;
using ExternalServices.AppServices.Services;
using ExternalServices.Contracts.Requests;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace OrderService.Api.Controllers
{
    /// <summary>
    /// Контроллер для управления заказами.
    /// </summary>
    [ApiController]
    [Route("v1/api/orders")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderProcessingService _orderService;
        private readonly ILogger<OrderController> _logger;

        ///private readonly IKafkaEventService _kafkaEventService;
        //private readonly ILogger<OrderController> _logger;

        public OrderController(
            IOrderProcessingService orderService,
            //IKafkaEventService kafkaEventService, 
            ILogger<OrderController> logger)
        {
            _orderService = orderService;
            //_kafkaEventService = kafkaEventService;
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
            _logger.LogInformation("Received request to create order with correlation ID: {CorrelationId}", correlationId);

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

            //try
            //{
            //    _logger.LogInformation("Creating a new order event for Kafka");

            //    var correlationId = Guid.NewGuid();

            //    var createOrderEvent = new CreateOrderEvent
            //    { 
            //        CorrelationId = correlationId, 
            //        Description = request.Description
            //    };

            //    try
            //    {
            //        var response = await _kafkaEventService.ProduceAndConsumeAsync(createOrderEvent);

            //        if (response == null)
            //        {
            //            _logger.LogError("Failed to create order event in Kafka");
            //            return BadRequest("Failed to create order event");
            //        }
            //        _logger.LogInformation("Order created successfully with ID: {OrderId}", response.OrderId);
            //        return Ok(response.OrderId);
            //    }
            //    catch (TimeoutException ex)
            //    {
            //        return StatusCode(503, ex.Message);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex, "Error producing Kafka message");
            //    return StatusCode(500, ex.Message);
            //}
        }
    }
}