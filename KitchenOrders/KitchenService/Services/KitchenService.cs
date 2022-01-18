using ksqlDB.RestApi.Client.KSql.Query.Context;
using Timestamp = Google.Protobuf.WellKnownTypes.Timestamp;

namespace KitchenService.Services;

public class KitchenService : Kitchen.KitchenBase
{
    private readonly ILogger<KitchenService> _logger;
    private readonly IProducer<Null, Order> _orderProducer;
    private readonly IKSqlDBContext _kSqlDbContext;
    
    public KitchenService(ILogger<KitchenService> logger, IProducer<Null, Order> orderProducer, IKSqlDBContext kSqlDbContextcontext)
    {
        _logger = logger;
        _orderProducer = orderProducer;
        _kSqlDbContext = kSqlDbContextcontext;
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
                    orderCreated = orderReply.OrderCreated.ToDateTimeOffset().ToUnixTimeMilliseconds(),
                    productId = request.ProductId,
                    quantity = request.Quantity
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
        _logger.LogInformation("Triggered: GetOrders");

        var stats = await _kSqlDbContext.ExecutePullQuery<ProductOrderStats>("SElECT productId, noOfOrders, totalQuantity, lastOrderId FROM product_orders;");
        _logger.LogInformation("Only displaying this as a log: {@Stats}",stats.ToString());
        
        var result = _kSqlDbContext
            .CreatePullQuery<Order>("orders")
            .GetManyAsync();

        var currentCount = 0;
        await foreach (var response in result)
        {
            if (currentCount >= request.Limit)
            {
                // Pull queries don't support LIMIT clauses. See https://cnfl.io/queries for more info.
                break;
            }

            currentCount++;

            await responseStream.WriteAsync(new OrderReply
            {
                Success = true,
                OrderId = response.orderId,
                OrderCreated = Timestamp.FromDateTimeOffset(DateTimeOffset.FromUnixTimeMilliseconds(response.orderCreated))
            });
        }
    }
}