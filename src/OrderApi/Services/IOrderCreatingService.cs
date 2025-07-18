using Common.Contracts.Events;

/// <summary>
/// Интерфейс сервиса, который взаимодействует с Kafka для создания заказов.
/// </summary>
public interface IOrderCreatingService
{
    /// <summary>
    /// Создает заказ асинхронно, отправляя событие в Kafka.
    /// </summary>
    /// <param name="request">Событие создания заказа.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Уникальный идентификатор созданного заказа.</returns>
    Task<Guid> CreateOrderAsync(OrderCreateEvent request, CancellationToken cancellationToken);
}