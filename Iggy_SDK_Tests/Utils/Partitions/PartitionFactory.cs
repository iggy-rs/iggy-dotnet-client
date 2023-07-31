using Iggy_SDK.Contracts.Http;

namespace Iggy_SDK_Tests.Utils.Partitions;

public static class PartitionFactory
{
	public static CreatePartitionsRequest CreatePartitionsRequest()
	{
		return new CreatePartitionsRequest
		{
			PartitionsCount = Random.Shared.Next(1, 69)
		};
	}
	public static DeletePartitionsRequest CreateDeletePartitionsRequest()
	{
		return new DeletePartitionsRequest
		{
			PartitionsCount = Random.Shared.Next(1, 69)
		};
	}
}