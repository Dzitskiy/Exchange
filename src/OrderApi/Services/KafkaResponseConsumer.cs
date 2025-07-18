using Confluent.Kafka;
using OrderApi.Infrastructure;

namespace OrderApi.Services;

public class KafkaResponseConsumer
{
    private readonly ResponseTracker _responseTracker;
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly ILogger<KafkaResponseConsumer> _logger;

    public KafkaResponseConsumer(
        IConfiguration config,
        ILogger<KafkaResponseConsumer> logger,
        ResponseTracker responseTracker)
    {
        _logger = logger;
        _responseTracker = responseTracker;
        
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"],
            GroupId = $"order-api-{Guid.NewGuid()}",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true,
            MaxPollIntervalMs = 300000
        };
        
        _logger.LogInformation($"Creating Kafka consumer with BootstrapServers: {consumerConfig.BootstrapServers} and GroupId: {consumerConfig.GroupId}");

        _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();

        var topicName = config["Kafka:ResponseTopic"];
        _logger.LogInformation($"Subscribing to topic: {topicName}");

        _consumer.Subscribe(topicName);
    }

    public async Task ProcessResponseAsync(CancellationToken ct)
    {
        var consumeResult = _consumer.Consume(ct);
        var correlationId = GetCorrelationId(consumeResult.Message.Headers);
        
        if (Guid.TryParse(consumeResult.Message.Value, out var orderId))
        {
            _logger.LogInformation($"Received response for order with orderId: {orderId} by correlationID: {correlationId}");
            _responseTracker.TryCompleteRequest(correlationId, orderId);
        }
    }

    private static Guid GetCorrelationId(Headers headers)
    {
        var header = headers.First(h => h.Key == "CorrelationId");
        return new Guid(header.GetValueBytes());
    }
}