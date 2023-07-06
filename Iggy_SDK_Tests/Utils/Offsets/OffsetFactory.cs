using Iggy_SDK.Contracts;

namespace Iggy_SDK_Tests.Utils.Offset;

public static class OffsetFactory
{
	public static OffsetRequest CreateOffsetRequest()
	{
		return new OffsetRequest
		{
			TopicId = Random.Shared.Next(1, 10),
			PartitionId = Random.Shared.Next(1, 10),
			ConsumerId = Random.Shared.Next(1, 10),
			StreamId = Random.Shared.Next(1, 10),
		};
	}

	public static OffsetResponse CreateOffsetResponse()
	{
		return new OffsetResponse
		{
			Offset = Random.Shared.Next(1, 10),
			ConsumerId = Random.Shared.Next(1, 10),
		};
	}

	public static OffsetContract CreateOffsetContract()
	{
		return new OffsetContract
		{
			Offset = Random.Shared.Next(1, 10),
			ConsumerId = Random.Shared.Next(1, 10),
			PartitionId = Random.Shared.Next(1, 10),
		};
	}
}