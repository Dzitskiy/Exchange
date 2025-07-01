using Common.Contracts.Events;
using Confluent.Kafka;
using External.Infrastructure;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace External.AppServices.Services
{
    /// <inheritdoc cref="ICreateOrderService"/>/>
    public class CreateOrderService : ICreateOrderService
    {
        private readonly KafkaProducerFactory _producerFactory;
        private readonly ResponseTracker _responseTracker;
        private readonly IConfiguration _config;

        public CreateOrderService(
            KafkaProducerFactory producerFactory,
            ResponseTracker responseTracker,
            IConfiguration config)
        {
            _producerFactory = producerFactory;
            _responseTracker = responseTracker;
            _config = config;
        }

        public async Task<Guid> CreateOrderAsync(CreateOrderEvent request, CancellationToken cancellationToken)
        {
            var correlationId = Guid.NewGuid();
            var timeout = TimeSpan.FromSeconds(int.Parse(_config["Kafka:ResponseTimeoutSec"]));

            _responseTracker.RegisterRequest(correlationId, timeout);

            var producer = _producerFactory.GetProducer();
            try
            {
                var message = new Message<Null, string>
                {
                    Value = JsonSerializer.Serialize(request),
                    Headers = new Headers {
                    new Header("CorrelationId", correlationId.ToByteArray())
                }
                };

                await producer.ProduceAsync(
                    _config["Kafka:RequestTopic"],
                    message
                );

                return await _responseTracker.WaitForResponseAsync(correlationId);
            }
            finally
            {
                _producerFactory.ReturnProducer(producer);
            }
        }
    }
}
