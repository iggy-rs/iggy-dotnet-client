using Iggy_SDK.Contracts.Http;

namespace Iggy_SDK.MessageStream;

public interface IConsumerGroupClient
{
	Task<List<ConsumerGroupResponse>> GetConsumerGroupsAsync(Identifier streamId, Identifier topicId, CancellationToken token = default);
	Task<ConsumerGroupResponse?> GetConsumerGroupByIdAsync(Identifier streamId, Identifier topicId, int groupId, CancellationToken token = default);
	Task CreateConsumerGroupAsync(Identifier streamId, Identifier topicId, CreateConsumerGroupRequest request, CancellationToken token = default);
	Task DeleteConsumerGroupAsync(Identifier streamId, Identifier topicId, int groupId, CancellationToken token = default);
	Task JoinConsumerGroupAsync(JoinConsumerGroupRequest request, CancellationToken token = default);
	Task LeaveConsumerGroupAsync(LeaveConsumerGroupRequest request, CancellationToken token = default);

}