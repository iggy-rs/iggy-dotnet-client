using Iggy_SDK.Contracts;
using Iggy_SDK.Contracts.Http;

namespace Iggy_SDK_Tests.Utils.Topics;

internal static class TopicFactory
{
	internal static TopicRequest CreateTopicRequest()
	{
		return new TopicRequest
		{
			Name = "Test Topic" + Random.Shared.Next(1,69),
			TopicId = Random.Shared.Next(1,10),
			PartitionsCount = Random.Shared.Next(1,10)
		};
	}

	internal static TopicResponse CreateTopicsResponse()
	{
		return new TopicResponse
		{
			
			Id = Random.Shared.Next(1, 10),
			Name = "Test Topic" + Random.Shared.Next(1, 69),
			MessagesCount = (ulong)Random.Shared.Next(1, 10),
			PartitionsCount = Random.Shared.Next(1, 10),
			SizeBytes = (ulong)Random.Shared.Next(1, 10),
			Partitions = new List<PartitionContract>
			{
				new PartitionContract
				{
					MessagesCount = (ulong)Random.Shared.Next(1, 10),
					Id = Random.Shared.Next(1, 10),
					CurrentOffset = (ulong)Random.Shared.Next(1, 10),
					SegmentsCount = Random.Shared.Next(1, 10),
					SizeBytes = (ulong)Random.Shared.Next(1, 10),
				}
			}

		};
	}
}
