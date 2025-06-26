using Common.Contracts.Events;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace ExternalServices.AppServices.Services
{
    /// <summary>
    /// <inheritdoc cref="IOrderProcessingService"/>
    /// </summary>
    public class OrderProcessingService(
        IKafkaProducerService producer,
        ILogger<OrderProcessingService> logger) : IOrderProcessingService
    {
        private readonly IKafkaProducerService _producer = producer;
        private readonly ILogger<OrderProcessingService> _logger = logger;

        /// <summary>
        /// Словарь для хранения ожидающих заказов.
        /// </summary>
        private readonly ConcurrentDictionary<Guid, TaskCompletionSource<Guid>> _pendingOrders = new();

        public async Task<Guid> CreateOrderAsync(CreateOrderEvent model, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<Guid>(TaskCreationOptions.RunContinuationsAsynchronously);

            if (!_pendingOrders.TryAdd(model.CorrelationId, tcs))
            {
                throw new InvalidOperationException($"Duplicate request detected: {model.CorrelationId}");
            }

            try
            {
                await _producer.ProduceOrderRequestAsync(model);
                _logger.LogInformation($"Order request sent: {model.CorrelationId}");

                using (cancellationToken.Register(() => tcs.TrySetCanceled()))
                {
                    return await tcs.Task;
                }
            }
            finally
            {
                _pendingOrders.TryRemove(model.CorrelationId, out _);
            }
        }

        public void CompleteOrder(Guid correlationId, Guid orderId)
        {
            if (_pendingOrders.TryRemove(correlationId, out var tcs))
            {
                tcs.TrySetResult(orderId);
                _logger.LogDebug($"Order crteation completed: {correlationId}");
            }
        }
    }
}