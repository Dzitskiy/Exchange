using Confluent.Kafka;

namespace OrderProcessor.Services
{
    /// <summary>
    /// Интерфейс для работы с продюсером Kafka.
    /// </summary>
    public interface IOrderProcessingService
    {
        /// <summary>
        /// Отправка события с идентификатором созданного заказа в Kafka.
        /// </summary>
        /// <param name="message">Модель создания заказа.</param>
        public Task ProcessOrderAsync(Message<Ignore, string> message);
    }
}