using Iggy_SDK.Contracts;

namespace Iggy_SDK_Tests.Utils.Topics;

public static class TopicFactory
{
	public static TopicRequest CreateTopicRequest()
	{
		return new TopicRequest
		{
			Name = "Test Topic" + Random.Shared.Next(1,69),
			TopicId = Random.Shared.Next(1,10),
			PartitionsCount = Random.Shared.Next(1,10)
		};
	}

	public static TopicsResponse CreateTopicsResponse()
	{
		return new TopicsResponse
		{
			Id = Random.Shared.Next(1, 10),
			Name = "Test Topic" + Random.Shared.Next(1, 69),
			PartitionsCount = Random.Shared.Next(1, 10),
			Partitions = new List<PartitionContract>
			{
				new PartitionContract
				{
					Id = Random.Shared.Next(1, 10),
					CurrentOffset = Random.Shared.Next(1, 10),
					SegmentsCount = Random.Shared.Next(1, 10),
					SizeBytes = Random.Shared.Next(1, 10),
				}
			}

		};
	}
}
