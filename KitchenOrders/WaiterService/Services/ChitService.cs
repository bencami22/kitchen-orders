using Confluent.Kafka;
using KitchenOrders.Messages;

namespace WaiterService.Services;

/// <summary>
/// A Kafka consumer listening to all 'orders.created' and prints them to console.
/// </summary>
public class ChitService
{
    public ChitService(ILogger<ChitService> logger, IConsumer<Null, Order> orderConsumer)
    {
        Task.Run(() =>
        {
            try
            {
                orderConsumer.Subscribe("order.created");

                while (true)
                {
                    var consumeResult = orderConsumer.Consume(TimeSpan.FromMilliseconds(1000));

                    if (consumeResult != null)
                    {
                        logger.LogInformation("Consumer a kafka message: {@Message}", consumeResult);

                        Console.WriteLine("New order created:" +
                                          $"Id:{consumeResult?.Message.Value.orderId}" +
                                          $"Created At:{consumeResult?.Message.Value.orderCreated}");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in Kafka consumer");
            }
            finally
            {
                orderConsumer.Close();
            }
        });
    }
}