using Common.Contracts.Settings;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using OrderProcessing.AppServices.Order.Services;

namespace OrderProcessing.Api.HostedServices
{
    public class OrderProcessingConsumer : BackgroundService
    {
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly IOrderProcessingService _orderProcessingService;
        //private readonly KafkaOptions _options;
        private readonly ILogger<OrderProcessingConsumer> _logger;

        private readonly string _topic;

        public OrderProcessingConsumer(
            IConfiguration config,
            OrderProcessingService orderProcessingService,
            //IOptions<KafkaOptions> options,
            ILogger<OrderProcessingConsumer> logger) 
        {
            _orderProcessingService = orderProcessingService;
            //_options = options.Value;
            _logger = logger;

            _topic = config["Kafka:ResponseTopic"];

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = config["BootstrapServers"], //_options.BootstrapServers,
                GroupId = config["ConsumerGroup"], //_options.ConsumerGroup,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
                EnableAutoOffsetStore = false,
                MaxPollIntervalMs = 300000
            };

            _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            _consumer.Subscribe(_topic);

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
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Order processing failed");
                    HandleProcessingFailure(consumeResult);
                }
                //catch (Exception ex)
                //{
                //    _logger.LogCritical(ex, "Critical error in order consumer");
                //    if (consumeResult != null)
                //        HandleProcessingFailure(consumeResult);

                //    await Task.Delay(5000, ct);
                //}
            }
        }

        private void HandleProcessingFailure(ConsumeResult<Ignore, string> result)
        {
            try
            {
                // Смещение offset для повторной обработки
                _consumer.Seek(result.TopicPartitionOffset);
            }
            catch (Exception seekEx)
            {
                _logger.LogError(seekEx, "Failed to reset offset for retry");
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
