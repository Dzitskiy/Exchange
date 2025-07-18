using Common.Contracts.Events;
using Confluent.Kafka;
using OrderApi.Infrastructure;
using System.Text.Json;

namespace OrderApi.Services
{
    ///<inheritdoc cref="IOrderCreatingService" />
    public class OrderCreatingService : IOrderCreatingService
    {
        private readonly KafkaProducerFactory _producerFactory;
        private readonly ResponseTracker _responseTracker;
        private readonly IConfiguration _config;
        private readonly ILogger<OrderCreatingService> _logger;
        public OrderCreatingService(
            KafkaProducerFactory producerFactory,
            ResponseTracker responseTracker,
            IConfiguration config,
            ILogger<OrderCreatingService> logger)
        {
            _producerFactory = producerFactory;
            _responseTracker = responseTracker;
            _config = config;
            _logger = logger;
        }

        public async Task<Guid> CreateOrderAsync(OrderCreateEvent request, CancellationToken cancellationToken)
        {
            var correlationId = request.CorrelationId;

            var timeout = TimeSpan.FromSeconds(_config.GetValue<int>("Kafka:ResponseTimeoutSec"));

            _logger.LogInformation("Creating order with CorrelationId: {CorrelationId} and Timeout: {Timeout}", correlationId, timeout);

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

                var topicName = _config["Kafka:RequestTopic"];

                _logger.LogInformation("Producing message to topic {TopicName} with CorrelationId: {CorrelationId}", topicName, correlationId);

                await producer.ProduceAsync(topicName, message);

                return await _responseTracker.WaitForResponseAsync(correlationId);
            }
            finally
            {
                _producerFactory.ReturnProducer(producer);
            }
        }
    }
}