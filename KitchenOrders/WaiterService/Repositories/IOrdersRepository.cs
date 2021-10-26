using System.Collections.Generic;
using System.Threading.Tasks;

namespace KitchenOrders.Repositories
{
    public interface IOrdersRepository
    {
        Task AddAsync(OrderReply order);
        IAsyncEnumerable<OrderReply> GetAsync(int? limit = null);
    }
}