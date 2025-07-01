using Common.Contracts.Events;
using Confluent.Kafka;
using OrderApi.Infrastructure;
using System.Reflection.PortableExecutable;
using System.Text.Json;

namespace OrderApi.Services
{
    public class KafkaOrderService : IKafkaOrderService
    {
        private readonly KafkaProducerFactory _producerFactory;
        private readonly ResponseTracker _responseTracker;
        private readonly IConfiguration _config;

        public KafkaOrderService(
            KafkaProducerFactory producerFactory,
            ResponseTracker responseTracker,
            IConfiguration config)
        {
            _producerFactory = producerFactory;
            _responseTracker = responseTracker;
            _config = config;
        }

        public async Task<Guid> CreateOrderAsync(CreateOrderEvent request)
        {
            var correlationId = Guid.NewGuid();
            var timeout = TimeSpan.FromSeconds(_config.GetValue<int>("Kafka:ResponseTimeoutSec"));

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
