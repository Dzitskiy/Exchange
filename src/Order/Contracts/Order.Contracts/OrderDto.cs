using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessing.Contracts
{
    /// <summary>
    /// Модель заказа.
    /// </summary>
    public class OrderDto
    {
        /// <summary>
        /// Идентификатор заказа.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Описание заказа.
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }
}