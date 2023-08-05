using System.Diagnostics;
using System.Text;
using Benchmarks;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;
using Iggy_SDK.Factory;
using Iggy_SDK.Identifiers;
using Iggy_SDK.Messages;

const int messagesCount = 1000;
const int messagesBatch = 1000;
const int messageSize = 69;
const int producerCount = 7;
const int startingStreamId = 100;
const int topicId = 1;

var bus = MessageStreamFactory.CreateMessageStream(options =>
{
	options.BaseAdress = "127.0.0.1:8090";
	options.Protocol = Protocol.Tcp;
	options.ReceiveBufferSize = Int32.MaxValue;
	options.SendBufferSize = Int32.MaxValue;
});



//store offset
// await bus.StoreOffsetAsync(Identifier.Numeric(1), Identifier.Numeric(1), new OffsetContract
// {
// 	Offset = 0,
// 	ConsumerId = 1,
// 	ConsumerType = ConsumerType.Consumer,
// 	PartitionId = 1,
// });
//get offset
// var offset = await bus.GetOffsetAsync(new OffsetRequest
// {
// 	ConsumerId = 1,
// 	ConsumerType = ConsumerType.Consumer,
// 	PartitionId = 1,
// 	StreamId = Identifier.Numeric(1),
// 	TopicId = Identifier.Numeric(1)
// });
// Console.WriteLine(offset.Offset);
//create consumer group
// await bus.CreateConsumerGroupAsync(Identifier.Numeric(1), Identifier.Numeric(1), new CreateConsumerGroupRequest
// {
// 	ConsumerGroupId = 1,
// });
//get consumer groups
//var groups = await bus.GetConsumerGroupsAsync(Identifier.String("my-stream"), Identifier.String("my-topic"));
//get consumer group by id
// var group = await bus.GetConsumerGroupByIdAsync(Identifier.String("my-stream"), Identifier.String("my-topic"), 1);
// Console.WriteLine(group.Id);
//create partitions
// await bus.CreatePartitionsAsync(Identifier.Numeric(1), Identifier.String("my-topic"), new CreatePartitionsRequest
// {
// 	PartitionsCount = 5,
// });
//delete partitions 
// await bus.DeletePartitionsAsync(Identifier.String("my-stream"), Identifier.Numeric(1), new DeletePartitionsRequest
// {
// 	PartitionsCount = 5,
// });
/*
await bus.SendMessagesAsync(Identifier.Numeric(1), Identifier.String("my-topic"), new MessageSendRequest
{
	Key = Key.PartitionId(1),
	Messages = Enumerable.Range(0, 3).Select(_ => new Message
	{
		Id = Guid.NewGuid(),
		Payload = "TROLOLOLO"u8.ToArray()
	}).ToArray()
	});
var messages = await bus.PollMessagesAsync(new MessageFetchRequest
{
	Count = 2,
	ConsumerType = ConsumerType.Consumer,
	ConsumerId = 1,
	PartitionId = 1,
	PollingStrategy = MessagePolling.Offset,
	StreamId = Identifier.Numeric(1),
	TopicId = Identifier.Numeric(1),
	AutoCommit = true,
	Value = 0,
});
Console.WriteLine(messages.Count());
Console.WriteLine();
*/
/*try
{
	for (int i = 0; i < producerCount; i++)
	{
		await bus.CreateStreamAsync(new StreamRequest
		{
			Name = "Test bench stream",
			StreamId = startingStreamId + i
		});
		await bus.CreateTopicAsync(startingStreamId + i, new TopicRequest
		{
			Name = "Test bench topic",
			PartitionsCount = 1,
			TopicId = topicId
		});
	}
}
catch
{
	Console.WriteLine("Failed to create streams, they already exist.");
}

List<Task> tasks = new();
var valBytes = BitConverter.GetBytes(1);

for (int i = 0; i < producerCount; i++)
{
	tasks.Add(SendMessage.Create(bus, i, producerCount, messagesBatch, messagesCount, messageSize,
		startingStreamId + i,
		topicId));
}

await Task.WhenAll(tasks);

try
{
	for (int i = 0; i < producerCount; i++)
	{
		await bus.DeleteStreamAsync(startingStreamId + i);
	}

}
catch
{
	Console.WriteLine("Failed to delete streams");
}

Console.ReadLine();*/