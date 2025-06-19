using OrderServices.Contracts.Requests;

namespace OrderServices.ApiClient.Order
{
    /// <summary>
    /// Интерфейс для API-клиента заказов.
    /// </summary>
    public interface IOrderApiClient
    {
        /// <summary>
        /// Метод для создания нового заказа.
        /// </summary>
        /// <param name="request">Модель создания заказа.</param>
        /// <param name="cancellationToken">Операция отмены.</param>
        /// <returns>Идентификатор заказа.</returns>
        Task<Guid> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);
    }
}