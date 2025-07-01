using Cassandra;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace External.Infrastructure
{
    public class KafkaResponseConsumer
    {
        private readonly ResponseTracker _responseTracker;
        private readonly IConsumer<Ignore, string> _consumer;

        public KafkaResponseConsumer(
            IConfiguration config,
            ResponseTracker responseTracker)
        {
            _responseTracker = responseTracker;

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = config["Kafka:BootstrapServers"],
                GroupId = $"order-api-{Guid.NewGuid()}",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = true,
                MaxPollIntervalMs = 300000
            };

            _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
            _consumer.Subscribe(config["Kafka:ResponseTopic"]);
        }

        public async Task ProcessResponseAsync(CancellationToken ct)
        {
            var consumeResult = _consumer.Consume(ct);
            var correlationId = GetCorrelationId(consumeResult.Message.Headers);

            if (Guid.TryParse(consumeResult.Message.Value, out var orderId))
            {
                _responseTracker.TryCompleteRequest(correlationId, orderId);
            }
        }

        private static Guid GetCorrelationId(Headers headers)
        {
            var header = headers.First(h => h.Key == "CorrelationId");
            return new Guid(header.GetValueBytes());
        }
    }
}
