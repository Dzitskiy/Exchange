namespace OrderProcessing.Contracts.Requests
{
    /// <summary>
    /// Модель ответа для создания заказа.
    /// </summary>
    public class CreateOrderRequest
    {
        /// <summary>
        /// Описание заказа.
        /// </summary>
        public string Description { get; set; }
    }
}