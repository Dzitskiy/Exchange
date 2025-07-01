using Common.Contracts.Events;
using OrderApi.Services;

public interface IKafkaOrderService
{
    Task<Guid> CreateOrderAsync(CreateOrderEvent request);
}