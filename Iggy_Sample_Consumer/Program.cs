using System.Text;
using System.Text.Json;
using Iggy_SDK.Contracts;
using Iggy_SDK.Enums;
using Iggy_SDK.Factory;
using Iggy_SDK.SerializationConfiguration;
using Shared;

var jsonOptions = new JsonSerializerOptions();
jsonOptions.PropertyNamingPolicy = new ToSnakeCaseNamingPolicy();
jsonOptions.WriteIndented = true;
var protocol = Protocol.Http;
var bus = MessageStreamFactory.CreateMessageStream(options =>
{
    options.BaseAdress = "http://localhost:3000";
    options.Protocol = protocol;
});

var streamId = 1;
var topicId = 1;
var partitionId = 3;
var consumerId = 1;

Console.WriteLine($"Consumer has started, selected protocol {protocol}");

await ValidateSystem(streamId, topicId, partitionId);
await ConsumeMessages();

async Task ConsumeMessages()
{
    int intervalInMs = 1000;
    Console.WriteLine($"Messages will be polled from stream {streamId}, topic {topicId}, partition {partitionId} with interval {intervalInMs} ms");
    while (true)
    {
        var messages = await bus.GetMessagesAsync(new MessageFetchRequest
        {
            Count = 1,
            TopicId = topicId,
            StreamId = streamId,
            ConsumerId = consumerId,
            PartitionId = partitionId,
            PollingStrategy = MessagePolling.Next,
            Value = 0,
            AutoCommit = true
        });
        if (!messages.Any())
        {
            Console.WriteLine("No messages were found");
            await Task.Delay(intervalInMs);
            continue;
        }

        foreach (var message in messages)
        {
            await HandleMessage(message);
        }

        await Task.Delay(intervalInMs);
    }
}

async Task HandleMessage(MessageResponse messageResponse)
{
    var bytes = Convert.FromBase64String(messageResponse.Payload);
    var json = Encoding.UTF8.GetString(bytes);
    var message = JsonSerializer.Deserialize<Envelope>(json);
    Console.Write($"Handling message type: {message!.MessageType} at offset: {messageResponse.Offset} ");

    switch (message.MessageType)
    {
        case "order_created":
        {
            var orderCreated = JsonSerializer.Deserialize<OrderCreated>(message.Payload, jsonOptions);
            Console.WriteLine(orderCreated);
            break;
        }
        case "order_confirmed":
        {
            var orderConfirmed = JsonSerializer.Deserialize<OrderConfirmed>(message.Payload, jsonOptions);
            Console.WriteLine(orderConfirmed);
            break;
        }
        case "order_rejected":
        {
            var orderRejected = JsonSerializer.Deserialize<OrderRejected>(message.Payload, jsonOptions);
            Console.WriteLine(orderRejected);
            break;
        }
    }
}

async Task ValidateSystem(int streamId, int topicId, int partitionId)
{
    Console.WriteLine($"Validating if stream exists.. {streamId}");
    var result = await bus.GetStreamByIdAsync(streamId);
    if (result is not null)
    {
        Console.WriteLine($"Stream {streamId} was found");
    }
    else
    {
        throw new SystemException($"Stream {streamId} was not found");
    }

    Console.WriteLine($"Validating if topic exists.. {topicId}");
    var topicResult = await bus.GetTopicByIdAsync(streamId, topicId);
    if (topicResult is not null)
    {
        Console.WriteLine($"Topic {topicId} was found");
    }
    else
    {
        throw new SystemException($"Topic {topicId} was not found");
    }

    if (topicResult.PartitionsCount < partitionId)
    {
        throw new SystemException($"Topic {topicId} has only {topicResult.PartitionsCount} partitions, but partition {partitionId} was requested");
    }
}

