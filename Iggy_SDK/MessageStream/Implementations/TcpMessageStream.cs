using System.Buffers;
using System.Buffers.Binary;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Contracts.Tcp;
using Iggy_SDK.Enums;
using Iggy_SDK.Exceptions;
using Iggy_SDK.Kinds;
using Iggy_SDK.Mappers;
using Iggy_SDK.Messages;
using Iggy_SDK.Utils;

namespace Iggy_SDK.MessageStream.Implementations;

//TODO - look into changing IEnumerable<T> response types to List<T>, or make them lazy
public sealed class TcpMessageStream : IMessageStream, IDisposable
{
	private const int INITIAL_BYTES_LENGTH = 4;
	private const int EXPECTED_RESPONSE_SIZE = 8;
	private readonly Socket _socket;

	private Memory<byte> _buffer = new(new byte[EXPECTED_RESPONSE_SIZE]);

	internal TcpMessageStream(Socket socket)
	{
		_socket = socket;
	}

	public async Task CreateStreamAsync(StreamRequest request)
	{
		var message = TcpContracts.CreateStream(request);
		var payload = new byte[4 + INITIAL_BYTES_LENGTH + message.Length];
		CreatePayload(payload, message,CommandCodes.CREATE_STREAM_CODE);

		await _socket.SendAsync(payload);

		var buffer = new byte[EXPECTED_RESPONSE_SIZE];
		await _socket.ReceiveAsync(buffer);

		var status = GetResponseStatus(buffer);
		if (status != 0)
		{
			throw new InvalidResponseException($"Invalid response status code: {status}");
		}
	}

	public async Task<StreamResponse?> GetStreamByIdAsync(Identifier streamId)
	{
		var message = GetBytesFromIdentifier(streamId);
		var payload = new byte[4 + INITIAL_BYTES_LENGTH + message.Length];
		CreatePayload(payload, message, CommandCodes.GET_STREAM_CODE);

		await _socket.SendAsync(payload);

		var buffer = new byte[EXPECTED_RESPONSE_SIZE];
		await _socket.ReceiveAsync(buffer);

		var response = GetResponseLengthAndStatus(buffer);
		var responseBuffer = new byte[response.Length];

		if (response.Status != 0)
		{
			throw new InvalidResponseException($"Invalid response status code: {response.Status}");
		}

		if (response.Length <= 1)
		{
			return null;	
		}

		await _socket.ReceiveAsync(responseBuffer);
		return BinaryMapper.MapStream(responseBuffer);
	}

	public async Task<IEnumerable<StreamResponse>> GetStreamsAsync()
	{
		var message = Enumerable.Empty<byte>().ToArray();
		var payload = new byte[4 + INITIAL_BYTES_LENGTH + message.Length];
		CreatePayload(payload, message, CommandCodes.GET_STREAMS_CODE);

		await _socket.SendAsync(payload);

		var buffer = new byte[EXPECTED_RESPONSE_SIZE];
		await _socket.ReceiveAsync(buffer);

		var response = GetResponseLengthAndStatus(buffer);
		var responseBuffer = new byte[response.Length];

		if (response.Status != 0)
		{
			throw new InvalidResponseException($"Invalid response status code: {response.Status}");
		}

		if (response.Length <= 1)
		{
			return Enumerable.Empty<StreamResponse>();
		}
		
		await _socket.ReceiveAsync(responseBuffer);
		return BinaryMapper.MapStreams(responseBuffer);
	}

	public async Task DeleteStreamAsync(Identifier streamId)
	{
		var message = GetBytesFromIdentifier(streamId);
		var payload = new byte[4 + INITIAL_BYTES_LENGTH + message.Length];
		CreatePayload(payload, message, CommandCodes.DELETE_STREAM_CODE);

		await _socket.SendAsync(payload);

		var buffer = new byte[EXPECTED_RESPONSE_SIZE];
		await _socket.ReceiveAsync(buffer);

		var status = GetResponseStatus(buffer);

		if (status != 0)
		{
			throw new InvalidResponseException($"Invalid response status code: {status}"); 
		}
	}

	public async Task<IEnumerable<TopicResponse>> GetTopicsAsync(Identifier streamId)
	{
		var message = GetBytesFromIdentifier(streamId);
		var payload = new byte[4 + INITIAL_BYTES_LENGTH + message.Length];
		CreatePayload(payload, message, CommandCodes.GET_TOPICS_CODE);

		await _socket.SendAsync(payload);

		var buffer = new byte[EXPECTED_RESPONSE_SIZE];
		await _socket.ReceiveAsync(buffer);

		var response = GetResponseLengthAndStatus(buffer);

		if (response.Status != 0)
		{
			throw new InvalidResponseException($"Invalid response status code: {response.Status}");
		}

		if (response.Length <= 1)
		{
			return Enumerable.Empty<TopicResponse>();
		}
		
		var responseBuffer = new byte[response.Length];
		await _socket.ReceiveAsync(responseBuffer);
		return BinaryMapper.MapTopics(responseBuffer);
	}

	public async Task<TopicResponse?> GetTopicByIdAsync(Identifier streamId, Identifier topicId)
	{
		var message = TcpContracts.GetTopicById(streamId, topicId);
		var payload = new byte[4 + INITIAL_BYTES_LENGTH + message.Length];
		CreatePayload(payload, message, CommandCodes.GET_TOPIC_CODE);

		await _socket.SendAsync(payload);

		var buffer = new byte[EXPECTED_RESPONSE_SIZE];
		await _socket.ReceiveAsync(buffer);

		var response = GetResponseLengthAndStatus(buffer);

		if (response.Status != 0)
		{
			throw new InvalidResponseException($"Invalid response status code: {response.Status}");
		}

		if (response.Length <= 1)
		{
			return null;
		}
		
		var responseBuffer = new byte[response.Length];
		await _socket.ReceiveAsync(responseBuffer);
		return BinaryMapper.MapTopic(responseBuffer);
	}


	public async Task CreateTopicAsync(Identifier streamId, TopicRequest topic)
	{
		var message = TcpContracts.CreateTopic(streamId, topic);
		var payload = new byte[4 + INITIAL_BYTES_LENGTH + message.Length];
		CreatePayload(payload, message, CommandCodes.CREATE_TOPIC_CODE);

		await _socket.SendAsync(payload);

		var buffer = new byte[EXPECTED_RESPONSE_SIZE];
		await _socket.ReceiveAsync(buffer);
		
		var status = GetResponseStatus(buffer); 
		
		if (status != 0)
		{
			throw new InvalidResponseException($"Invalid response status code: {status}");
		}
	}

	public async Task DeleteTopicAsync(Identifier streamId, Identifier topicId)
	{
		var message = TcpContracts.DeleteTopic(streamId, topicId);
		var payload = new byte[4 + INITIAL_BYTES_LENGTH + message.Length];
		CreatePayload(payload, message, CommandCodes.DELETE_TOPIC_CODE);

		await _socket.SendAsync(payload);

		var buffer = new byte[EXPECTED_RESPONSE_SIZE];
		await _socket.ReceiveAsync(buffer);
		
		var status = GetResponseStatus(buffer);
		
		if (status != 0)
		{
			throw new InvalidResponseException($"Invalid response status code: {status}");
		}
	}
	public async Task SendMessagesAsync(Identifier streamId, Identifier topicId, MessageSendRequest request,
		Func<byte[], byte[]>? encryptor = null)
	{
		//TODO - explore making fields of Message class mutable, so there is no need to create em from scratch
		var messages = request.Messages;
		if (encryptor is not null)
		{
			for (var i = 0; i < request.Messages.Count; i++)
			{
				messages[i] = messages[i] with { Payload = encryptor(messages[i].Payload) };
			}
		}
		
		var streamTopicIdLength = 2 + streamId.Length + 2 + topicId.Length;
		var messageBufferSize = CalculateMessageBytesCount(messages)
		               + request.Partitioning.Length + streamTopicIdLength + 2;
        var payloadBufferSize = messageBufferSize + 4 + INITIAL_BYTES_LENGTH;
        
		var message = ArrayPool<byte>.Shared.Rent(messageBufferSize);
		var payload = ArrayPool<byte>.Shared.Rent(payloadBufferSize);
		try
		{
			TcpContracts.CreateMessage(message.AsSpan()[..messageBufferSize], streamId, topicId, request.Partitioning,
				messages);
			CreatePayload(payload, message.AsSpan()[..messageBufferSize], CommandCodes.SEND_MESSAGES_CODE);

			var recv = _socket.ReceiveAsync(_buffer);
			await _socket.SendAsync(payload.AsMemory()[..payloadBufferSize]);
			
			await recv;
			
			var status = GetResponseStatus(_buffer.Span);
			if (status != 0)
			{
				throw new InvalidResponseException($"Invalid response status code: {status}");
			}
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(message);
			ArrayPool<byte>.Shared.Return(payload);
		}
		
	}

	public async Task SendMessagesAsync<TMessage>(Identifier streamId, Identifier topicId, Partitioning partitioning,
		IList<TMessage> messages, Func<TMessage, byte[]> serializer, Func<byte[], byte[]>? encryptor = null)
	{
		var messagesPool = ArrayPool<Message>.Shared.Rent(messages.Count);
		for (var i = 0; i < messages.Count; i++)
		{
			messagesPool[i] = new Message
			{
				Payload = encryptor is not null ?
					encryptor(serializer(messages[i])) : serializer(messages[i]),
				Id = Guid.NewGuid()
			};
		}

		var messagesToSend = messagesPool[..messages.Count];
		var msgBytesSum = CalculateMessageBytesCount(messagesToSend);

		var streamTopicIdLength = 2 + streamId.Length + 2 + topicId.Length;
		var messageBufferSize = msgBytesSum + partitioning.Length + streamTopicIdLength + 2;
        var payloadBufferSize = messageBufferSize + 4 + INITIAL_BYTES_LENGTH;
        
		var message = ArrayPool<byte>.Shared.Rent(messageBufferSize);
		var payload = ArrayPool<byte>.Shared.Rent(payloadBufferSize);
		try
		{
			TcpContracts.CreateMessage(message.AsSpan()[..messageBufferSize], streamId, topicId, partitioning,
				messagesToSend);
			CreatePayload(payload, message.AsSpan()[..messageBufferSize], CommandCodes.SEND_MESSAGES_CODE);

			var recv = _socket.ReceiveAsync(_buffer);
			await _socket.SendAsync(payload.AsMemory()[..payloadBufferSize]);
			
			await recv;
			
			var status = GetResponseStatus(_buffer.Span);
			if (status != 0)
			{
				throw new InvalidResponseException($"Invalid response status code: {status}");
			}
		}
		finally
		{
			ArrayPool<Message>.Shared.Return(messagesPool);
			ArrayPool<byte>.Shared.Return(message);
			ArrayPool<byte>.Shared.Return(payload);
		}
	}
	private static int CalculateMessageBytesCount(IEnumerable<Message> messages)
	{
		return messages switch
		{
			Message[] messagesArray => CalculateMessageBytesCountArray(messagesArray),
			List<Message> messagesList => CalculateMessageBytesCountList(messagesList),
			_ => messages.Sum(x => 16 + 4 + x.Payload.Length)
		};
	}
	private static int CalculateMessageBytesCountArray(Message[] messages)
	{
		ref var start = ref MemoryMarshal.GetArrayDataReference(messages);
		ref var end = ref Unsafe.Add(ref start, messages.Length);
		var msgBytesSum = 0;
		while (Unsafe.IsAddressLessThan(ref start, ref end))
		{
			msgBytesSum += start.Payload.Length + 16 + 4;
			start = ref Unsafe.Add(ref start, 1);
		}
		return msgBytesSum;
	}
	private static int CalculateMessageBytesCountList(List<Message> messages)
	{
		var messagesSpan = CollectionsMarshal.AsSpan(messages);
		ref var start = ref MemoryMarshal.GetReference(messagesSpan);
		ref var end = ref Unsafe.Add(ref start, messagesSpan.Length);
		var msgBytesSum = 0;
		while (Unsafe.IsAddressLessThan(ref start, ref end))
		{
			msgBytesSum += start.Payload.Length + 16 + 4;
			start = ref Unsafe.Add(ref start, 1);
		}
		return msgBytesSum;
	}
	public async Task<IEnumerable<MessageResponse<TMessage>>> PollMessagesAsync<TMessage>(MessageFetchRequest request,
		Func<byte[], TMessage> serializer, Func<byte[], byte[]>? decryptor = null)
	{

		int messageBufferSize = 18 + 5 + 2 + request.StreamId.Length + 2 + request.TopicId.Length;
		int payloadBufferSize = messageBufferSize + 4 + INITIAL_BYTES_LENGTH;
		var message = ArrayPool<byte>.Shared.Rent(messageBufferSize);
		var payload = ArrayPool<byte>.Shared.Rent(payloadBufferSize);
		
		//I fucking hate exceptions
		try
		{
			TcpContracts.GetMessages(message.AsSpan()[..messageBufferSize], request);
			CreatePayload(payload, message.AsSpan()[..messageBufferSize], CommandCodes.POLL_MESSAGES_CODE);

			await _socket.SendAsync(payload.AsMemory()[..payloadBufferSize]);
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(message);
			ArrayPool<byte>.Shared.Return(payload);
		}

		var buffer = ArrayPool<byte>.Shared.Rent(EXPECTED_RESPONSE_SIZE);
		try
		{
			await _socket.ReceiveAsync(buffer.AsMemory()[..EXPECTED_RESPONSE_SIZE]);
			var response = GetResponseLengthAndStatus(buffer);
			if (response.Status != 0)
			{
				throw new TcpInvalidResponseException();
			}

			if (response.Length <= 1)
			{
				return Enumerable.Empty<MessageResponse<TMessage>>();
			}

			var responseBuffer = ArrayPool<byte>.Shared.Rent(response.Length);
			
			try
			{
				await _socket.ReceiveAsync(responseBuffer.AsMemory()[..response.Length]);
				var result = BinaryMapper.MapMessages(
					responseBuffer.AsSpan()[..response.Length], serializer, decryptor);
				return result;
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(responseBuffer);
			}
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(buffer);
		}
	}
	public async Task<IEnumerable<MessageResponse>> PollMessagesAsync(MessageFetchRequest request, Func<byte[], byte[]>? decryptor = null)
	{
		int messageBufferSize = 18 + 5 + 2 + request.StreamId.Length + 2 + request.TopicId.Length;
		int payloadBufferSize = messageBufferSize + 4 + INITIAL_BYTES_LENGTH;
		var message = ArrayPool<byte>.Shared.Rent(messageBufferSize);
		var payload = ArrayPool<byte>.Shared.Rent(payloadBufferSize);
		
		//I fucking hate exceptions
		try
		{
			TcpContracts.GetMessages(message.AsSpan()[..messageBufferSize], request);
			CreatePayload(payload, message.AsSpan()[..messageBufferSize], CommandCodes.POLL_MESSAGES_CODE);

			await _socket.SendAsync(payload.AsMemory()[..payloadBufferSize]);
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(message);
			ArrayPool<byte>.Shared.Return(payload);
		}

		var buffer = ArrayPool<byte>.Shared.Rent(EXPECTED_RESPONSE_SIZE);
		try
		{
			await _socket.ReceiveAsync(buffer.AsMemory()[..EXPECTED_RESPONSE_SIZE]);

			var response = GetResponseLengthAndStatus(buffer);
			if (response.Status != 0)
			{
				throw new TcpInvalidResponseException();
			}

			if (response.Length <= 1)
			{
				return Enumerable.Empty<MessageResponse>();
			}

			var responseBuffer = ArrayPool<byte>.Shared.Rent(response.Length);
			
			try
			{
				await _socket.ReceiveAsync(responseBuffer.AsMemory()[..response.Length]);
				var result = BinaryMapper.MapMessages(
					responseBuffer.AsSpan()[..response.Length], decryptor);
				return result;
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(responseBuffer);
			}
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(buffer);
		}
	}


	public async Task StoreOffsetAsync(Identifier streamId, Identifier topicId, OffsetContract contract)
	{
		var message = TcpContracts.UpdateOffset(streamId, topicId, contract);
		var payload = new byte[4 + INITIAL_BYTES_LENGTH + message.Length];
		CreatePayload(payload, message, CommandCodes.STORE_OFFSET_CODE);

		await _socket.SendAsync(payload);

		var buffer = new byte[EXPECTED_RESPONSE_SIZE];
		await _socket.ReceiveAsync(buffer);

		var status = GetResponseStatus(buffer);

		if (status != 0)
		{
			throw new InvalidResponseException($"Invalid response status code: {status}");
		}
	}

	public async Task<OffsetResponse?> GetOffsetAsync(OffsetRequest request)
	{
		var message = TcpContracts.GetOffset(request);
		var payload = new byte[4 + INITIAL_BYTES_LENGTH + message.Length];
		CreatePayload(payload, message, CommandCodes.GET_OFFSET_CODE);

		await _socket.SendAsync(payload);

		var buffer = new byte[EXPECTED_RESPONSE_SIZE];
		await _socket.ReceiveAsync(buffer);
		
		var response = GetResponseLengthAndStatus(buffer); 
		
		if (response.Status != 0)
		{
			throw new InvalidResponseException($"Invalid response status code: {response.Status}");
		}

		if (response.Length <= 1)
		{
			return null;
		}

		var responseBuffer = new byte[response.Length];
		await _socket.ReceiveAsync(responseBuffer);
		return BinaryMapper.MapOffsets(responseBuffer);
	}

	public async Task<IEnumerable<ConsumerGroupResponse>> GetConsumerGroupsAsync(Identifier streamId, Identifier topicId)
	{
		var message = TcpContracts.GetGroups(streamId, topicId);
		var payload = new byte[4 + INITIAL_BYTES_LENGTH + message.Length];
		CreatePayload(payload, message, CommandCodes.GET_GROUPS_CODE);

		await _socket.SendAsync(payload);

		var buffer = new byte[EXPECTED_RESPONSE_SIZE];
		await _socket.ReceiveAsync(buffer);

		var response = GetResponseLengthAndStatus(buffer);
		
		if (response.Status != 0)
		{
			throw new InvalidResponseException($"Invalid response status code: {response.Status}");
		}

		if (response.Length <= 1)
		{
			return Enumerable.Empty<ConsumerGroupResponse>();
		}

		var responseBuffer = new byte[response.Length];
		await _socket.ReceiveAsync(responseBuffer);
		return BinaryMapper.MapConsumerGroups(responseBuffer);
	}

	public async Task<ConsumerGroupResponse?> GetConsumerGroupByIdAsync(Identifier streamId, Identifier topicId, int groupId)
	{
		var message = TcpContracts.GetGroup(streamId, topicId, groupId);
		var payload = new byte[4 + INITIAL_BYTES_LENGTH + message.Length];
		CreatePayload(payload, message, CommandCodes.GET_GROUP_CODE);

		await _socket.SendAsync(payload);

		var buffer = new byte[EXPECTED_RESPONSE_SIZE];
		await _socket.ReceiveAsync(buffer);

		var response = GetResponseLengthAndStatus(buffer);
		
		if (response.Status != 0)
		{
			throw new InvalidResponseException($"Invalid response status code: {response.Status}");
		}

		if (response.Length <= 1)
		{
			return null;
		}

		var responseBuffer = new byte[response.Length];
		await _socket.ReceiveAsync(responseBuffer);
		return BinaryMapper.MapConsumerGroup(responseBuffer);
	}

	public async Task CreateConsumerGroupAsync(Identifier streamId, Identifier topicId, CreateConsumerGroupRequest request)
	{
		var message = TcpContracts.CreateGroup(streamId, topicId, request);
		var payload = new byte[4 + INITIAL_BYTES_LENGTH + message.Length];
		CreatePayload(payload, message, CommandCodes.CREATE_GROUP_CODE);

		await _socket.SendAsync(payload);

		var buffer = new byte[EXPECTED_RESPONSE_SIZE];
		await _socket.ReceiveAsync(buffer);
		
		var status = GetResponseStatus(buffer);
		
		if (status != 0)
		{
			throw new InvalidResponseException($"Invalid response status code: {status}");
		}
	}

	public async Task DeleteConsumerGroupAsync(Identifier streamId, Identifier topicId, int groupId)
	{
		var message = TcpContracts.DeleteGroup(streamId, topicId, groupId);
		var payload = new byte[4 + INITIAL_BYTES_LENGTH + message.Length];
		CreatePayload(payload, message, CommandCodes.DELETE_GROUP_CODE);

		await _socket.SendAsync(payload);

		var buffer = new byte[EXPECTED_RESPONSE_SIZE];
		await _socket.ReceiveAsync(buffer);

		var status = GetResponseStatus(buffer);

		if (status != 0)
		{
			throw new InvalidResponseException($"Invalid response status code: {status}");
		}
	}

	public async Task JoinConsumerGroupAsync(JoinConsumerGroupRequest request)
	{
		var message = TcpContracts.JoinGroup(request);
		var payload = new byte[4 + INITIAL_BYTES_LENGTH + message.Length];
		CreatePayload(payload, message, CommandCodes.JOIN_GROUP_CODE);

		await _socket.SendAsync(payload);

		var buffer = new byte[EXPECTED_RESPONSE_SIZE];
		await _socket.ReceiveAsync(buffer);

		var status = GetResponseStatus(buffer);

		if (status != 0)
		{
			throw new InvalidResponseException($"Invalid response status code: {status}");
		}
	}

	public async Task LeaveConsumerGroupAsync(LeaveConsumerGroupRequest request)
	{
		var message = TcpContracts.LeaveGroup(request);
		var payload = new byte[4 + INITIAL_BYTES_LENGTH + message.Length];
		CreatePayload(payload, message, CommandCodes.LEAVE_GROUP_CODE);

		await _socket.SendAsync(payload);

		var buffer = new byte[EXPECTED_RESPONSE_SIZE];
		await _socket.ReceiveAsync(buffer);

		var status = GetResponseStatus(buffer);

		if (status != 0)
		{
			throw new InvalidResponseException($"Invalid response status code: {status}");
		}
	}
	public async Task DeletePartitionsAsync(Identifier streamId, Identifier topicId, DeletePartitionsRequest request)
	{
		var message = TcpContracts.DeletePartitions(streamId, topicId, request);
		var payload = new byte[4 + INITIAL_BYTES_LENGTH + message.Length];
		CreatePayload(payload, message, CommandCodes.DELETE_PARTITIONS_CODE);
		
		await _socket.SendAsync(payload);

		var buffer = new byte[EXPECTED_RESPONSE_SIZE];
		await _socket.ReceiveAsync(buffer);

		var status = GetResponseStatus(buffer);

		if (status != 0)
		{
			throw new InvalidResponseException($"Invalid response status code: {status}");
		}
	}

	public async Task CreatePartitionsAsync(Identifier streamId, Identifier topicId, CreatePartitionsRequest request)
	{
		var message = TcpContracts.CreatePartitions(streamId, topicId, request);
		var payload = new byte[4 + INITIAL_BYTES_LENGTH + message.Length];
		CreatePayload(payload, message, CommandCodes.CREATE_PARTITIONS_CODE);
		
		await _socket.SendAsync(payload);

		var buffer = new byte[EXPECTED_RESPONSE_SIZE];
		await _socket.ReceiveAsync(buffer);

		var status = GetResponseStatus(buffer);

		if (status != 0)
		{
			throw new InvalidResponseException($"Invalid response status code: {status}");
		}
	}
	public async Task<Stats?> GetStatsAsync()
	{
		var message = Array.Empty<byte>();
		var payload = new byte[4 + INITIAL_BYTES_LENGTH + message.Length];
		CreatePayload(payload, message, CommandCodes.GET_STATS_CODE);

		await _socket.SendAsync(payload);

		var buffer = new byte[EXPECTED_RESPONSE_SIZE];
		await _socket.ReceiveAsync(buffer);

		var response = GetResponseLengthAndStatus(buffer);

		if (response.Status != 0)
		{
			throw new InvalidResponseException($"Invalid response status code: {response.Status}");
		}
		
		if (response.Length <= 1)
		{
			return null;
		}

		var responseBuffer = new byte[response.Length];
		await _socket.ReceiveAsync(responseBuffer);

		return BinaryMapper.MapStats(responseBuffer);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static byte[] GetBytesFromIdentifier(Identifier identifier)
	{
        Span<byte> bytes = stackalloc byte[2 + identifier.Length];
        bytes[0] = identifier.Kind switch
        {
            IdKind.Numeric => 1,
            IdKind.String => 2,
            _ => throw new ArgumentOutOfRangeException()
        };
        bytes[1] = (byte)identifier.Length;
        for (int i = 0; i < identifier.Length; i++)
        {
            bytes[i + 2] = identifier.Value[i];
        }
        
        return bytes.ToArray();
	}

	private static (int Status, int Length) GetResponseLengthAndStatus(Span<byte> buffer)
	{
		var status = GetResponseStatus(buffer);
		var length = BinaryPrimitives.ReadInt32LittleEndian(buffer[4..]);
		
		return (status, length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int GetResponseStatus(Span<byte> buffer) =>
		BinaryPrimitives.ReadInt32LittleEndian(buffer[..4]);
	
	private static void CreatePayload(Span<byte> result, Span<byte> message, int command)
	{
		var messageLength = message.Length + 4;
		BinaryPrimitives.WriteInt32LittleEndian(result[..4], messageLength);
		BinaryPrimitives.WriteInt32LittleEndian(result[4..8], command);
		message.CopyTo(result[8..]);
	}
	public void Dispose()
	{
		_socket.Close();
		_socket.Dispose();
	}

}