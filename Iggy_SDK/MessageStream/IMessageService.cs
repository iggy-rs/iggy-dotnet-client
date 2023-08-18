using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Kinds;

namespace Iggy_SDK.MessageStream;

public interface IMessageClient
{
	Task SendMessagesAsync(Identifier streamId, Identifier topicId, MessageSendRequest request, Func<byte[], byte[]>?
		encryptor = null);
	Task SendMessagesAsync<TMessage>(Identifier streamId, Identifier topicId, Partitioning partitioning,
		IList<TMessage> messages, Func<TMessage, byte[]> serializer, Func<byte[], byte[]>? encryptor = null);
	Task<List<MessageResponse>> PollMessagesAsync(MessageFetchRequest request, Func<byte[],byte[]>? decryptor = null);
	Task<List<MessageResponse<TMessage>>> PollMessagesAsync<TMessage>(MessageFetchRequest request,
		Func<byte[], TMessage> serializer, Func<byte[], byte[]>? decryptor = null);
	
}