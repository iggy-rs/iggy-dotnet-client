using Iggy_SDK.Contracts;

namespace Iggy_SDK.MessageStream;

public interface ITopicService
{
	Task<IEnumerable<TopicResponse>> GetTopicsAsync(int streamId);
	Task<TopicResponse?> GetTopicByIdAsync(int streamId, int topicId);
	Task<bool> CreateTopicAsync(int streamId, TopicRequest topic);
	Task<bool> DeleteTopicAsync(int streamId, int topicId);
}