using Common.Contracts.Settings;
using Microsoft.Extensions.Options;
using OrderApi.Services;

namespace OrderApi.HostedServices;

public class KafkaConsumerHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly KafkaOptions _options;
    private readonly ILogger<KafkaConsumerHostedService> _logger;

    public KafkaConsumerHostedService(
        IServiceProvider serviceProvider,
        IOptions<KafkaOptions> options,
        ILogger<KafkaConsumerHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _options = options.Value;
        _logger = logger;
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
                    catch (OperationCanceledException ex)
                    {
                        _logger.LogError(ex, "OperationCanceledException");

                        break;
                    }
                    catch (Exception ex)
                    {
                        // Логирование ошибок
                        _logger.LogError(ex, "Unexpected error");

                        await Task.Delay(1000, ct);

                    }
                }
            }, ct));
        }

        await Task.WhenAll(tasks);
    }
}