using Common.Contracts.Events;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using OrderProcessor.Entities;
using Common.Contracts.Models;

namespace OrderProcessor.Services;

///<inheritdoc cref="IOrderProcessingService"/>/>
public class OrderProcessingService : IOrderProcessingService
{
    private readonly IProducer<Null, string> _producer;
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<OrderProcessingService> _logger;
    private readonly KafkaOptions _kafkaOptions;

    public OrderProcessingService(IOptions<KafkaOptions> options, IOrderRepository orderRepository, ILogger<OrderProcessingService> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;

        _kafkaOptions = options.Value;

        _logger.LogInformation($"Creating Kafka producer with BootstrapServers: {_kafkaOptions.BootstrapServers} and ResponseTopic: {_kafkaOptions.ResponseTopic}");

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = _kafkaOptions.BootstrapServers,
            EnableIdempotence = true,
            Acks = Acks.All,
            //LingerMs = 20, // батчинг
            //BatchSize = 65536,
            //CompressionType = CompressionType.Snappy
        };

        _producer = new ProducerBuilder<Null, string>(producerConfig).Build();
    }

    public async Task ProcessOrderAsync(Message<Ignore, string> message)
    {
        var correlationId = GetCorrelationId(message.Headers);

        try
        {
            var orderEvent = JsonSerializer.Deserialize<OrderCreateEvent>(
                   message.Value,
                   new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var model = orderEvent.OrderModel;
            if (model == null)
            {
                _logger.LogError("Order model is null for CorrelationId: {CorrelationId}", correlationId);
            }

            var order = new Order
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,

                    Operation = model.Operation,
                    InstrumentId = model.InstrumentId,
                    TradeMode = model.TradeMode,
                    ClientOrderId = model.ClientOrderId,
                    Side = model.Side,
                    OrderType = model.OrderType,
                    Price = model.Price,
                    Size = model.Size               
            };

            // Логика обработки заказа
            var orderId = await _orderRepository.CreateOrderAsync(order);

            _logger.LogInformation($"Order created with ID: {orderId} for CorrelationId: {correlationId}");

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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing order for CorrelationId: {CorrelationId}", correlationId);
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