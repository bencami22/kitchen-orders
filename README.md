# Restaurant (kitchen orders)

Playground app utilizing .NET 6, GRPC, Apache Kafka, Apache Avro, KSQLDB.

<img width="1361" alt="Screenshot 2022-01-16 at 21 49 21" src="https://user-images.githubusercontent.com/26182143/149677636-3f5b846c-a8da-48d7-bc03-f0f8a140eb30.png">

Consisting of 2 hosted services, `Waiter` and `Kitchen`, mimicking a real life restaurant. 

An order is initiated by sending a GRPC request (proto: `Waiter.Order`) to the Waiter service. The `Waiter` forwards the request to the `Kitchen` (proto: `Kitchen.Order`). 

The kitchen will randomly determine whether an order is successful or not, if so, assigns an order Id and produces an `Order` Kafka message (topic: `order.created`) whilst also responding with the same data to the GRPC request, bubbling up back to the initiator.

A `Chit Service` exists in the `Waiter` which consumes the Kafka `Order` message and simply printing out the order to console.

A KSQL `Stream` is also attached to the `order.created` topic, which is then utilised when retrieving orders (`Waiter`- proto: `Waiter.GetOrders`) via a Pull Query.

## Run


```
docker compose up -d
```

> **Note:** Reserve ~8GB Memory in Docker.


### Schema Regsitry (avro) setup
Register the `order` schema (may not be necessarily due to auto create).
```sh
curl -X POST -H "Content-Type: application/vnd.schemaregistry.v1+json" \
  --data '{"schema":"{\"type\":\"record\",\"name\":\"Order\",\"doc\":\"Represents a successfully created kitchen order.\",\"namespace\":\"KitchenOrders.Messages\",\"fields\":[{\"name\":\"orderId\",\"type\":\"string\"},{\"name\":\"orderCreated\",\"type\":\"long\",\"logicalType\":\"timestamp-millis\"}]}"}' \
  http://localhost:8081/subjects/order_created/versions
```

### KSQLDB setup
`docker exec -it {ksqldb-cli-container-id} /bin/sh`

Confirm if connection can be made to the server:
`nc -zv ksqldb-server 8088`

Setup KSQL stream (via `ksqldb-cli`):-

First connect to server
```sh
ksql http://ksqldb-server:8088
```
then

Create stream with will contain the individual immutable messages from the kafka topic.

```sql
CREATE STREAM orders (orderId VARCHAR, orderCreated BIGINT, productId VARCHAR, quantity INT) WITH (kafka_topic='order.created', value_format='avro', partitions=1);
```

Create a materialised view/cache with will contain an aggregation of product/orders. Processing (aggregation) happens on each stream entry for faster querying.

```sql
CREATE TABLE product_orders AS
   SELECT productId,
          count_distinct(orderId) as noOfOrders,
          count_distinct(orderId) * SUM(quantity) AS totalQuantity,
          latest_by_offset(orderId) AS lastOrderId
    FROM orders
    GROUP BY productId
    EMIT CHANGES;
```
### Run services

Terminal #1
```sh
cd KitchenOrders/WaiterService/      
dotnet run
```

Terminal #2
```sh
cd KitchenOrders/KitchenService/      
dotnet run
```


## Avro

To create C# class from avro schema file:
```sh
dotnet tool install --global Apache.Avro.Tools --version 1.11.0
```

```sh
avrogen -s Order.avsc .
```