using Microsoft.AspNetCore.Mvc;
using OrderServices.AppServices.Order.Services;
using OrderServices.Contracts;
using OrderServices.Contracts.Requests;
using System.ComponentModel.DataAnnotations;

namespace OrderServices.Api.Controllers
{
    /// <summary>
    /// Контроллер для управления заказами.
    /// </summary>
    [ApiController]
    [Route("v1/[controller]")]

    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        private readonly ILogger<OrderController> _logger;
        
        public OrderController(IOrderService orderService, ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        [HttpPost()]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateOrder([Required] CreateOrderRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating a new order");

            var dto = new OrderDto
            {
                Description = request.Description,
            };
;
            var orderId = await _orderService.CreateOrderAsync(dto, cancellationToken);
            
            return CreatedAtAction(nameof(CreateOrder), new CreateOrderResponse { Id = orderId });
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<OrderDto>> GetById(Guid id)
        {
            var dto = await _orderService.GetByIdAsync(id);
            return dto != null ? dto : NotFound();
        }
    }
}