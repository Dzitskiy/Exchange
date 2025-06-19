using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Common.Contracts.Events;
using Microsoft.Extensions.Configuration;
using Confluent.Kafka;

namespace ExternalServices.AppServices.Services
{
    /// <inheritdoc cref="IKafkaEventService"/>
    public class KafkaEventService : IKafkaEventService, IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly IConsumer<string, string> _consumer;
        private readonly string _createOrderTopic;
        private readonly string _orderCreatedTopic;
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(30);
        private readonly ILogger<KafkaEventService> _logger;

        public KafkaEventService(IConfiguration config, ILogger<KafkaEventService> logger)
        {            
            _logger = logger;

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = config["Kafka:BootstrapServers"]
            };

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = config["Kafka:BootstrapServers"],
                GroupId = config["Kafka:ConsumerGroup"],
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            _producer = new ProducerBuilder<string, string>(producerConfig)
                //.SetValueSerializer(new JsonSerializer<CreateOrderEvent>())
                .Build();

            _consumer = new ConsumerBuilder<string, string>(consumerConfig)
                //.SetValueSerializer(new JsonDeserializer<OrderCreatedEvent>())
                .Build();

            _createOrderTopic = config["Kafka:CreateOrderTopic"];
            _orderCreatedTopic = config["Kafka:OrderCreatedTopic"];
        }
                
        public async Task<OrderCreatedEvent> ProduceAndConsumeAsync(CreateOrderEvent createOrderEvent, CancellationToken cancellationToken = default)
        {
            // Отправка сообщения о создании заказа
            await _producer.ProduceAsync(
                _createOrderTopic,
                new Message<string, string> { 
                    Key = createOrderEvent.CorrelationId.ToString(),
                    Value = JsonSerializer.Serialize(createOrderEvent)
                }
            );

            // Подписка на топик ответов
            _consumer.Subscribe(_orderCreatedTopic);
            var cts = new CancellationTokenSource(_timeout);

            try
            {
                while (!cts.IsCancellationRequested)
                {
                    var consumeResult = _consumer.Consume(cts.Token);
                    if (consumeResult.Message.Key == createOrderEvent.CorrelationId.ToString())
                    {
                        var result = JsonSerializer.Deserialize<OrderCreatedEvent> (consumeResult.Message.Value);
                        return result;
                    }
                }
            }
            finally
            {
                _consumer.Unsubscribe();
            }

            throw new TimeoutException("Response not received within timeout");
        }

        public void Dispose()
        {
            _producer.Flush();
            _producer.Dispose();
            _consumer.Close();
            _consumer.Dispose();
        }
    }
}