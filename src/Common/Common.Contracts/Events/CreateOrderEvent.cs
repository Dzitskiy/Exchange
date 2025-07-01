using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Contracts.Events
{
    /// <summary>
    /// Событие создания заказа.
    /// </summary>
    /// 
    public record CreateOrderEvent
    {
        /// <summary>
        /// Уникальный идентификатор, гекнерируется для идемпотентности запроса.
        /// </summary>
        public Guid CorrelationId { get; set; }

        /// <summary>
        /// Описание заказа.
        /// </summary>
        public string Description { get; set; }
    }
}
