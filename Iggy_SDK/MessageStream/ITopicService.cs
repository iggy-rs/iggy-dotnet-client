using Iggy_SDK.Contracts;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Kinds;
using Iggy_SDK.Utils;

namespace Iggy_SDK.MessageStream;

public interface ITopicClient
{
	Task<IEnumerable<TopicResponse>> GetTopicsAsync(Identifier streamId);
	Task<TopicResponse?> GetTopicByIdAsync(Identifier streamId, Identifier topicId);
	Task CreateTopicAsync(Identifier streamId, TopicRequest topic);
	Task DeleteTopicAsync(Identifier streamId, Identifier topicId);
}