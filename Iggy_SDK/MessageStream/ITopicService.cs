using Iggy_SDK.Contracts;

namespace Iggy_SDK.MessageStream;

public interface ITopicService
{
	Task<IEnumerable<TopicsResponse>> GetTopicsAsync(int streamId);
	Task<TopicsResponse?> GetTopicByIdAsync(int streamId, int topicId);
	Task<bool> CreateTopicAsync(int streamId, TopicRequest topic);
	Task<bool> DeleteTopicAsync(int streamId, int topicId);
}