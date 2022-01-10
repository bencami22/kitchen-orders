# kitchen-orders
.NET 5, Grpc, Apache Kafka, Apache Avro

![Add Order](https://ibb.co/jVNxyhQ)




curl -X POST -H "Content-Type: application/vnd.schemaregistry.v1+json" \
  --data '{"schema":"{\"type\":\"record\",\"name\":\"Order\",\"doc\":\"Represents a successfully created kitchen order.\",\"namespace\":\"KitchenOrders.Messages\",\"fields\":[{\"name\":\"orderId\",\"type\":\"string\"},{\"name\":\"orderCreated\",\"type\":\"long\",\"logicalType\":\"timestamp-millis\"}]}"}' \
  http://localhost:8081/subjects/order_created/versions

