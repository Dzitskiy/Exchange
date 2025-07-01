using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OrderProcessing.ComponentRegistrar
{
    /// <summary>
    /// Регистратор зависимостей.
    /// </summary>
    public static class Registrar
    {
        /// <summary>
        /// Добавление сервисов.
        /// </summary>
        /// <param name="services">Коллекция сервисов.</param>
        /// <param name="configuration">Конфигурация проекта.</param>        
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            return services.AddSingleton((IConfigurationRoot)configuration)
                .ConfigureServices()
                .ConfigureRepositories()
                .ConfigureClients(configuration)
                .ConfigureBus(configuration)
                .ConfigureMapper()
                .ConfigureCqrs()
                ;
        }

        private static IServiceCollection ConfigureServices(this IServiceCollection services)
        {
            //TODO
            return services;
        }

        private static IServiceCollection ConfigureRepositories(this IServiceCollection services)
        {
            //TODO
            return services;
        }

        private static IServiceCollection ConfigureMapper(this IServiceCollection services)
        {
            //TODO
            return services;
        }
        private static IServiceCollection ConfigureCqrs(this IServiceCollection services)
        {
            //TODO
            return services;
        }

        private static IServiceCollection ConfigureBus(this IServiceCollection services, IConfiguration configuration)
        {
            //TODO
            return services;
        }

        private static IServiceCollection ConfigureClients(this IServiceCollection services, IConfiguration configuration)
        {
            //TODO
            return services;
        }
    }
}
