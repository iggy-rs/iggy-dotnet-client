using Iggy_SDK.Contracts;

namespace Iggy_SDK_Tests.Utils.Groups;

public static class GroupFactory
{
	public static GroupResponse CreateGroupResponse()
	{
		return new GroupResponse
		{
			Id = Random.Shared.Next(1, 10),
			MembersCount = Random.Shared.Next(1, 10),
		};
	}

	public static IEnumerable<GroupResponse> CreateGroupsResponse(int count)
	{
		return Enumerable.Range(1, count)
			.Select(x => new GroupResponse
				{ Id = Random.Shared.Next(1, 10), MembersCount = Random.Shared.Next(1, 10) });
	}

	public static IEnumerable<GroupResponse> Empty()
	{
		return Enumerable.Empty<GroupResponse>();
	}
}