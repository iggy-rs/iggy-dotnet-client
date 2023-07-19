using System.Text;
using System.Text.Json;
using Iggy_SDK.Contracts;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;
using Iggy_SDK.Factory;
using Iggy_SDK.SerializationConfiguration;
using Shared;

var jsonOptions = new JsonSerializerOptions();
jsonOptions.PropertyNamingPolicy = new ToSnakeCaseNamingPolicy();
jsonOptions.WriteIndented = true;
var protocol = Protocol.Tcp;
var bus = MessageStreamFactory.CreateMessageStream(options =>
{
    options.BaseAdress = "127.0.0.1:8090";
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
        try
        {
            var messages = (await bus.PollMessagesAsync(new MessageFetchRequest
            {
                Count = 1,
                TopicId = topicId,
                StreamId = streamId,
                ConsumerId = consumerId,
                PartitionId = partitionId,
                PollingStrategy = MessagePolling.Next,
                Value = 0,
                AutoCommit = true
            })).ToList();
            
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
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }
}

async Task HandleMessage(MessageResponse messageResponse)
{
    //this is giga inefficient, but its only a sample so who cares
    var length = (messageResponse.Payload.Length * 3) / 4;
    var bytes = new byte[length];
    var isBase64 = Convert.TryFromBase64Chars(messageResponse.Payload, bytes, out _);
    Envelope message = new Envelope();
    if (isBase64)
    {
        bytes = Convert.FromBase64String(messageResponse.Payload);
        var json = Encoding.UTF8.GetString(bytes);
        message = JsonSerializer.Deserialize<Envelope>(json);
    }
    else
    {
        message = JsonSerializer.Deserialize<Envelope>(messageResponse.Payload, jsonOptions);
    }
    
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
    try
    {
        Console.WriteLine($"Validating if stream exists.. {streamId}");
        var result = await bus.GetStreamByIdAsync(streamId);
        Console.WriteLine($"Validating if topic exists.. {topicId}");
        var topicResult = await bus.GetTopicByIdAsync(streamId, topicId);
        if (topicResult.PartitionsCount < partitionId)
        {
            throw new SystemException(
                $"Topic {topicId} has only {topicResult.PartitionsCount} partitions, but partition {partitionId} was requested");
        }
    }
    catch
    {
        
        Console.WriteLine($"Creating stream with {streamId}");
        await bus.CreateStreamAsync(new StreamRequest
        {
            StreamId = streamId,
            Name = "Test Consumer Stream",
        });
        Console.WriteLine($"Creating topic with {topicId}");
        await bus.CreateTopicAsync(streamId, new TopicRequest
        {
            Name = "Test Consumer Topic",
            PartitionsCount = 12,
            TopicId = topicId
        });
        var topicRes = await bus.GetTopicByIdAsync(streamId, topicId);
        if (topicRes.PartitionsCount < partitionId)
        {
            throw new SystemException(
                $"Topic {topicId} has only {topicRes.PartitionsCount} partitions, but partition {partitionId} was requested");
        }
    }

}

