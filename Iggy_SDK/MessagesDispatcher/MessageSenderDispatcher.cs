namespace Iggy_SDK.MessagesDispatcher;

internal abstract class MessageSenderDispatcher
{
    internal abstract void Start();
    protected abstract Task SendMessages();
    internal abstract Task StopAsync();
}