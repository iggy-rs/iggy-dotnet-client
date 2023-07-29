using Benchmarks;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;
using Iggy_SDK.Factory;

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

try
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

Console.ReadLine();