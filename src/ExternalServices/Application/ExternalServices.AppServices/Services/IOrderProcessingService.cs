using Common.Contracts.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalServices.AppServices.Services
{
    /// <summary>
    // Интерфейс сервиса создания заказов.
    /// </summary>
    public interface IOrderProcessingService
    {
        /// <summary>
        /// Создание нового заказа.
        /// </summary>
        /// <param name="model">Модель события создания заказа. <see cref="CreateOrderEvent"/>.</param>
        /// <param name="cancellationToken">Отмена операции.</returns>
        /// <returns>Идентификатор заказа.</returns>
        public Task<Guid> CreateOrderAsync(CreateOrderEvent model, CancellationToken cancellationToken);

        /// <summary>
        /// Завершение запроса по идентификатору запроса и идентификатору заказа.
        /// </summary>
        /// <param name="correlationId">Идентификатор запроса.</param>
        /// <param name="orderId">Идентификатор созданного заказа.</param>
        public void CompleteOrder(Guid correlationId, Guid orderId);
    }
}