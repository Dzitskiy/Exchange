using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessing.Domain.Entities
{
    /// <summary>
    /// Сущность заказа.
    /// </summary>
    public class Order 
    {
        /// <summary>
        /// Идентификатор.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Дата создания.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Описание заказа.
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }
}