using Iggy_SDK.Contracts.Http;
namespace Iggy_SDK.MessagesDispatcher;

internal abstract class MessageInvoker
{
	internal abstract Task SendMessagesAsync(MessageSendRequest request,
		CancellationToken token = default);
}