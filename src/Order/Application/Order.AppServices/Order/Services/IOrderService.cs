using OrderProcessing.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessing.AppServices.Order.Services
{
    /// <summary>
    /// Интерфейс сервиса для работы с заказами.
    /// </summary>
    public interface IOrderService
    {
        /// <summary>
        /// Добавление нового заказа.
        /// </summary>
        /// <param name="dto">Модель заказа. <see cref="OrderDto"/>.</param>
        /// <param name="cancellationToken">Отмена операции.</returns>
        /// <returns>Идентификатор заказа.</returns>
        public Task<Guid> CreateOrderAsync(OrderDto dto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Добавление нового заказа.
        /// </summary>
        /// <param name="id">Идентификатор зазака.<see cref="OrderDto"/>.</param>
        /// <param name="cancellationToken">Отмена операции.</returns>
        /// <returns>Модель заказа</returns>
        public Task<OrderDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);       
    }
}