using Iggy_SDK.Contracts;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Kinds;
using Iggy_SDK.Utils;

namespace Iggy_SDK.MessageStream;

public interface IConsumerGroupClient
{
	Task<IEnumerable<ConsumerGroupResponse>> GetConsumerGroupsAsync(Identifier streamId, Identifier topicId);
	Task<ConsumerGroupResponse?> GetConsumerGroupByIdAsync(Identifier streamId, Identifier topicId, int groupId);
	Task CreateConsumerGroupAsync(Identifier streamId, Identifier topicId, CreateConsumerGroupRequest request);
	Task DeleteConsumerGroupAsync(Identifier streamId, Identifier topicId, int groupId);
	Task JoinConsumerGroupAsync(JoinConsumerGroupRequest request);
	Task LeaveConsumerGroupAsync(LeaveConsumerGroupRequest request);

}