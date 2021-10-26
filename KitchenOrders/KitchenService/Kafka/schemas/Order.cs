//dotnet avro generate --id 1 --registry-url http://localhost:8081

namespace KitchenOrders.Messages
{
    /// <summary>Represents a successfully created kitchen order.</summary>
    public class Order
    {
        public string orderId { get; set; }
        public long orderCreated { get; set; }
    }
}
