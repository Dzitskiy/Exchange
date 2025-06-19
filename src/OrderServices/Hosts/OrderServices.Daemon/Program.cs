using Cassandra;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderServices.AppServices.Order.Repositories;
using OrderServices.AppServices.Order.Services;
using OrderServices.Daemon.Workers;
using OrderServices.DataAccess.Orders;

namespace OrderServices.Daemon
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            IHost host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHttpClient();

                    //services.AddServices(hostContext.Configuration, hostContext.HostingEnvironment);
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
                    })
                .Build();
            
            await host.RunAsync();
        }
    }
}