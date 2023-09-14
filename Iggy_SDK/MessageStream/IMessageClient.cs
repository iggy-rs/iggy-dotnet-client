using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Headers;
using Iggy_SDK.Kinds;

namespace Iggy_SDK.MessageStream;

public interface IMessageClient
{
    Task SendMessagesAsync(MessageSendRequest request, Func<byte[], byte[]>?
        encryptor = null, CancellationToken token = default);
    //TODO - should I create a MessageSendRequest<TMessage> to clean up function arguments ?
    Task SendMessagesAsync<TMessage>(Identifier streamId, Identifier topicId, Partitioning partitioning,
        IList<TMessage> messages, Func<TMessage, byte[]> serializer,
        Func<byte[], byte[]>? encryptor = null, Dictionary<HeaderKey, HeaderValue>? headers = null,
        CancellationToken token = default);
    Task<PolledMessages> FetchMessagesAsync(MessageFetchRequest request, Func<byte[], byte[]>? decryptor = null,
        CancellationToken token = default);
    Task<PolledMessages<TMessage>> FetchMessagesAsync<TMessage>(MessageFetchRequest request,
        Func<byte[], TMessage> deserializer, Func<byte[], byte[]>? decryptor = null, CancellationToken token = default);
    IAsyncEnumerable<MessageResponse<TMessage>> PollMessagesAsync<TMessage>(PollMessagesRequest request,
        Func<byte[], TMessage> deserializer, Func<byte[], byte[]>? decryptor = null, 
        Action<MessageFetchRequest>? logFetchingError = null,
        Action<StoreOffsetRequest>? logStoringOffset = null,
        CancellationToken token = default);

}