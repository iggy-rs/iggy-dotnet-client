using Iggy_SDK.Contracts;

namespace Iggy_SDK.MessageStream;

public interface IGroupService
{
	Task<IEnumerable<GroupResponse>> GetGroupsAsync(int streamId, int topicId);
	Task<GroupResponse?> GetGroupByIdAsync(int streamId, int topicId, int groupId);
	Task<bool> CreateGroupAsync(int streamId, int topicId, GroupRequest request);
	Task<bool> DeleteGroupAsync(int streamId, int topicId, int groupId);
	//delete group
}