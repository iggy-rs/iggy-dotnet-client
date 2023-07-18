using Iggy_SDK.Contracts;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Utils;

namespace Iggy_SDK.MessageStream;

public interface IGroupClient
{
	Task<IEnumerable<GroupResponse>> GetGroupsAsync(int streamId, int topicId);
	Task<GroupResponse?> GetGroupByIdAsync(int streamId, int topicId, int groupId);
	Task<Result> CreateGroupAsync(int streamId, int topicId, CreateGroupRequest request);
	Task<Result> DeleteGroupAsync(int streamId, int topicId, int groupId);
}