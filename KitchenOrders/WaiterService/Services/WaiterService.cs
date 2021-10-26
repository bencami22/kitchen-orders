using System;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using KitchenOrders.Repositories;
using Microsoft.Extensions.Logging;

namespace KitchenOrders.Services
{
    public class WaiterService : Waiter.WaiterBase
    {
        private readonly ILogger<WaiterService> _logger;
        private readonly IOrdersRepository _ordersRepository;

        public WaiterService(ILogger<WaiterService> logger, IOrdersRepository ordersRepository)
        {
            _logger = logger;
            _ordersRepository = ordersRepository;
        }

        public override async Task<OrderReply> Order(OrderRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Triggered: Order");
            using var channel = GrpcChannel.ForAddress("http://localhost:5001");
            var client = new Kitchen.KitchenClient(channel);
            var reply = client.Order(request);

            await _ordersRepository.AddAsync(reply);

            return reply;
        }

        public override async Task GetOrders(GetOrdersRequest request,
            Grpc.Core.IServerStreamWriter<OrderReply> responseStream,
            Grpc.Core.ServerCallContext context)
        {
            _logger.LogInformation("Triggered: Order");
            var orders = _ordersRepository.GetAsync(request.Limit);

            await foreach (var response in orders)
            {
                await responseStream.WriteAsync(response);
            }
        }
    }
}