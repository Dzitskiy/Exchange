using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OrderServices.Daemon.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace OrderServices.Daemon
{
    /// <summary>
    /// Расширения ServiceCollection.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        //private const string MessageQueueSectionName = "MessageQueue";

        /// <summary>
        /// Регистрация сервисов.
        /// </summary>
        /// <param name="services">Коллекция сервисов.</param>
        /// <param name="configuration">Конфигурация проекта.</param>
        /// <param name="environment">Переменная окружения сервиса.</param>        
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            services.ConfigureBusService(configuration);
            services.AddApiClients(configuration);
            return services;
        }

        private static IServiceCollection ConfigureBusService(this IServiceCollection services, IConfiguration configuration)
        {
            //TODO 
            //var section = configuration.GetSection(MessageQueueSectionName);

            return services;
        }

        private static IServiceCollection AddApiClients(this IServiceCollection services, IConfiguration configuration)
        {
            //services.AddConfigurationOptions<ClientOptions>();
            //services.AddApiClient<IOrderApiClient, OrderApiClient>().ConfigureHttpClient(ConfigureApiClient);

            return services;
        }

        private static void ConfigureApiClient(IServiceProvider serviceProvider, HttpClient client)
        {
            var options = serviceProvider.GetRequiredService<IOptions<ClientOptions>>().Value;
            client.BaseAddress = new Uri(options.OrderApiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
            client.DefaultRequestHeaders.ConnectionClose = true;
            client.Timeout = options.Timeout;
        }
    }
}
