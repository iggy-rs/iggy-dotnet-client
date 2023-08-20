using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Kinds;

namespace Iggy_SDK.MessageStream;

//TODO - look into creating another overload for PollMessages method that will use IAsyncEnumerable as return type.
public interface IMessageClient
{
	Task SendMessagesAsync(Identifier streamId, Identifier topicId, MessageSendRequest request, Func<byte[], byte[]>?
		encryptor = null, CancellationToken token = default);
	Task SendMessagesAsync<TMessage>(Identifier streamId, Identifier topicId, Partitioning partitioning,
		IList<TMessage> messages, Func<TMessage, byte[]> serializer, Func<byte[], byte[]>? encryptor = null,
		CancellationToken token = default);
	Task<IReadOnlyList<MessageResponse>> PollMessagesAsync(MessageFetchRequest request, Func<byte[], byte[]>? decryptor = null,
		CancellationToken token = default);
	Task<IReadOnlyList<MessageResponse<TMessage>>> PollMessagesAsync<TMessage>(MessageFetchRequest request,
		Func<byte[], TMessage> serializer, Func<byte[], byte[]>? decryptor = null, CancellationToken token = default);
	
}