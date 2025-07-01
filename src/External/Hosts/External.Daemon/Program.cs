using Common.Contracts.Settings;
using ExternalServices.AppServices.Services;
using ExternalServices.AppServices.Services.Interfaces;
using ExternalServices.Daemon.BackgroundServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace OrderServices.Daemon
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            IHost host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    //services.AddHttpClient();

                    services.AddSingleton<IConfiguration>(hostContext.Configuration);

                    //services.AddHttpClient();

                    ////services.AddServices(hostContext.Configuration, hostContext.HostingEnvironment);
                    //services.AddScoped<IOrderService, OrderService>();
                    //services.AddScoped<IOrderRepository, OrderRepository>();

                    //#region Cassandra

                    //// Регистрация Cassandra
                    //services.AddSingleton<ICluster>(_ =>
                    //    Cluster.Builder()
                    //        .AddContactPoint("localhost")
                    //        .WithPort(9042)
                    //        .Build());

                    //services.AddScoped<Cassandra.ISession>(provider =>
                    //{
                    //    var cluster = provider.GetRequiredService<ICluster>();
                    //    return cluster.Connect();
                    //});

                    //#endregion

                    services.AddSingleton<IKafkaProducerService, KafkaProducerService>();
                    services.AddSingleton<IOrderProcessingService, OrderProcessingService>();

                    services.AddHostedService<OrderIdConsumer>();
                })
                .Build();

            await host.RunAsync();
        }
    }
}