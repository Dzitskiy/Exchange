using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Contracts.Events
{
    /// <summary>
    /// Событие, которое генерируется при создании заказа.
    /// </summary>
    public record OrderCreatedEvent
    {
        /// <summary>
        /// Уникальный идентификатор события.
        /// </summary>
        public Guid CorrelationId { get; set; }

        /// <summary>
        /// Идентификатор заказа.
        /// </summary>
        public Guid OrderId { get; set; }
    }
}