using OrderServices.ApiClient.Order;
using OrderServices.Contracts.Requests;

namespace OrderServices.ApiClient
{
    /// <inheritdoc cref="IOrderApiClient"/>
    public class OrderApiClient : IOrderApiClient
    {
        public OrderApiClient(HttpClient httpClient)
        {
        }

        Task<Guid> IOrderApiClient.CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
