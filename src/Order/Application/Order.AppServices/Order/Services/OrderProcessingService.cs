using Common.Contracts.Settings;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Prometheus;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessing.AppServices.Order.Services
{
    public class OrderProcessingService : IOrderProcessingService
    {
        /*
        private static readonly Counter _processedOrders = Metrics
    .CreateCounter("orderprocessor_orders_processed", "Total processed orders");
        private static readonly Counter _failedOrders = Metrics
            .CreateCounter("orderprocessor_orders_failed", "Total failed orders");
        private static readonly Histogram _processingTime = Metrics
            .CreateHistogram("orderprocessor_processing_time", "Order processing time in ms");
        */

        private readonly IProducer<Null, string> _producer;
        //private readonly KafkaOptions _kafkaOptions;
        
        private readonly IConfiguration _config;

        public OrderProcessingService(IConfiguration config)
        {
            _config = config;
            //_kafkaOptions = options.Value;

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = _config["Kafka:BootstrapServers"],
                EnableIdempotence = true,
                Acks = Acks.All,
                MessageSendMaxRetries = 3
            };

            _producer = new ProducerBuilder<Null, string>(producerConfig).Build();
        }

        public async Task ProcessOrderAsync(Message<Ignore, string> message)
        {
            /* using var timer = _processingTime.NewTimer(); */

            var correlationId = GetCorrelationId(message.Headers);

            try
            {
                // Логика обработки заказа
                var orderId = GenerateOrderId();

                // Отправка ответа
                var responseMessage = new Message<Null, string>
                {
                    Value = orderId.ToString(),
                    Headers = new Headers {
                    new Header("CorrelationId", correlationId.ToByteArray())
                }
                };

                await _producer.ProduceAsync(
                    _config["Kafka:ResponseTopic"],
                    responseMessage
                );

                /* _processedOrders.Inc(); */
            }
            catch (Exception ex)
            {
                /* _failedOrders.Inc(); */
                // Перевыброс исключения для управления повторной обработкой
                throw new Exception("Order processing failed", ex);
            }
        }

        private static Guid GetCorrelationId(Headers headers)
        {
            var header = headers.First(h => h.Key == "CorrelationId");
            return new Guid(header.GetValueBytes());
        }

        private static Guid GenerateOrderId() => Guid.NewGuid();
    }
}
