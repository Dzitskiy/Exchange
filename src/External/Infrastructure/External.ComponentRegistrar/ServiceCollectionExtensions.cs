using Confluent.Kafka;
using External.AppServices.Services;
using External.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace External.ComponentRegistrar
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration config)
        {
            // Конфигурация Kafka
            var kafkaConfig = new ProducerConfig
            {
                BootstrapServers = config["Kafka:BootstrapServers"],
                EnableIdempotence = true,
                Acks = Acks.All,
                MessageSendMaxRetries = 3
            };

            services.AddSingleton(kafkaConfig);
            services.AddSingleton<KafkaProducerFactory>();
            services.AddSingleton<ResponseTracker>();

            // Регистрация сервисов
            services.AddScoped<IKafkaOrderService, KafkaOrderService>();
            services.AddSingleton<KafkaResponseConsumer>();

            return services;
        }
    }
}