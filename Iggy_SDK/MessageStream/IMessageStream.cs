using Iggy_SDK.Contracts;

namespace Iggy_SDK.MessageStream;

public interface IMessageStream
{
    Task<bool> CreateStreamAsync(CreateStreamRequest request);
    Task<StreamResponse?> GetStreamByIdAsync(int streamId);
    Task<bool> CreateTopicAsync(int streamId, TopicRequest topic);
    Task<IEnumerable<TopicsResponse>> GetTopicsAsync(int streamId);
    Task<TopicsResponse?> GetTopicByIdAsync(int streamId, int topicId);
    Task<bool> SendMessagesAsync(MessageSendRequest request);
    Task<IEnumerable<MessageResponse>> GetMessagesAsync(MessageFetchRequest request);
}
