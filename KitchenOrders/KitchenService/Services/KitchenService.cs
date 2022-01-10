using Timestamp = Google.Protobuf.WellKnownTypes.Timestamp;

namespace KitchenService.Services;

public class KitchenService : Kitchen.KitchenBase
{
    private readonly ILogger<KitchenService> _logger;
    private readonly IProducer<Null, Order> _orderProducer;

    public KitchenService(ILogger<KitchenService> logger, IProducer<Null, Order> orderProducer)
    {
        _logger = logger;
        _orderProducer = orderProducer;
    }

    public override async Task<OrderReply> Order(OrderRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Triggered: Order");

        var success = new Random().Next(2) % 2 == 0;

        var orderReply = new OrderReply
        {
            Success = success,
            OrderId = Guid.NewGuid().ToString(),
            OrderCreated = Timestamp.FromDateTime(DateTime.UtcNow)
        };

        if (!success) return orderReply;

        var deliveryReport = await _orderProducer.ProduceAsync("order.created",
            new Message<Null, Order>
            {
                Value = new()
                {
                    orderId = orderReply.OrderId,
                    orderCreated = orderReply.OrderCreated.ToDateTimeOffset().ToUnixTimeMilliseconds()
                }
            });

        if (deliveryReport.Status != PersistenceStatus.Persisted)
        {
            _logger.LogWarning("Kafka didn't persist");
        }

        return orderReply;
    }

    public override async Task GetOrders(GetOrdersRequest request,
        Grpc.Core.IServerStreamWriter<OrderReply> responseStream,
        Grpc.Core.ServerCallContext context)
    {
        _logger.LogInformation("Triggered: Order");
        var orders = new[]
        {
            new OrderReply {Success = true, OrderId = Guid.NewGuid().ToString()},
            new OrderReply {Success = true, OrderId = Guid.NewGuid().ToString()}
        };

        foreach (var response in orders)
        {
            await responseStream.WriteAsync(response);
        }
    }
}