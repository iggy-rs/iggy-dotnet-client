using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Headers;
using Iggy_SDK.Kinds;

namespace Iggy_SDK.MessageStream;

//TODO - look into creating another overload for PollMessages method that will use IAsyncEnumerable as return type.
public interface IMessageClient
{
    Task SendMessagesAsync(MessageSendRequest request, Func<byte[], byte[]>?
        encryptor = null, CancellationToken token = default);
    //TODO - should I create a MessageSendRequest<TMessage> to clean up function arguments ?
    Task SendMessagesAsync<TMessage>(Identifier streamId, Identifier topicId, Partitioning partitioning,
        IList<TMessage> messages, Func<TMessage, byte[]> serializer,
        Func<byte[], byte[]>? encryptor = null, Dictionary<HeaderKey, HeaderValue>? headers = null,
        CancellationToken token = default);
    Task<PolledMessages> PollMessagesAsync(MessageFetchRequest request, Func<byte[], byte[]>? decryptor = null,
        CancellationToken token = default);
    Task<PolledMessages<TMessage>> PollMessagesAsync<TMessage>(MessageFetchRequest request,
        Func<byte[], TMessage> serializer, Func<byte[], byte[]>? decryptor = null, CancellationToken token = default);

}