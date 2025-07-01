using Cassandra;
using Common.Contracts.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderServices.AppServices.Order.Repositories;
using OrderServices.AppServices.Order.Services;
using OrderServices.DataAccess.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderServices.Daemon
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration config)
        {
            //services.AddSingleton<IKafkaProducerService, KafkaProducerService>();
            //services.AddSingleton<IOrderProcessingService, OrderProcessingService>();

            //services.AddHostedService<OrderIdConsumer>();

            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IOrderRepository, OrderRepository>();

            #region Cassandra

            // Регистрация Cassandra
            services.AddSingleton<ICluster>(_ =>
                Cluster.Builder()
                    .AddContactPoint("localhost")
                    .WithPort(9042)
                    .Build());

            services.AddScoped<Cassandra.ISession>(provider =>
            {
                var cluster = provider.GetRequiredService<ICluster>();
                return cluster.Connect();
            });

            #endregion


            services.AddHostedService<KafkaConsumerWorker>();

            services.Configure<KafkaOptions>(config.GetSection("Kafka"));

            return services;
        }
    }
}
