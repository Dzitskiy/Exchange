namespace OrderProcessing.Contracts.Requests
{
    /// <summary>
    /// Модель создания заказа.
    /// </summary>
    public class CreateOrderResponse
    {
        /// <summary>
        /// Идентификатор заказа.
        /// </summary>
        public Guid Id { get; set; }        
    }
}