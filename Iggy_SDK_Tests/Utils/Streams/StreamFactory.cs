using Iggy_SDK_Tests.Utils.Topics;
using Iggy_SDK.Contracts;
using Iggy_SDK.Contracts.Http;

namespace Iggy_SDK_Tests.Utils.Streams;

internal static class StreamFactory
{
	
	internal static (int id, int topicsCount, ulong sizeBytes, ulong messagesCount, string name, ulong createdAt)
		CreateStreamsResponseFields()
	{
		int id = Random.Shared.Next(1,69);
		var topicsCount = Random.Shared.Next(1,69);
		var sizeBytes = (ulong)Random.Shared.Next(69, 42069);
		var messageCount = (ulong)Random.Shared.Next(2,3);
		var name = "Stream "+Random.Shared.Next(1,4);
		var createdAt = (ulong)Random.Shared.Next(69, 42069);
		return (id, topicsCount, sizeBytes, messageCount, name, createdAt);
	}
	internal static StreamRequest CreateStreamRequest()
	{
		return new StreamRequest
		{
			Name = "Test Topic" + Random.Shared.Next(1, 69),
			StreamId = Random.Shared.Next(1, 10),
		};
	}

	internal static StreamResponse CreateStreamResponse()
	{
		var responses = new List<TopicResponse>();
		var topicResponse = TopicFactory.CreateTopicsResponse();
		responses.Add(topicResponse);
		return new StreamResponse
		{
			Id = Random.Shared.Next(1, 10),
			SizeBytes = (ulong)Random.Shared.Next(1, 10),
			MessagesCount = (ulong)Random.Shared.Next(1, 10),
			Name = "Test Topic" + Random.Shared.Next(1, 69),
			CreatedAt = DateTimeOffset.UtcNow,
			TopicsCount = Random.Shared.Next(1, 10),
			Topics = responses
		};
	}
	internal static StreamResponse CreateStreamsResponse()
	{
		return new StreamResponse
		{
			Id = Random.Shared.Next(1, 10),
			SizeBytes = (ulong)Random.Shared.Next(1, 10),
			MessagesCount = (ulong)Random.Shared.Next(1, 10),
			CreatedAt = DateTimeOffset.UtcNow,
			Name = "Test Topic" + Random.Shared.Next(1, 69),
			TopicsCount = Random.Shared.Next(1, 10),
		};
	}
}