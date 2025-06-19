using OrderServices.Contracts;
using OrderServices.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderServices.AppServices.Order.Repositories
{
    /// <summary>
    ///  Репозиторий для чтения/сохранения заказов.
    /// </summary>
    public interface IOrderRepository
    {
        /// <summary>
        /// Метод добавления заказа.
        /// </summary>
        /// <param name="order">Модель заказа.</param>
        /// <param name="cancellationToken">Отмена операции.</returns>
        Task<Guid> CreateOrderAsync(Domain.Entities.Order order, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получение заказа по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор заказа.</param>
        /// <param name="cancellationToken">Отмена операции.</returns>
        /// <returns>Модель заказа.</returns>
        Task<Domain.Entities.Order> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    }
}