using OrderServices.AppServices.Order.Repositories;
using OrderServices.Contracts;

namespace OrderServices.AppServices.Order.Services
{
    /// <inheritdoc cref="IOrderService"/>
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _repository;

        public OrderService(IOrderRepository repository)
        {
            _repository = repository;
        }

        public async Task<Guid> CreateOrderAsync(OrderDto dto, CancellationToken cancellationToken = default)
        {
            var order = new Domain.Entities.Order
            {
                Id = Guid.NewGuid(),
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow
            };

            return await _repository.CreateOrderAsync(order, cancellationToken);
        }

        public async Task<OrderDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var order = await _repository.GetByIdAsync(id, cancellationToken);

           var dto = new OrderDto
           {
               Id = order.Id,
               Description = order.Description
           };

            return dto;
        }
    }
}