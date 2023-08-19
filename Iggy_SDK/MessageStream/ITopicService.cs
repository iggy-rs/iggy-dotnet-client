using Iggy_SDK.Contracts.Http;

namespace Iggy_SDK.MessageStream;

public interface ITopicClient
{
	Task<List<TopicResponse>> GetTopicsAsync(Identifier streamId, CancellationToken token = default);
	Task<TopicResponse?> GetTopicByIdAsync(Identifier streamId, Identifier topicId, CancellationToken token = default);
	Task CreateTopicAsync(Identifier streamId, TopicRequest topic,CancellationToken token = default);
	Task DeleteTopicAsync(Identifier streamId, Identifier topicId, CancellationToken token = default);
}