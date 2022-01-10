using Common.Kafka;
using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class KafkaServiceExtension
{
    public static void AddProducer<TKey, TValue>(this IServiceCollection services, KafkaOptions config)
    {
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = config.BootstrapServers,
        };

        var schemaRegistry =
            new CachedSchemaRegistryClient(new SchemaRegistryConfig {Url = config.SchemaRegistryUrl});
        var producer = new ProducerBuilder<TKey, TValue>(producerConfig)
            .SetValueSerializer(new AvroSerializer<TValue>(schemaRegistry))
            .Build();

        services.AddSingleton(producer);
    }

    public static void AddConsumer<TKey, TValue>(this IServiceCollection services, KafkaOptions config, string groupId)
    {
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = config.BootstrapServers,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        var schemaRegistry =
            new CachedSchemaRegistryClient(new SchemaRegistryConfig {Url = config.SchemaRegistryUrl});

        var consumer = new ConsumerBuilder<TKey, TValue>(consumerConfig)
            .SetValueDeserializer(new AvroDeserializer<TValue>(schemaRegistry).AsSyncOverAsync())
            .Build();

        services.AddSingleton(consumer);
    }
}