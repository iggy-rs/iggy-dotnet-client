using Iggy_SDK.Contracts;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Utils;

namespace Iggy_SDK.MessageStream;

public interface IConsumerGroupClient
{
	Task<IEnumerable<ConsumerGroupResponse>> GetConsumerGroupsAsync(int streamId, int topicId);
	Task<ConsumerGroupResponse?> GetConsumerGroupByIdAsync(int streamId, int topicId, int groupId);
	Task CreateConsumerGroupAsync(int streamId, int topicId, CreateConsumerGroupRequest request);
	Task DeleteConsumerGroupAsync(int streamId, int topicId, int groupId);
	Task JoinConsumerGroupAsync(JoinConsumerGroupRequest request);
	Task LeaveConsumerGroupAsync(LeaveConsumerGroupRequest request);

}