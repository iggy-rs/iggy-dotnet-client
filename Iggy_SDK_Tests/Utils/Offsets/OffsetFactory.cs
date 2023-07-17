using Iggy_SDK.Contracts;

namespace Iggy_SDK_Tests.Utils.Offset;

internal static class OffsetFactory
{
	internal static OffsetRequest CreateOffsetRequest()
	{
		return new OffsetRequest
		{
			TopicId = Random.Shared.Next(1, 10),
			PartitionId = Random.Shared.Next(1, 10),
			ConsumerId = Random.Shared.Next(1, 10),
			StreamId = Random.Shared.Next(1, 10),
		};
	}

	internal static OffsetResponse CreateOffsetResponse()
	{
		return new OffsetResponse
		{
			Offset = Random.Shared.Next(1, 10),
			ConsumerId = Random.Shared.Next(1, 10),
		};
	}

	internal static OffsetContract CreateOffsetContract()
	{
		return new OffsetContract
		{
			Offset = (ulong)Random.Shared.Next(1, 10),
			ConsumerId = Random.Shared.Next(1, 10),
			PartitionId = Random.Shared.Next(1, 10),
		};
	}
}