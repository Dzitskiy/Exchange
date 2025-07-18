namespace OrderProcessor.Entities
{
    /// <summary>
    /// Сущность заказа.
    /// </summary>
    public class Order
    {
        /// <summary>
        /// Идентификатор.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Дата создания.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Операция.
        /// </summary>
        public string Operation { get; set; }

        /// <summary>
        /// Идентификатор инструмента.
        /// </summary>
        public string InstrumentId { get; set; }

        /// <summary>
        /// Режим торговли.
        /// </summary>
        public string TradeMode { get; set; }

        /// <summary>
        /// Клиентский идентификатор заказа.
        /// </summary>
        public string ClientOrderId { get; set; }

        /// <summary>
        /// Направление сделки (покупка/продажа).
        /// </summary>
        public string Side { get; set; }

        /// <summary>
        /// Тип заказа (лимитный/рыночный).
        /// </summary>
        public string OrderType { get; set; }

        /// <summary>
        /// Цена заказа. 
        /// </summary>
        public string Price { get; set; }

        /// <summary>
        /// Размер заказа.
        /// </summary>
        public string Size { get; set; }
    }
}