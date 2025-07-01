using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace External.Contracts.Requests
{
    /// <summary>
    /// Модель ответа при создании нового заказа.
    /// </summary>
    public class CreateOrderResponse
    {
        /// <summary>
        /// Идентификатор заказа.
        /// </summary>
        public Guid OrderId { get; set; }

    }
}