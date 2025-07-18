namespace OrderApi.Models
{
    /// <summary>
    /// Модель ответа при создании нового заказа.
    /// </summary>
    public class CreateOrderResponse
    {
        /// <summary>
        /// Идентификатор заказа.
        /// </summary>
        public Guid OrderId { get; set; }

    }
}
