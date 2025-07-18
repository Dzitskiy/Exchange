using Confluent.Kafka;
using Microsoft.Extensions.Options;
using OrderProcessor.Services;

namespace OrderProcessor.HostedServices
{
    public class KafkaConsumerHostedService : BackgroundService
    {
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly KafkaOptions _options;
        private readonly ILogger<KafkaConsumerHostedService> _logger;

        public KafkaConsumerHostedService(
            IOrderProcessingService orderProcessingService,
            IOptions<KafkaOptions> options,
            ILogger<KafkaConsumerHostedService> logger)
        {
            _orderProcessingService = orderProcessingService;
            _options = options.Value;
            _logger = logger;

            _logger.LogInformation($"Creating Kafka consumer with BootstrapServers: {_options.BootstrapServers} and RequestTopic: {_options.RequestTopic}, ConsumerGroup: {_options.ConsumerGroup}");

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _options.BootstrapServers,
                GroupId = _options.ConsumerGroup,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
                EnableAutoOffsetStore = false,
                MaxPollIntervalMs = 300000,
                //AllowAutoCreateTopics = false,
                //SessionTimeoutMs = 10000,
                //HeartbeatIntervalMs = 3000
            };

            
            _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            _consumer.Subscribe(_options.RequestTopic);

            while (!ct.IsCancellationRequested)
            {
                ConsumeResult<Ignore, string> consumeResult = null;

                try
                {
                    consumeResult = _consumer.Consume(ct);
                    await _orderProcessingService.ProcessOrderAsync(consumeResult.Message);
                    _consumer.StoreOffset(consumeResult);
                    _consumer.Commit();
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, $"Kafka consume error: {ex.Error.Reason}");
                }
                catch (OrderProcessingException ex)
                {
                    _logger.LogError(ex, "Order processing failed");
                    HandleProcessingFailure(consumeResult);
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Critical error in order consumer");
                    if (consumeResult != null)
                        HandleProcessingFailure(consumeResult);

                    await Task.Delay(5000, ct);
                }
            }
        }

        private void HandleProcessingFailure(ConsumeResult<Ignore, string> result)
        {
            try
            {
                // Смещение offset для повторной обработки
                _consumer.Seek(result.TopicPartitionOffset);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reset offset for retry");
            }
        }

        public override void Dispose()
        {
            _consumer.Close();
            _consumer.Dispose();
            base.Dispose();
        }
    }
}