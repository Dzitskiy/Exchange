using Common.Contracts.Events;
using Confluent.Kafka;
using ExternalServices.AppServices.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ExternalServices.Daemon.BackgroundServices
{
    public class OrderIdConsumer : BackgroundService, IDisposable
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly IOrderProcessingService _orderService;
        private readonly ILogger<OrderIdConsumer> _logger;

        private readonly string _orderCreatedTopic;

        public OrderIdConsumer(//IConfiguration config,
                                      IOrderProcessingService orderService,
                                      ILogger<OrderIdConsumer> logger)
        {

            var config = new ConfigurationBuilder()
       .AddJsonFile("appsettings.json")
       .Build();

            _orderService = orderService;
            _logger = logger;

            _orderCreatedTopic = config["Kafka:Topics:OrderCreated"];

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = config["Kafka:BootstrapServers"],                
                GroupId = config["Kafka:ConsumerGroup"],
                AutoOffsetReset = AutoOffsetReset.Earliest,
                //EnableAutoCommit = false,
                //EnablePartitionEof = true,
                //MaxPollIntervalMs = 300000
            };

            _consumer = new ConsumerBuilder<string, string>(consumerConfig)
                .SetErrorHandler((_, e) =>
                    logger.LogError("Kafka error: {Reason}", e.Reason))
                .SetLogHandler((_, log) =>
                    logger.LogInformation("Kafka log: {Message}", log.Message))
                .Build();
        }

        /// <summary>
        /// Запуск фонового сервиса для обработки сообщений о создании заказов.
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _consumer.Subscribe(_orderCreatedTopic);

            Console.WriteLine($"Kafka Consumer started for topic: {_orderCreatedTopic}");

            while (true)
            {
                try
                {
                    var consumeResult = _consumer.Consume(cancellationToken);

                    var message = consumeResult.Message;

                    if (message.Value == null)
                    {
                        _logger.LogWarning("Received null");
                        continue;
                    }

                    var orderEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(message.Value);

                    if (orderEvent == null)
                    {
                        _logger.LogWarning("Received null OrderCreatedEvent");
                        continue;
                    }

                    _logger.LogInformation($"Received OrderCreatedEvent for request: {orderEvent.CorrelationId}");

                    _orderService.CompleteOrder(orderEvent.CorrelationId, orderEvent.OrderId);
                    
                    _consumer.Commit(consumeResult);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Consume error");
                    Console.WriteLine(ex);
                }
            }
        }

        /// <inheritdoc/>
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("OrderIdConsumer was started...");
            return base.StartAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("OrderIdConsumer was stopped...");
            return base.StopAsync(cancellationToken);
        }
        public override void Dispose()
        {
            _consumer.Close();
            _consumer.Dispose();
            base.Dispose();
        }
    }
}
