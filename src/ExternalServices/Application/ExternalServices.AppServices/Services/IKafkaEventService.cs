using Common.Contracts.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalServices.AppServices.Services
{
    /// <summary>
    /// Интерфейс для работы с событиями создания заказа.
    /// </summary>
    public interface IKafkaEventService
    {
        /// <summary>
        /// Добавление нового ордера.
        /// </summary>
        /// <param name="createOrderEvent">Модель события создания заказа. <see cref="CreateOrderEvent"/>.</param>
        /// <param name="cancellationToken">Отмена операции.</returns>
        /// <returns>Модель события о созданном заказе.</returns>
        public Task<OrderCreatedEvent> ProduceAndConsumeAsync (CreateOrderEvent createOrderEvent, CancellationToken cancellationToken = default);
    }
}