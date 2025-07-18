using Common.Contracts.Models;
using System;

namespace Common.Contracts.Events
{
    /// <summary>
    /// Событие создания заказа.
    /// </summary>
    /// 
    public record OrderCreateEvent
    {
        /// <summary>
        /// Уникальный идентификатор, который генерируется для идемпотентности запроса.
        /// </summary>
        public Guid CorrelationId { get; set; }

        /// <summary>
        /// Модель заказа.
        /// </summary>
        public OrderDto OrderModel { get; set; }
    }
}
