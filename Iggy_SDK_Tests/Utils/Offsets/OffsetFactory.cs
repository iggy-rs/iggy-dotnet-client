using Iggy_SDK;
using Iggy_SDK.Contracts;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;
using Iggy_SDK.Kinds;

namespace Iggy_SDK_Tests.Utils.Offset;

internal static class OffsetFactory
{
	internal static OffsetRequest CreateOffsetRequest()
	{
		return new OffsetRequest
		{
			Consumer = Consumer.New(1),
			TopicId = Identifier.Numeric(Random.Shared.Next(1, 10)),
			PartitionId = Random.Shared.Next(1, 10),
			StreamId = Identifier.Numeric(Random.Shared.Next(1, 10)),
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

	internal static StoreOffsetRequest CreateOffsetContract()
	{
		return new StoreOffsetRequest
		{
			Consumer = Consumer.New(1),
			Offset = (ulong)Random.Shared.Next(1, 10),
			PartitionId = Random.Shared.Next(1, 10),
		};
	}
}