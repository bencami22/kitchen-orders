using System.Collections.Generic;

namespace KitchenOrders.Repositories
{
    public record Basket(string Name, int Ttl);

    public record Pantry(
        string Name, string Description, IEnumerable<string> Errors, bool Notifications, int PercentFull,
        IEnumerable<Basket> Baskets);
}