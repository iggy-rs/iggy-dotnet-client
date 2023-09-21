using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Headers;
using Iggy_SDK.Kinds;
namespace Iggy_SDK.MessageStream;

public interface IIggyPublisher
{
    Task SendMessagesAsync(MessageSendRequest request, Func<byte[], byte[]>?
        encryptor = null, CancellationToken token = default);
    //TODO - should I create a MessageSendRequest<TMessage> to clean up function arguments ?
    Task SendMessagesAsync<TMessage>(MessageSendRequest<TMessage> request,
         Func<TMessage, byte[]> serializer,
        Func<byte[], byte[]>? encryptor = null, Dictionary<HeaderKey, HeaderValue>? headers = null,
        CancellationToken token = default);
}