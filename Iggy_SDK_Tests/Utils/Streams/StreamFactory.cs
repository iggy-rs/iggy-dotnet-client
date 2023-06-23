using Iggy_SDK_Tests.Utils.Topics;
using Iggy_SDK.Contracts;

namespace Iggy_SDK_Tests.Utils.Streams;

public static class StreamFactory
{
	public static CreateStreamRequest CreateStreamRequest()
	{
		return new CreateStreamRequest
		{
			Name = "Test Topic" + Random.Shared.Next(1, 69),
			StreamId = Random.Shared.Next(1, 10),
		};
	}

	public static StreamResponse CreateStreamResponse()
	{
		var responses = new List<TopicsResponse>();
		var topicResponse = TopicFactory.CreateTopicsResponse();
		responses.Add(topicResponse);
		return new StreamResponse
		{
			Id = Random.Shared.Next(1, 10),
			Name = "Test Topic" + Random.Shared.Next(1, 69),
			TopicsCount = Random.Shared.Next(1, 10),
			Topics = responses
		};
	}
	public static StreamsResponse CreateStreamsResponse()
	{
		return new StreamsResponse
		{
			Id = Random.Shared.Next(1, 10),
			Name = "Test Topic" + Random.Shared.Next(1, 69),
			TopicsCount = Random.Shared.Next(1, 10),
		};
	}
}