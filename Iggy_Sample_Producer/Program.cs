using Iggy_Sample_Producer;
using Iggy_SDK.Contracts;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;
using Iggy_SDK.Factory;
using Iggy_SDK.Messages;
using Iggy_SDK.MessageStream;
using Shared;

var protocol = Protocol.Tcp;
var bus = MessageStreamFactory.CreateMessageStream(options =>
{
    options.BaseAdress = "127.0.0.1:8090";
    options.Protocol = protocol;
});

var streamId = 1;
var topicId = 1;

Console.WriteLine($"Producer has started, selected protocol {protocol.ToString()}");

var stream = await bus.GetStreamByIdAsync(streamId);
var topic = await bus.GetTopicByIdAsync(streamId, topicId);
if (stream is null)
{
    var result = await bus.CreateStreamAsync(new StreamRequest
    {
        StreamId = streamId,
        Name = "Test Producer Stream",
    });
    if (!result.IsSuccess)
    {
        throw new SystemException("Failed to create stream");
    }

    var topicResult = await bus.CreateTopicAsync(streamId, new TopicRequest
    {
        Name = "Test Topic From Producer Sample",
        PartitionsCount = 3,
        TopicId = topicId,
    });
    if (!topicResult.IsSuccess)
    {
        throw new SystemException("Failed to create topic");
    }

    stream = await bus.GetStreamByIdAsync(streamId);
    topic = await bus.GetTopicByIdAsync(streamId, topicId);
}

await ProduceMessages(bus, stream, topic);

async Task ProduceMessages(IMessageClient bus, StreamResponse? stream, TopicResponse? topic)
{
    var messageBatchCount = 1;
    int intervalInMs = 1000;
    Console.WriteLine($"Messages will be sent to stream {stream!.Id}, topic {topic!.Id}, partition {topic.PartitionsCount} with interval {intervalInMs} ms");

    while (true)
    {
        var debugMessages = new List<ISerializableMessage>();
        var messages = new List<IMessage>();
        
        for (int i = 0; i < messageBatchCount; i++)
        {
            var message = MessageGenerator.GenerateMessage();
            var json = message.ToJson();
            
            debugMessages.Add(message);
            messages.Add(new DummyMessage
            {
                Id = (ulong)i,
                Payload = json
            });
        }
        
        var result = await bus.SendMessagesAsync(new MessageSendRequest
        {
            Messages = messages,
            StreamId = stream.Id,
            TopicId = topic.Id,
            KeyKind = Keykind.PartitionId,
            KeyValue = topic.PartitionsCount,
        });
        Console.WriteLine($"Sent messages: {string.Join(Environment.NewLine, debugMessages.ConvertAll(m => m.ToString()))}");
        await Task.Delay(intervalInMs);
    }
}
