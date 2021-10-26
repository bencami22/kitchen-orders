using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace KitchenOrders.Repositories
{
    public class PantryOrdersRepository : IOrdersRepository
    {
        private readonly HttpClient _httpClient;
        private readonly IOptionsSnapshot<PantryOptions> _pantryOptions;

        public PantryOrdersRepository(HttpClient httpClient, IOptionsSnapshot<PantryOptions> pantryOptions)
        {
            _httpClient = httpClient;
            _pantryOptions = pantryOptions;
        }

        public async Task AddAsync(OrderReply order)
        {
            await _httpClient.PostAsync(_pantryOptions.Value.PantryId + "/basket/" + order.OrderId, JsonContent.Create(order));
        }

        public async IAsyncEnumerable<OrderReply> GetAsync(int? limit = null)
        {
            var getPantryResponse = await _httpClient.GetAsync(_pantryOptions.Value.PantryId);
            getPantryResponse.EnsureSuccessStatusCode();

            var ordersResponseJson = await getPantryResponse.Content.ReadAsStringAsync();
            var orders = JsonSerializer.Deserialize<Pantry>(ordersResponseJson,
                new JsonSerializerOptions {PropertyNameCaseInsensitive = true});
            if (orders == null)
            {
                yield break;
            }

            var count = 0;
            foreach (var basket in orders.Baskets)
            {
                if (count >= limit)
                {
                    break;
                }

                count++;
                
                var getBasketResponse = await _httpClient.GetAsync(_pantryOptions.Value.PantryId + "/basket/" + basket.Name);
                getBasketResponse.EnsureSuccessStatusCode();

                var orderResponseJson = await getBasketResponse.Content.ReadAsStringAsync();
                var order = JsonSerializer.Deserialize<OrderReply>(orderResponseJson,
                    new JsonSerializerOptions {PropertyNameCaseInsensitive = true});
                if (order == null)
                {
                    continue;
                }

                yield return order;
            }
        }
    }
}