namespace Common.Kafka;

public class KafkaOptions
{
    public string BootstrapServers { get; set; } = default!;
    public string SchemaRegistryUrl { get; set; } = default!;
    public string KSqlDbUrl { get; set; } = default!;
}