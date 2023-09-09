using Iggy_SDK.Configuration;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;
using Iggy_SDK.Messages;
using System.Buffers;
using System.Threading.Channels;

namespace Iggy_SDK.MessagesDispatcher;

internal sealed class MessageSenderDispatcher
{
    private readonly PeriodicTimer _timer;
    private Task? _timerTask;
    private readonly CancellationTokenSource _cts = new();
    private readonly MessageInvoker _messageInvoker;
    private readonly Channel<MessageSendRequest> _channel;
    private readonly int _maxMessages;

    internal MessageSenderDispatcher(SendMessageConfigurator sendMessagesOptions, Channel<MessageSendRequest> channel,
        MessageInvoker messageInvoker)
    {
        _timer = new PeriodicTimer(sendMessagesOptions.PollingInterval);
        _messageInvoker = messageInvoker;
        _maxMessages = sendMessagesOptions.MaxMessagesPerBatch;
        _channel = channel;
    }

    internal void Start()
    {
        _timerTask = SendMessagesInBatchesAsync();
    }

    //TODO - for non batching mode (PollingInterval = 0), should i introduce another flag to the while loop to make it infinite ?
    //or make another Dispatcher that would use IAsyncEnumerable read from the channel.
    private async Task SendMessagesInBatchesAsync()
    {
        //TODO - move this inside of the while loop and use the channel count to determine size of the buffer
        var messagesSendRequests = MemoryPool<MessageSendRequest>.Shared.Rent(1024);
        while (await _timer.WaitForNextTickAsync(_cts.Token))
        {
            int idx = 0;
            while (_channel.Reader.TryRead(out var msg))
            {
                messagesSendRequests.Memory.Span[idx++] = msg;
            }

            if (idx == 0)
            {
                continue;
            }

            var canBatchMessages = CanBatchMessages(messagesSendRequests.Memory.Span[..idx]);
            if (!canBatchMessages)
            {
                for (int i = 0; i < messagesSendRequests.Memory.Length; i++)
                {
                    await _messageInvoker.SendMessagesAsync(messagesSendRequests.Memory.Span[i], token: _cts.Token);
                }

                continue;
            }

            var messagesBatches = BatchMessages(messagesSendRequests.Memory.Span[..idx]);
            foreach (var message in messagesBatches)
            {
                if (message is null)
                {
                    break;
                }
                await _messageInvoker.SendMessagesAsync(message, token: _cts.Token);
            }
        }
    }
    private static bool CanBatchMessages(Span<MessageSendRequest> requests)
    {
        for (int i = 0; i < requests.Length - 1; i++)
        {
            var start = requests[i];
            var next = requests[i + 1];

            if (!start.StreamId.Equals(next.StreamId)
                || !start.TopicId.Equals(next.TopicId)
                || start.Partitioning.Kind is not PartitioningKind.PartitionId
                || !start.Partitioning.Value.SequenceEqual(next.Partitioning.Value))
            {
                return false;
            }
        }
        return true;
    }

    //I return the whole rented buffer, therefore there will be elements that are not filled (nulls)
    private MessageSendRequest?[] BatchMessages(Span<MessageSendRequest> requests)
    {
        int messagesCount = 0;
        for (int i = 0; i < requests.Length; i++)
        {
            messagesCount += requests[i].Messages.Count;
        }
        int batchesCount = (int)Math.Ceiling((decimal)messagesCount / _maxMessages);

        var messagesBuffer = ArrayPool<Message>.Shared.Rent(_maxMessages);
        var messages = messagesBuffer.AsSpan()[.._maxMessages];
        var messagesBatches = ArrayPool<MessageSendRequest>.Shared.Rent(batchesCount);

        int idx = 0;
        int batchCounter = 0;
        try
        {
            foreach (var request in requests)
            {
                foreach (var message in request.Messages)
                {
                    messages[idx++] = message;
                    if (idx >= _maxMessages)
                    {
                        var messageSendRequest = new MessageSendRequest
                        {
                            Partitioning = request.Partitioning,
                            StreamId = request.StreamId,
                            TopicId = request.TopicId,
                            Messages = messages.ToArray()
                        };
                        messagesBatches[batchCounter] = messageSendRequest;
                        batchCounter++;
                        idx = 0;
                        messages.Clear();
                    }
                }
            }

            if (!messages.IsEmpty)
            {
                var messageSendRequest = new MessageSendRequest
                {
                    Partitioning = requests[0].Partitioning,
                    StreamId = requests[0].StreamId,
                    TopicId = requests[0].TopicId,
                    Messages = messages[..idx].ToArray()
                };
                messagesBatches[batchCounter] = messageSendRequest;
            }
        }
        finally
        {
            ArrayPool<Message>.Shared.Return(messagesBuffer);
            ArrayPool<MessageSendRequest>.Shared.Return(messagesBatches);
        }

        return messagesBatches;
    }
    public async Task StopAsync()
    {
        if (_timerTask is null)
        {
            return;
        }
        _timer.Dispose();
        _cts.Cancel();
        await _timerTask;
        _cts.Dispose();
    }

}