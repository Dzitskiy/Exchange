using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalServices.Domain.Entities
{
    /// <summary>
    /// Базовая сущность.
    /// </summary>
    public class BaseEntity
    {
        /// <summary>
        /// Идентификатор.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Дата создания.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
