using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalServices.Contracts.Requests
{
    /// <summary>
    /// Модель запроса для создания нового заказа.
    /// </summary>
    public class CreateOrderRequest
    {
        /// <summary>
        /// Описание заказа.
        /// </summary>
        public string Description { get; set; }
    }
}
