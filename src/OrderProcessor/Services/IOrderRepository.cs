using OrderProcessor.Entities;

namespace OrderProcessor.Services
{
    /// <summary>
    ///  Репозиторий для чтения/сохранения заказов.
    /// </summary>
    public interface IOrderRepository
    {
        /// <summary>
        /// Метод добавления заказа.
        /// </summary>
        /// <param name="order">Модель заказа.</param>
        /// <param name="cancellationToken">Отмена операции.</returns>
        Task<Guid> CreateOrderAsync(Order order, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получение заказа по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор заказа.</param>
        /// <param name="cancellationToken">Отмена операции.</returns>
        /// <returns>Модель заказа.</returns>
        Task<Order> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    }
}