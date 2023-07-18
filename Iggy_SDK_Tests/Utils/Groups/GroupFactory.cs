using Iggy_SDK.Contracts;
using Iggy_SDK.Contracts.Http;

namespace Iggy_SDK_Tests.Utils.Groups;

internal static class GroupFactory
{
	internal static GroupResponse CreateGroupResponse()
	{
		return new GroupResponse
		{
			Id = Random.Shared.Next(1, 10),
			MembersCount = Random.Shared.Next(1, 10),
			PartitionsCount = Random.Shared.Next(1, 10)
		};
	}

	internal static CreateGroupRequest CreateGroupRequest()
	{
		return new CreateGroupRequest
		{
			ConsumerGroupId = Random.Shared.Next(1, 10),
		};
	}

	internal static IEnumerable<GroupResponse> CreateGroupsResponse(int count)
	{
		return Enumerable.Range(1, count)
			.Select(x => new GroupResponse
				{ Id = Random.Shared.Next(1, 10), MembersCount = Random.Shared.Next(1, 10), PartitionsCount = Random.Shared.Next(1, 10) });
	}

	internal static IEnumerable<GroupResponse> Empty()
	{
		return Enumerable.Empty<GroupResponse>();
	}
}