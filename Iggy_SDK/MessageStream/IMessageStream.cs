using Iggy_SDK.Contracts;
using Iggy_SDK.Enums;
using Iggy_SDK.Messages;

namespace Iggy_SDK.MessageStream;

public interface IMessageStream
{
    Task<bool> CreateStreamAsync(CreateStreamRequest request);
    Task<StreamResponse?> GetStreamByIdAsync(int streamId);
    Task<bool> CreateTopicAsync(int streamId, TopicRequest topic);
    Task<IEnumerable<TopicsResponse>> GetTopicsAsync(int streamId);
    Task<TopicsResponse?> GetTopicByIdAsync(int streamId, int topicId);
    //TODO Change function arguments to a discrete type
    Task<bool> SendMessagesAsync(int streamId, int topicId, Keykind keyKind, int keyValue, IEnumerable<IMessage> messages);
    Task<IEnumerable<MessageResponse>> GetMessagesAsync(MessageRequest request);
}