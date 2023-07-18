using Iggy_SDK.Contracts;
using Iggy_SDK.Contracts.Http;

namespace Iggy_SDK.MessageStream;

public interface IMessageClient
{
	Task<bool> SendMessagesAsync(MessageSendRequest request);
	Task<IEnumerable<MessageResponse>> PollMessagesAsync(MessageFetchRequest request);
}