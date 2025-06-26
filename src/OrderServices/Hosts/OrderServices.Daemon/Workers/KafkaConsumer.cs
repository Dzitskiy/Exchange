using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Common.Contracts.Events;
using static Confluent.Kafka.ConfigPropertyNames;
using OrderServices.AppServices.Order.Services;


namespace OrderServices.Daemon.Workers
{
    /// <summary>
    /// Фоновый сервис.
    /// </summary>
    public class KafkaConsumerWorker : BackgroundService
    {
        private readonly IProducer<string, string> _producer;
        private readonly IConsumer<string, string> _consumer;
        private readonly string _createOrderTopic;
        private readonly string _orderCreatedTopic;
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(30);
        
        private readonly ILogger<KafkaConsumerWorker> _logger;
        private readonly IOrderService _orderService;

        //private readonly HttpClient _httpClient;
        //private readonly string _orderApiUrl;

        /// <summary>
        /// Инициализация воркера <see cref="KafkaConsumerWorker"/>
        /// </summary>
        public KafkaConsumerWorker(
            //IHttpClientFactory httpClientFactory,
            IOrderService orderService,
            ILogger<KafkaConsumerWorker> logger)
        {
            _logger = logger;
            _orderService = orderService;

            //_httpClient = httpClientFactory.CreateClient();
            //_orderApiUrl = "http://localhost:5000"; // Environment.GetEnvironmentVariable("ORDER_API_URL")!;

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var bootstrapServers = config["Kafka:BootstrapServers"];
            var createOrderTopic = config["Kafka:Topics:CreateOrder"];
            var orderCreatedTopic = config["Kafka:Topics:OrderCreated"];
            
            //var consumerGroup = "order-consumer-group";

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = config["Kafka:BootstrapServers"],
                GroupId = config["Kafka:ConsumerGroup"],
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = config["Kafka:BootstrapServers"]
            };

            _consumer = new ConsumerBuilder<string, string>(consumerConfig)
                //.SetValueSerializer(new JsonDeserializer<OrderCreated>())
                .Build();

            _producer = new ProducerBuilder<string, string>(producerConfig)
                //.SetValueSerializer(new JsonSerializer<CreateOrderEvent>())
                .Build();

            _createOrderTopic = config["Kafka:Topics:CreateOrder"];
            _orderCreatedTopic = config["Kafka:Topics:OrderCreated"];
        }

        /// <inheritdoc/>
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("OrderServices Daemon was started...");
            return base.StartAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("OrderServices Daemon was stopped...");
            return base.StopAsync(cancellationToken);
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _consumer.Subscribe(_createOrderTopic);

            Console.WriteLine($"Kafka Consumer started for topic: {_createOrderTopic}");

            while (true)
            {
                try
                {
                    var consumeResult = _consumer.Consume();
                    var message = consumeResult.Message.Value;

                    var createOrderEvent = JsonSerializer.Deserialize<CreateOrderEvent>(consumeResult.Message.Value);

                    if (createOrderEvent == null)
                    {
                        _logger.LogError("Failed to deserialize CreateOrderEvent from Kafka message.");
                        continue;
                    }

                    _logger.LogInformation($"Received CreateOrderEvent for request: {createOrderEvent.CorrelationId}");

                    var orderId = await ProcessOrderEvent(createOrderEvent);

                    var orderCreatedEvent = new OrderCreatedEvent 
                    {
                        CorrelationId = createOrderEvent.CorrelationId,
                        OrderId = orderId,
                    };

                    // Отправка ответа
                    await _producer.ProduceAsync(
                        _orderCreatedTopic,
                        new Message<string, string> {
                            Key = createOrderEvent.CorrelationId.ToString(),
                            Value = JsonSerializer.Serialize(orderCreatedEvent) }
                    );

                    Console.WriteLine($"Processed order: {orderId} for request: {createOrderEvent.CorrelationId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing Kafka message: {ex.Message}");
                }
            }
        }

        private async Task<Guid> ProcessOrderEvent(CreateOrderEvent createOrderEvent)
        {
            //TODO

            //var response = await _httpClient.PostAsJsonAsync(
            //    $"{_orderApiUrl}/api/orders",
            //    new
            //    {
            //        Id = orderEvent.EventId,
            //        orderEvent.Description
            //    });

            //if (response.IsSuccessStatusCode)
            //{
            //    _logger.LogInformation($"Order created: {orderEvent.EventId}");
            //}
            //else
            //{
            //    var content = await response.Content.ReadAsStringAsync();
            //    _logger.LogError($"Failed to create order: {response.StatusCode} - {content}");
            //}

            return await _orderService.CreateOrderAsync(new Contracts.OrderDto
            {
                Description = createOrderEvent.Description
            });
        }
    }
}
