using Cassandra;
using Microsoft.Extensions.DependencyInjection;
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
                    services.AddHttpClient();

                    // Регистрация сервисов
                    services.AddServices(hostContext.Configuration);
                })
                .Build();
           
            await host.RunAsync();
        }
    }
}