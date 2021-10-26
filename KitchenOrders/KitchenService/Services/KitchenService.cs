using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using Grpc.Core;
using KitchenOrders.Messages;
using Microsoft.Extensions.Logging;
using Timestamp = Google.Protobuf.WellKnownTypes.Timestamp;

namespace KitchenOrders.Services
{
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

            var success = new Random().Next(1, 2) % 2 == 0;

            var orderReply = new OrderReply
            {
                Success = success,
                OrderId = Guid.NewGuid().ToString(),
                OrderCreated = Timestamp.FromDateTime(DateTime.UtcNow)
            };
            if (success)
            {
                await _orderProducer.ProduceAsync("orders",
                    new Message<Null, KitchenOrders.Messages.Order>
                    {
                        Value = new KitchenOrders.Messages.Order
                        {
                            orderId = orderReply.OrderId,
                            orderCreated = orderReply.OrderCreated.ToDateTimeOffset().ToUnixTimeMilliseconds()
                        }
                    });
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
}