using Iggy_SDK.Contracts.Http;
using System.Threading.Channels;
namespace Iggy_SDK.MessagesDispatcher;

internal sealed class MessageSenderDispatcherNoBatching : MessageSenderDispatcher
{
    private readonly CancellationTokenSource _cts = new();
    private readonly MessageInvoker _messageInvoker;
    private readonly Channel<MessageSendRequest> _channel;

    internal MessageSenderDispatcherNoBatching(Channel<MessageSendRequest> channel,
        MessageInvoker messageInvoker)
    {
        _messageInvoker = messageInvoker;
        _channel = channel;
    }
    internal override void Start()
    {
        Task.Run(async () => await SendMessages());
    }
    //TODO - currently when SendMessagesAsync throws, whole program crashes,
    //handle errors silently and allow user provide an delegate
    //that allows logging the error
    protected override async Task SendMessages()
    {
        while (!_cts.IsCancellationRequested)
        {
            await foreach (var request in _channel.Reader.ReadAllAsync())
            {
                await _messageInvoker.SendMessagesAsync(request); 
            }
        }
    }
    internal override Task StopAsync()
    {
        _channel.Writer.Complete();
        _cts.Cancel();
        _cts.Dispose();
        return Task.CompletedTask;
    }
}