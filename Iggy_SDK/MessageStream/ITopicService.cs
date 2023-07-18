using Iggy_SDK.Contracts;
using Iggy_SDK.Contracts.Http;

namespace Iggy_SDK.MessageStream;

public interface ITopicClient
{
	Task<IEnumerable<TopicResponse>> GetTopicsAsync(int streamId);
	Task<TopicResponse?> GetTopicByIdAsync(int streamId, int topicId);
	Task<bool> CreateTopicAsync(int streamId, TopicRequest topic);
	Task<bool> DeleteTopicAsync(int streamId, int topicId);
}