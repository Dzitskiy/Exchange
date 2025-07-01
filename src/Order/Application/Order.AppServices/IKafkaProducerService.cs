using Common.Contracts.Events;

namespace OrderProcessing.AppServices
{
    /// <summary>
    /// Интерфейс для работы с продюсером Kafka.
    /// </summary>
    public interface IKafkaProducerService
    {
        /// <summary>
        /// Отправка события с идентификатором созданного заказа в Kafka.
        /// </summary>
        /// <param name="model">Модель события с идентификатором созданного заказа. <see cref="OrderCreatedEvent"/>.</param>
        public Task ProduceOrderCreatedEventAsync(OrderCreatedEvent model);
    }
}
