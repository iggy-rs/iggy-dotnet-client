using System.Diagnostics;
using System.Text;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;
using Iggy_SDK.Factory;
using Iggy_SDK.Messages;

const int messagesCount = 1000;
const int messagesBatch = 1000;
const int messageSize = 69;
const int producerCount = 7;
const int streamId = 14;
const int topicId = 1;
ulong totalMessages = messagesBatch * messagesCount;
ulong totalMessagesBytes = totalMessages * messageSize;


var bus = MessageStreamFactory.CreateMessageStream(options =>
{
	options.BaseAdress = "127.0.0.1:8090";
	options.Protocol = Protocol.Tcp;
	options.ReceiveBufferSize = Int32.MaxValue;
	options.SendBufferSize = Int32.MaxValue;
});

try
{
	var stream = await bus.GetStreamByIdAsync(streamId);
}
catch
{
	await bus.CreateStreamAsync(new StreamRequest
	{
		Name = "Test bench stream",
		StreamId = streamId
	});
	await bus.CreateTopicAsync(streamId, new TopicRequest
	{
		Name = "Test bench topic",
		PartitionsCount = 1,
		TopicId = topicId
	});
}


List<Message> messages = CreateMessages();
async Task SendMessages(int producerNumber)
{
	List<TimeSpan> latencies = new();
	var valBytes = BitConverter.GetBytes(1);

	for (int i = 0; i < messagesBatch; i++)
	{
		var startTime = Stopwatch.GetTimestamp();
		await bus.SendMessagesAsync(streamId, topicId, new MessageSendRequest
		{
			Key = new Key
			{
				Kind = KeyKind.PartitionId,
				Length = 4,
				Value = valBytes
			},
			Messages = messages,
		});
		var diff = Stopwatch.GetElapsedTime(startTime);
		latencies.Add(diff);
	}

	
	var totalLatencies = latencies.Sum(x => x.TotalSeconds);
	var avgLatency = Math.Round((totalLatencies * 1000) / (producerCount * latencies.Count), 2);
	var duration = totalLatencies / producerCount;
	
	Console.WriteLine($"Total message bytes: {totalMessagesBytes}, average latency: {avgLatency} ms");
	var avgThroughput = Math.Round(totalMessagesBytes / duration / 1024.0 / 1024.0, 2);
	Console.WriteLine(
		$"Producer number: {producerNumber} send Messages: {messagesCount} in {messagesBatch} batches, with average throughput {avgThroughput} MB/s");
	
}

Parallel.For(1, producerCount + 1, async iter =>
{
	Console.WriteLine($"Executing producer number: {iter}");
	await SendMessages(iter);
});
Console.ReadLine();

static List<Message> CreateMessages()
{
	var messages = new List<Message>();
	for (int i = 0; i < messagesCount; i++)
	{
		messages.Add(new Message
		{
			Id = Guid.NewGuid(),
			Payload = CreatePayload(messageSize)
		});	
	}

	return messages;
}

static byte[] CreatePayload(uint size)
{
	StringBuilder payloadBuilder = new StringBuilder((int)size);
	for (uint i = 0; i < size; i++)
	{
		char character = (char)((i % 26) + 97);
		payloadBuilder.Append(character);
	}

	string payloadString = payloadBuilder.ToString();
	return Encoding.ASCII.GetBytes(payloadString);
}