using Iggy_SDK.Contracts;
using Iggy_SDK.Messages;

namespace Iggy_SDK.MessageStream;

public interface IMessageStream
{
    Task<bool> CreateStreamAsync(CreateStreamRequest request);
    Task<StreamResponse?> GetStreamByIdAsync(int streamId);
    Task<bool> CreateTopicAsync(int streamId, TopicRequest topic);
    Task<IEnumerable<TopicsResponse>?> GetTopicsAsync(int streamId);
    Task<TopicsResponse?> GetTopicByIdAsync(int streamId, int topicId);
    Task<bool> SendMessagesAsync(int streamId, int topicId, string keyKind, int keyValue, IEnumerable<IMessage> messages);
}