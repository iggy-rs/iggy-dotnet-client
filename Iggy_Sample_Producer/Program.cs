using Iggy_Sample_Producer;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;
using Iggy_SDK.Factory;
using Iggy_SDK.Messages;
using Iggy_SDK.MessageStream;
using Shared;

var protocol = Protocol.Http;
var bus = MessageStreamFactory.CreateMessageStream(options =>
{
    options.BaseAdress = "http://127.0.0.1:3000";
    options.Protocol = protocol;
});

Console.WriteLine("Using protocol : {0}", protocol.ToString());
var streamId = 1;
var topicId = 1;

Console.WriteLine($"Producer has started, selected protocol {protocol.ToString()}");
try
{
    var stream = await bus.GetStreamByIdAsync(streamId);
    var topic = await bus.GetTopicByIdAsync(streamId, topicId);
}
catch
{
    Console.WriteLine($"Creating stream with id:{streamId}");
    await bus.CreateStreamAsync(new StreamRequest
    {
        StreamId = streamId,
        Name = "Test Producer Stream",
    });

    Console.WriteLine($"Creating topic with id:{topicId}");
    await bus.CreateTopicAsync(streamId, new TopicRequest
    {
        Name = "Test Topic From Producer Sample",
        PartitionsCount = 3,
        TopicId = topicId,
    });
}
var actualStream = await bus.GetStreamByIdAsync(streamId);
var actualTopic = await bus.GetTopicByIdAsync(streamId, topicId); 

await ProduceMessages(bus, actualStream, actualTopic);

async Task ProduceMessages(IMessageClient bus, StreamResponse? stream, TopicResponse? topic)
{
    var messageBatchCount = 1;
    int intervalInMs = 1000;
    Console.WriteLine($"Messages will be sent to stream {stream!.Id}, topic {topic!.Id}, partition {topic.PartitionsCount} with interval {intervalInMs} ms");

    while (true)
    {
        var debugMessages = new List<ISerializableMessage>();
        var messages = new List<Message>();
        
        for (int i = 0; i < messageBatchCount; i++)
        {
            var message = MessageGenerator.GenerateMessage();
            var json = message.ToBytes();
            
            debugMessages.Add(message);
            messages.Add(new Message
            {
                Id = Guid.NewGuid(),
                Payload = json
            });
        }

        try
        {
            await bus.SendMessagesAsync(streamId,topicId, new MessageSendRequest
            {
                Messages = messages,
                KeyKind = Keykind.PartitionId,
                KeyValue = topic.PartitionsCount,
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
        Console.WriteLine($"Sent messages: {string.Join(Environment.NewLine, debugMessages.ConvertAll(m => m.ToString()))}");
        await Task.Delay(intervalInMs);
    }
}
