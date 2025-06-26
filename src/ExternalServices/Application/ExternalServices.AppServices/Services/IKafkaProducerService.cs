using Common.Contracts.Events;

namespace ExternalServices.AppServices.Services
{
    /// <summary>
    /// Интерфейс для работы с продюсером Kafka.
    /// </summary>
    public interface IKafkaProducerService
    {
        /// <summary>
        /// Отправка события создания заказа в Kafka.
        /// </summary>
        /// <param name="model">Модель события создания заказа. <see cref="CreateOrderEvent"/>.</param>
        public Task ProduceOrderRequestAsync(CreateOrderEvent model);
    }
}