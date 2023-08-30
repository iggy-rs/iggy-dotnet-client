using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Channels;
using Iggy_SDK.Configuration;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;
using Iggy_SDK.Messages;

namespace Iggy_SDK.MessagesDispatcher;

internal sealed class MessageSenderDispatcher
{
	private readonly PeriodicTimer _timer;
	private Task? _timerTask;
	private readonly CancellationTokenSource _cts = new();
	private readonly MessageInvoker _bus;
	private readonly Channel<MessageSendRequest> _channel;
	private readonly int _maxMessages;

	internal MessageSenderDispatcher(SendMessageConfigurator sendMessagesOptions, Channel<MessageSendRequest> channel,
		MessageInvoker bus)
	{
		_timer = new PeriodicTimer(sendMessagesOptions.PollingInterval);
		_bus = bus;
		_maxMessages = sendMessagesOptions.MaxMessagesPerBatch;
		_channel = channel;
	}

	internal void Start()
	{
		_timerTask = SendMessagesInBatchesAsync();
	}

	private async Task SendMessagesInBatchesAsync()
	{
		//TODO - this should match the bound limit of channel in the future
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
					await _bus.SendMessagesAsync(messagesSendRequests.Memory.Span[i], token: _cts.Token);
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
				await _bus.SendMessagesAsync(message, token: _cts.Token);
			}
			
		}
	}
	private static bool CanBatchMessages(Span<MessageSendRequest> requests)
	{
        ref var start = ref MemoryMarshal.GetReference(requests);
        ref var end = ref Unsafe.Add(ref start, requests.Length - 1);
        while (Unsafe.IsAddressLessThan(ref start, ref end))
        {
	        ref var next = ref Unsafe.Add(ref start, 1);

	        if (!start.StreamId.Equals(next.StreamId)
	            || !start.TopicId.Equals(next.TopicId)
	            || start.Partitioning.Kind is not PartitioningKind.PartitionId
	            || !start.Partitioning.Value.SequenceEqual(next.Partitioning.Value))
	        {
		        return false;
	        }

	        start = ref Unsafe.Add(ref start, 1);
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
