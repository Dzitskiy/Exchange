using Common.Contracts.Events;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace ExternalServices.AppServices.Services
{
    public class KafkaProducerService : IKafkaProducerService, IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly string _topic;
        private readonly ILogger<KafkaProducerService> _logger;

        public KafkaProducerService(IConfiguration config, ILogger<KafkaProducerService> logger)
        {
            var bootstrapServers = config["Kafka:BootstrapServers"];
            _topic = config["Kafka:Topics:CreateOrder"];

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = bootstrapServers,
                EnableIdempotence = true,
                Acks = Acks.All,
                MessageSendMaxRetries = 5,
                RetryBackoffMs = 1000,
                LingerMs = 5,
                BatchSize = 16384
            };

            _producer = new ProducerBuilder<string, string>(producerConfig).Build();
            _logger = logger;
        }

        public async Task ProduceOrderRequestAsync(CreateOrderEvent model)
        {
            try
            {
                var message = new Message<string, string>
                {
                    Key = model.CorrelationId.ToString(),
                    Value = JsonSerializer.Serialize(model),
                    Headers = new Headers { new Header("MessageType", Encoding.UTF8.GetBytes("OrderRequest")) }
                };

                // Produce the message to the Kafka topic

                await _producer.ProduceAsync(_topic, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error producing Kafka message for request: {model.CorrelationId}");
                throw;
            }
        }

        public void Dispose() => _producer?.Dispose();
    }
}