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
    [Route("v1/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IKafkaEventService _kafkaEventService;

        private readonly ILogger<OrderController> _logger;

        public OrderController(IKafkaEventService kafkaEventService, ILogger<OrderController> logger)
        {
            _kafkaEventService = kafkaEventService;
            _logger = logger;
        }

        [HttpPost()]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateOrderAsync([Required] CreateOrderRequest request, CancellationToken cancellationToken)
        {                      
            try
            {
                _logger.LogInformation("Creating a new order event for Kafka");

                var correlationId = Guid.NewGuid();

                var createOrderEvent = new CreateOrderEvent
                { 
                    CorrelationId = correlationId, 
                    Description = request.Description
                };

                try
                {
                    var response = await _kafkaEventService.ProduceAndConsumeAsync(createOrderEvent);

                    if (response == null)
                    {
                        _logger.LogError("Failed to create order event in Kafka");
                        return BadRequest("Failed to create order event");
                    }
                    _logger.LogInformation("Order created successfully with ID: {OrderId}", response.OrderId);
                    return Ok(response.OrderId);
                }
                catch (TimeoutException ex)
                {
                    return StatusCode(503, ex.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error producing Kafka message");
                return StatusCode(500, ex.Message);
            }
        }
    }
}