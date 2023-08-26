using Iggy_SDK;
using Iggy_SDK.Contracts;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Kinds;

namespace Iggy_SDK_Tests.Utils.Groups;

internal static class GroupFactory
{

	internal static (int id, int membersCount, int partitionsCount) CreateConsumerGroupResponseFields()
	{
        int id1 = Random.Shared.Next(1, 10);
        int membersCount1 = Random.Shared.Next(1, 10);
        int partitionsCount1 = Random.Shared.Next(1, 10);
		return (id1, membersCount1, partitionsCount1);
	}
	internal static ConsumerGroupResponse CreateGroupResponse()
	{
		return new ConsumerGroupResponse
		{
			Id = Random.Shared.Next(1, 10),
			MembersCount = Random.Shared.Next(1, 10),
			PartitionsCount = Random.Shared.Next(1, 10)
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

	internal static JoinConsumerGroupRequest CreateJoinGroupRequest()
	{
		return new JoinConsumerGroupRequest
		{
			StreamId = Identifier.Numeric(Random.Shared.Next(1,10)),
			ConsumerGroupId = Random.Shared.Next(1, 10),
			TopicId = Identifier.Numeric(Random.Shared.Next(1,10))
		};
	}

	internal static LeaveConsumerGroupRequest CreateLeaveGroupRequest()
	{
		return new LeaveConsumerGroupRequest
		{
			StreamId = Identifier.Numeric(Random.Shared.Next(1,10)),
			ConsumerGroupId = Random.Shared.Next(1, 10),
			TopicId = Identifier.Numeric(Random.Shared.Next(1,10))
		};
	}
}