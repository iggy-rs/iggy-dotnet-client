using Iggy_SDK.Contracts;
using Iggy_SDK.Contracts.Http;

namespace Iggy_SDK_Tests.Utils.Groups;

internal static class GroupFactory
{
	internal static ConsumerGroupResponse CreateGroupResponse()
	{
		return new ConsumerGroupResponse
		{
			Id = Random.Shared.Next(1, 10),
			MembersCount = Random.Shared.Next(1, 10),
			PartitionsCount = Random.Shared.Next(1, 10)
		};
	}

	internal static CreateConsumerGroupRequest CreateGroupRequest()
	{
		return new CreateConsumerGroupRequest
		{
			ConsumerGroupId = Random.Shared.Next(1, 10),
		};
	}

	internal static IEnumerable<ConsumerGroupResponse> CreateGroupsResponse(int count)
	{
		return Enumerable.Range(1, count)
			.Select(x => new ConsumerGroupResponse
				{ Id = Random.Shared.Next(1, 10), MembersCount = Random.Shared.Next(1, 10), PartitionsCount = Random.Shared.Next(1, 10) });
	}

	internal static IEnumerable<ConsumerGroupResponse> Empty()
	{
		return Enumerable.Empty<ConsumerGroupResponse>();
	}
}