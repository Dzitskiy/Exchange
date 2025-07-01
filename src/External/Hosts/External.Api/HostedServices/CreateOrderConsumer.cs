using Common.Contracts.Settings;
using External.Infrastructure;
using Microsoft.Extensions.Options;

namespace External.Api.HostedServices
{

    public class CreateOrderConsumer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly KafkaOptions _options;

        public CreateOrderConsumer(
            IServiceProvider serviceProvider,
            IOptions<KafkaOptions> options)
        {
            _serviceProvider = serviceProvider;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            var tasks = new List<Task>();

            // Создаем пул консьюмеров
            for (var i = 0; i < _options.ConsumerPoolSize; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    var consumer = scope.ServiceProvider.GetRequiredService<KafkaResponseConsumer>();

                    while (!ct.IsCancellationRequested)
                    {
                        try
                        {
                            await consumer.ProcessResponseAsync(ct);
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                        catch (Exception ex)
                        {
                            // Логирование ошибок
                            await Task.Delay(1000, ct);
                        }
                    }
                }, ct));
            }

            await Task.WhenAll(tasks);
        }
    }
}