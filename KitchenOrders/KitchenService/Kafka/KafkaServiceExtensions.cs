using System;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using KitchenService.Kafka;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class KafkaServiceExtension
    {
        public static void AddProducer<TKey, TValue>(this IServiceCollection services, KafkaOptions config)
        {
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = config.BootstrapServers, ClientId = Environment.MachineName
            };

            using var schemaRegistry =
                new CachedSchemaRegistryClient(new SchemaRegistryConfig {Url = config.SchemaRegistryUrl});
            using var producer = new ProducerBuilder<Null, KitchenOrders.Messages.Order>(producerConfig)
                .SetValueSerializer(new AvroSerializer<KitchenOrders.Messages.Order>(schemaRegistry))
                .Build();
            services.AddSingleton(producer);
        }
    }
}