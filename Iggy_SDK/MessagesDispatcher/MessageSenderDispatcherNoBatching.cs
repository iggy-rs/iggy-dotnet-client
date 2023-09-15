using Iggy_SDK.Contracts.Http;
using Microsoft.Extensions.Logging;
using System.Buffers.Binary;
using System.Threading.Channels;
namespace Iggy_SDK.MessagesDispatcher;

internal sealed class MessageSenderDispatcherNoBatching : MessageSenderDispatcher
{
    private readonly CancellationTokenSource _cts = new();
    private readonly MessageInvoker _messageInvoker;
    private readonly Channel<MessageSendRequest> _channel;
    private readonly ILogger<MessageSenderDispatcherNoBatching> _logger;

    internal MessageSenderDispatcherNoBatching(Channel<MessageSendRequest> channel,
        MessageInvoker messageInvoker, ILoggerFactory loggerFactory)
    {
        _messageInvoker = messageInvoker;
        _channel = channel;
        _logger = loggerFactory.CreateLogger<MessageSenderDispatcherNoBatching>();
    }
    internal override void Start()
    {
        Task.Run(async () => await SendMessages());
    }
    protected override async Task SendMessages()
    {
        while (!_cts.IsCancellationRequested)
        {
            await foreach (var request in _channel.Reader.ReadAllAsync())
            {
                try
                {
                    await _messageInvoker.SendMessagesAsync(request); 
                }
                catch
                {
                    var partId = BinaryPrimitives.ReadInt32LittleEndian(request.Partitioning.Value);
                    _logger.LogError("Error encountered while sending messages - Stream ID:{streamId}, Topic ID:{topicId}, Partition ID: {partitionId}",
                        request.StreamId, request.TopicId, partId); 
                }
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