using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderServices.Domain.Entities
{
    /// <summary>
    /// Сущность заказа.
    /// </summary>
    public class Order : BaseEntity
    {
        /// <summary>
        /// Описание заказа.
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }
}