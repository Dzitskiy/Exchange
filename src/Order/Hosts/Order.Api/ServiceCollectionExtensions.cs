using Cassandra;
using Common.Contracts.Settings;
using OrderProcessing.Api.BackgroundServices;
using OrderProcessing.AppServices;
using OrderProcessing.AppServices.Order.Repositories;
using OrderProcessing.AppServices.Order.Services;
using OrderProcessing.DataAccess.Orders;

namespace OrderProcessing.WebApi
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration config)
        {

            services.AddSingleton<IKafkaProducerService, KafkaProducerService>();

            services.AddSingleton<IOrderService, OrderService>();
            services.AddSingleton<IOrderRepository, OrderRepository>();

            services.Configure<KafkaOptions>(config.GetSection("Kafka"));

            services.AddHostedService<OrderProcessingConsumer>();

            #region Cassandra

            // Регистрация Cassandra
            services.AddSingleton<ICluster>(_ =>
                Cluster.Builder()
                    .AddContactPoint("localhost")
                    .WithPort(9042)
                    .Build());

            services.AddSingleton<Cassandra.ISession>(provider =>
            {
                var cluster = provider.GetRequiredService<ICluster>();
                return cluster.Connect();
            });

            #endregion
         
            return services;
        }
    }
}