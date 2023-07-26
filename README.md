

# C# SDK for [Iggy](https://github.com/spetz/iggy)

### Getting Started
The whole SDK revolves around `IMessageStream` interface to create an instance of it, use following code
```c#
var bus = MessageStreamFactory.CreateMessageStream(x =>
{
    x.Protocol = Protocol.Http;
    x.BaseAddress = "http://localhost:8080";
});

```
Currently supported transfer protocols
-  TCP
-  HTTP

### Creating first stream
In order to create stream call `CreateStreamAsync` method
```c#
bus.CreateStreamAsync(new StreamRequest
{
    Name = "First Stream",
    StreamId = 1,
});

```
Every stream has a topic to which you can broadcast messages, to create a topic
use `CreateTopicAsync` method
```c#
bus.CreateTopicAsync(streamId, new TopicRequest
{
    Name = "First Topic",
    PartitionsCount = 3,
    TopicId = 1
});

```
To send messages you can use `SendMessagesAsync` method.
```c#
var messages = new List<Message>();
await bus.SendMessagesAsync(streamId, topicId, new MessageSendRequest
{
    Messages = messages,
    Key = new Key
    {
        Kind = KeyKind.PartitionId,
        Length = 4,
        Value = BitConverter.GetBytes(partitionId),
    }
});
```
The `Message` struct has two fields `Id` and `Payload`
```c#
struct Message
{
    public Guid Id { get; set; }
    public byte[] Payload { get; set; }
}
```

Polling messages is done with `PollMessagesAsync` 
```c#
var messages = await bus.PollMessagesAsync(new MessageFetchRequest
{
    Count = 1,
    ConsumerId = 1,
    PartitionId = 1,
    PollingStrategy = MessagePolling.Next,
    ConsumerType = ConsumerType.Consumer,
    Value = 0,
    AutoCommit = true
});
```
It is worth noting that every method will throw an `InvalidResponseException` when encountering an error.<br><br>
If you register `IMessageStream` in a dependency injection container, you will have access to interfaces
that encapsulate smaller parts of the system `IStreamClient` `ITopicClient` `IMessageClient` `IOffsetClient` `IConsumerGroupClient` `IUtilsClient`

For more information about how Iggy works check its [documentation](https://docs.iggy.rs/)

# Producer / Consumer Sample

https://github.com/iggy-rs/iggy-dotnet-client/assets/112548209/3a89d2f5-d066-40d2-8b82-96c3e338007e

To run the samples, first get [Iggy](https://github.com/spetz/iggy), Run the server with `cargo r --bin server`, then get the SDK, cd into `Iggy_SDK`
and run following commands: `dotnet run -c Release --project Iggy_Sample_Producer` for producer, `dotnet run -c Release --project Iggy_Sample_Consumer`
for consumer.

## TODO 
- Add support for `ASP.NET Core` Dependency Injection




