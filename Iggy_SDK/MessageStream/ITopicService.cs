using Iggy_SDK.Contracts.Http;

namespace Iggy_SDK.MessageStream;

public interface ITopicClient
{
	Task<IEnumerable<TopicResponse>> GetTopicsAsync(Identifier streamId);
	Task<TopicResponse?> GetTopicByIdAsync(Identifier streamId, Identifier topicId);
	Task CreateTopicAsync(Identifier streamId, TopicRequest topic);
	Task DeleteTopicAsync(Identifier streamId, Identifier topicId);
}