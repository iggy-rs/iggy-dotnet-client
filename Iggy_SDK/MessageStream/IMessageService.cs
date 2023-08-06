using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Identifiers;

namespace Iggy_SDK.MessageStream;

public interface IMessageClient
{
	Task SendMessagesAsync(Identifier streamId, Identifier topicId, MessageSendRequest request);
	Task SendMessagesAsync<TMessage>(Identifier streamId, Identifier topicId, Partitioning partitioning,
		ICollection<TMessage> messages, Func<TMessage, byte[]> serializer);
	Task<IEnumerable<MessageResponse>> PollMessagesAsync(MessageFetchRequest request);
	Task<IEnumerable<MessageResponse<TMessage>>> PollMessagesAsync<TMessage>(MessageFetchRequest request,
		Func<byte[], TMessage> serializer);
}