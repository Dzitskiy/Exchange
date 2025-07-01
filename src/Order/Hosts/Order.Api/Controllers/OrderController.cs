using Microsoft.AspNetCore.Mvc;
using OrderProcessing.AppServices.Order.Services;
using OrderProcessing.Contracts;

namespace OrderProcessing.Api.Controllers
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

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<OrderDto>> GetById(Guid id)
        {
            var dto = await _orderService.GetByIdAsync(id);
            return dto != null ? dto : NotFound();
        }
    }
}