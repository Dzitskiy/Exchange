using Common.Contracts.Events;
using Confluent.Kafka;
using OrderProcessing.AppServices;
using OrderProcessing.AppServices.Order.Services;
using OrderProcessing.Contracts;
using System.Text.Json;
using static Confluent.Kafka.ConfigPropertyNames;

namespace OrderProcessing.Api.BackgroundServices
{
    public class OrderProcessingConsumer : BackgroundService
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly IKafkaProducerService _kafkaProducerService;
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderProcessingConsumer> _logger;
        
        private readonly int _batchSize;
        private readonly TimeSpan _batchTimeout;
        private readonly string _topic;

        public OrderProcessingConsumer(
            IConfiguration config,
            IKafkaProducerService kafkaProducerService,
            IOrderService orderService,
            ILogger<OrderProcessingConsumer> logger)
        {
            _kafkaProducerService = kafkaProducerService;
            _orderService = orderService;
            _logger = logger;

            _topic = config["Kafka:Topics:CreateOrder"];            

            _batchSize = config.GetValue("Kafka:BatchSize", 100);
            _batchTimeout = TimeSpan.FromMilliseconds(config.GetValue("Kafka:BatchTimeoutMs", 1000));

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = config["Kafka:BootstrapServers"],
                GroupId = config["Kafka:ConsumerGroup"],
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
                EnablePartitionEof = true,
                MaxPollIntervalMs = 300000,
                FetchMaxBytes = 10485760, // 10 MB
                FetchMinBytes = 1048576,  // 1 MB
                FetchWaitMaxMs = 100
            };

            _consumer = new ConsumerBuilder<string, string>(consumerConfig)
                .SetErrorHandler((_, e) =>
                    logger.LogError("Consumer error: {Reason}", e.Reason))
                .SetLogHandler((_, log) =>
                    logger.LogInformation("Kafka log: {Message}", log.Message))
                .Build();
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _consumer.Subscribe(_topic);

            Console.WriteLine($"BackgroundService {nameof(OrderProcessingConsumer)} started for topic: {_topic}");

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Пакетное чтение сообщений
                    var batch = ConsumeBatch(cancellationToken);

                    if (batch.Count == 0)
                        continue;

                    // Параллельная обработка
                    var processingTasks = batch.Select(message =>
                        ProcessMessageAsync(message, cancellationToken)).ToList();

                    await Task.WhenAll(processingTasks);

                    // Коммит обработанных сообщений
                    CommitProcessedBatch(batch);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error in Kafka consumer {nameof(OrderProcessingConsumer)}");
                }
            }
        }

        private async Task ProcessMessageAsync(ConsumeResult<string, string> consumeResult, CancellationToken cancellationToken)
        {
            try
            {
                var orderEvent = JsonSerializer.Deserialize<CreateOrderEvent>(
                    consumeResult.Message.Value,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (orderEvent == null)
                {
                    _logger.LogWarning("Received null OrderCreatedEvent");
                    return;
                }

                _logger.LogInformation($"Received CreateOrderEvent for request: {orderEvent.CorrelationId}");

                var orderId = await ProcessOrderEvent(orderEvent);

                var orderCreatedEvent = new OrderCreatedEvent
                {
                    CorrelationId = orderEvent.CorrelationId,
                    OrderId = orderId,
                };

                // Отправка ответа
                await _kafkaProducerService.ProduceOrderCreatedEventAsync(orderCreatedEvent);

                Console.WriteLine($"Processed order: {orderId} for request: {orderCreatedEvent.CorrelationId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in Kafka consumer {nameof(OrderProcessingConsumer)}");
            }
        }

        public override void Dispose()
        {
            _consumer.Close();
            _consumer.Dispose();
            base.Dispose();
        }

        private async Task<Guid> ProcessOrderEvent(CreateOrderEvent createOrderEvent)
        {
            var orderDto = new OrderDto
            {
                Description = createOrderEvent.Description
            };

            return await _orderService.CreateOrderAsync(orderDto);
        }

        private IList<ConsumeResult<string, string>> ConsumeBatch(CancellationToken ct)
        {
            var messages = new List<ConsumeResult<string, string>>();
            var startTime = DateTime.UtcNow;

            while (messages.Count < _batchSize &&
                   DateTime.UtcNow - startTime < _batchTimeout)
            {
                var result = _consumer.Consume(ct);
                if (result != null && !result.IsPartitionEOF)
                {
                    messages.Add(result);
                }
            }

            return messages;
        }

        private void CommitProcessedBatch(IEnumerable<ConsumeResult<string, string>> batch)
        {
            try
            {
                var last = batch.Last();
                _consumer.Commit(last);
            }
            catch (KafkaException ex)
            {
                _logger.LogError(ex, "Commit error");
            }
        }
    }
}
