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
    public class CreateOrderEvent
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

//public record CreateOrderEvent(
//    string RequestId,
//    string ProductId,
//    int Quantity,
//    decimal Price,
//    DateTimeOffset CreatedAt);

