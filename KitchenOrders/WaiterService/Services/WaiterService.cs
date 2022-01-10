using Grpc.Net.Client;

namespace KitchenOrders.Services;

public class WaiterService : Waiter.WaiterBase
{
    private readonly ILogger<WaiterService> _logger;

    public WaiterService(ILogger<WaiterService> logger)
    {
        _logger = logger;
    }

    public override async Task<OrderReply> Order(OrderRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Triggered: Order");

        using var channel = GrpcChannel.ForAddress("http://localhost:5001");
        var client = new Kitchen.KitchenClient(channel);
        var reply = await client.OrderAsync(request);

        return reply;
    }

    public override async Task GetOrders(GetOrdersRequest request,
        Grpc.Core.IServerStreamWriter<OrderReply> responseStream,
        Grpc.Core.ServerCallContext context)
    {
        _logger.LogInformation("Triggered: GetOrders");

        using var channel = GrpcChannel.ForAddress("http://localhost:5001");
        var client = new Kitchen.KitchenClient(channel);
        var reply = client.GetOrders(request);

        await foreach (var response in reply.ResponseStream.ReadAllAsync())
        {
            await responseStream.WriteAsync(response);
        }
    }
}