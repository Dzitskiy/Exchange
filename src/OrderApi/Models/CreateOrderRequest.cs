using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OrderApi.Models
{
    /// <summary>
    /// Модель запроса для создания нового заказа.
    /// </summary>
    public class CreateOrderRequest
    {
        [JsonPropertyName("op")]
        public string Operation { get; set; }

        [JsonPropertyName("instId")]
        public string InstrumentId { get; set; }

        [JsonPropertyName("tdMode")]
        public string TradeMode { get; set; }

        [JsonPropertyName("clOrdId")]
        public string ClientOrderId { get; set; }

        [JsonPropertyName("side")]
        public string Side { get; set; }

        [JsonPropertyName("ordType")]
        public string OrderType { get; set; }

        [JsonPropertyName("px")]
        public string? Price { get; set; }

        [JsonPropertyName("sz")]
        public string Size { get; set; }
    }
}