using Iggy_SDK.Contracts;

namespace Iggy_SDK.MessageStream;

public interface IMessageService
{
	Task<bool> SendMessagesAsync(MessageSendRequest request);
	Task<IEnumerable<MessageResponse>> GetMessagesAsync(MessageFetchRequest request);
}