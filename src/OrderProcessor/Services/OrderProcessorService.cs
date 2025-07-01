using System.Text;
using Common.Contracts.Events;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Prometheus;

namespace OrderProcessor.Services;

public class OrderProcessorService
{
    private readonly IProducer<Null, string> _producer;
    private readonly KafkaOptions _kafkaOptions;
    private static readonly Counter _processedOrders = Metrics
        .CreateCounter("orderprocessor_orders_processed", "Total processed orders");
    private static readonly Counter _failedOrders = Metrics
        .CreateCounter("orderprocessor_orders_failed", "Total failed orders");
    private static readonly Histogram _processingTime = Metrics
        .CreateHistogram("orderprocessor_processing_time", "Order processing time in ms");

    private readonly IOrderRepository _orderRepository;

    public OrderProcessorService(IOptions<KafkaOptions> options, IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;

        _kafkaOptions = options.Value;

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = _kafkaOptions.BootstrapServers,
            EnableIdempotence = true,
            Acks = Acks.All
        };

        _producer = new ProducerBuilder<Null, string>(producerConfig).Build();
    }

    public async Task ProcessOrderAsync(Message<Ignore, string> message)
    {
        using var timer = _processingTime.NewTimer();
        var correlationId = GetCorrelationId(message.Headers);

        try
        {
            var orderEvent = JsonSerializer.Deserialize<CreateOrderEvent>(
                   message.Value,
                   new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            var order = new Order
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                Description = orderEvent.Description
            };

            // Логика обработки заказа
            var orderId = await _orderRepository.CreateOrderAsync(order);

            // Отправка ответа
            var responseMessage = new Message<Null, string>
            {
                Value = orderId.ToString(),
                Headers = new Headers {
                    new Header("CorrelationId", correlationId.ToByteArray())
                }
            };

            await _producer.ProduceAsync(
                _kafkaOptions.ResponseTopic,
                responseMessage
            );

            _processedOrders.Inc();
        }
        catch (Exception ex)
        {
            _failedOrders.Inc();
            // Перевыброс исключения для управления повторной обработкой
            throw new OrderProcessingException("Order processing failed", ex);
        }
    }

    private static Guid GetCorrelationId(Headers headers)
    {
        var header = headers.First(h => h.Key == "CorrelationId");
        return new Guid(header.GetValueBytes());
    }

}

public class OrderProcessingException : Exception
{
    public OrderProcessingException(string message, Exception inner)
        : base(message, inner) { }
}

public class KafkaOptions
{
    public string BootstrapServers { get; set; }
    public string RequestTopic { get; set; }
    public string ResponseTopic { get; set; }
    public string ConsumerGroup { get; set; }
    public int RetryIntervalMs { get; set; }
    public int MaxRetryAttempts { get; set; }
}