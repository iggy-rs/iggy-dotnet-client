using Iggy_SDK;
using Iggy_SDK.Contracts.Http;

namespace Iggy_SDK_Tests.Utils.Partitions;

public static class PartitionFactory
{
	public static CreatePartitionsRequest CreatePartitionsRequest()
	{
        var streamId = Identifier.Numeric(1);
        var topicId = Identifier.Numeric(1);
		return new CreatePartitionsRequest
		{
			StreamId = streamId,
			TopicId = topicId,
			PartitionsCount = Random.Shared.Next(1, 69)
		};
	}
	public static DeletePartitionsRequest CreateDeletePartitionsRequest()
	{
        var streamId = Identifier.Numeric(1);
        var topicId = Identifier.Numeric(1);
		return new DeletePartitionsRequest
		{
			StreamId = streamId,
			TopicId = topicId,
			PartitionsCount = Random.Shared.Next(1, 69)
		};
	}
}