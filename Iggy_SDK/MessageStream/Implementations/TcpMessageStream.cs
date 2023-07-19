using System.Buffers.Binary;
using System.Net.Sockets;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Contracts.Tcp;
using Iggy_SDK.Exceptions;
using Iggy_SDK.Mappers;
using Iggy_SDK.Utils;

namespace Iggy_SDK.MessageStream.Implementations;

public sealed class TcpMessageStream : IMessageStream, IDisposable
{
	private const int InitialBytesLength = 4;
	private const int ExpectedResponseSize = 8;
	private readonly TcpClient _client;
	private readonly NetworkStream _stream;

	internal TcpMessageStream(TcpClient client)
	{
		_client = client;
		_stream = client.GetStream();
	}

	public async Task CreateStreamAsync(StreamRequest request)
	{
		var message = TcpContracts.CreateStream(request);
		var payload = CreatePayload(message,CommandCodes.CREATE_STREAM_CODE);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);

		var status = GetResponseStatus(buffer);
		if (status != 0)
		{
			throw new InvalidResponseException($"Invalid response status code: {status}");
		}
	}

	public async Task<StreamResponse?> GetStreamByIdAsync(int streamId)
	{
		var message = BitConverter.GetBytes(streamId);
		var payload = CreatePayload(message, CommandCodes.GET_STREAM_CODE);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);

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

	await _stream.ReadExactlyAsync(responseBuffer);
		return BinaryMapper.MapStream(responseBuffer);
	}

	public async Task<IEnumerable<StreamResponse>> GetStreamsAsync()
	{
		var message = Enumerable.Empty<byte>().ToArray();
		var payload = CreatePayload(message, CommandCodes.GET_STREAMS_CODE);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);

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
		
		await _stream.ReadExactlyAsync(responseBuffer);
		return BinaryMapper.MapStreams(responseBuffer);
	}

	public async Task DeleteStreamAsync(int streamId)
	{
		var message = BitConverter.GetBytes(streamId);
		var payload = CreatePayload(message, CommandCodes.DELETE_STREAM_CODE);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);

		var status = GetResponseStatus(buffer);

		if (status != 0)
		{
			throw new InvalidResponseException($"Invalid response status code: {status}"); 
		}
	}

	public async Task<IEnumerable<TopicResponse>> GetTopicsAsync(int streamId)
	{
		var message = BitConverter.GetBytes(streamId);
		var payload = CreatePayload(message, CommandCodes.GET_TOPICS_CODE);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);

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
		await _stream.ReadExactlyAsync(responseBuffer);
		return BinaryMapper.MapTopics(responseBuffer);
	}

	public async Task<TopicResponse?> GetTopicByIdAsync(int streamId, int topicId)
	{
		var message = TcpContracts.GetTopicById(streamId, topicId);
		var payload = CreatePayload(message, CommandCodes.GET_TOPIC_CODE);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);

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
		await _stream.ReadExactlyAsync(responseBuffer);
		return BinaryMapper.MapTopic(responseBuffer);
	}


	public async Task CreateTopicAsync(int streamId, TopicRequest topic)
	{
		var message = TcpContracts.CreateTopic(streamId, topic);
		var payload = CreatePayload(message, CommandCodes.CREATE_TOPIC_CODE);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);
		
		var status = GetResponseStatus(buffer); 
		
		if (status != 0)
		{
			throw new InvalidResponseException($"Invalid response status code: {status}");
		}
	}

	public async Task DeleteTopicAsync(int streamId, int topicId)
	{
		var message = TcpContracts.DeleteTopic(streamId, topicId);
		var payload = CreatePayload(message, CommandCodes.DELETE_TOPIC_CODE);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);
		
		var status = GetResponseStatus(buffer);
		
		if (status != 0)
		{
			throw new InvalidResponseException($"Invalid response status code: {status}");
		}
	}

	public async Task SendMessagesAsync(MessageSendRequest request)
	{
		var message = TcpContracts.CreateMessage(request);
		var payload = CreatePayload(message, CommandCodes.SEND_MESSAGES_CODE);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);
		
		var status = GetResponseStatus(buffer);
		
		if (status != 0)
		{
			throw new InvalidResponseException($"Invalid response status code: {status}");
		}
	}

	public async Task<IEnumerable<MessageResponse>> PollMessagesAsync(MessageFetchRequest request)
	{
		var message = TcpContracts.GetMessages(request);
		var payload = CreatePayload(message, CommandCodes.POLL_MESSAGES_CODE);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);

		var status = BinaryPrimitives.ReadInt32LittleEndian(buffer.AsSpan()[..4]);
		var length = BinaryPrimitives.ReadInt32LittleEndian(buffer.AsSpan()[4..]);
		if (status != 0)
		{
			throw new TcpInvalidResponseException();
		}

		if (length <= 1)
		{
			return Enumerable.Empty<MessageResponse>();
		}

		var responseBuffer = new byte[length];
		await _stream.ReadExactlyAsync(responseBuffer);
		return BinaryMapper.MapMessages(responseBuffer);
	}

	public async Task StoreOffsetAsync(int streamId, int topicId, OffsetContract contract)
	{
		var message = TcpContracts.UpdateOffset(streamId, topicId, contract);
		var payload = CreatePayload(message, CommandCodes.STORE_OFFSET_CODE);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);

		var status = GetResponseStatus(buffer);

		if (status != 0)
		{
			throw new InvalidResponseException($"Invalid response status code: {status}");
		}
	}

	public async Task<OffsetResponse?> GetOffsetAsync(OffsetRequest request)
	{
		var message = TcpContracts.GetOffset(request);
		var payload = CreatePayload(message, CommandCodes.GET_OFFSET_CODE);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);
		
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
		await _stream.ReadExactlyAsync(responseBuffer);
		return BinaryMapper.MapOffsets(responseBuffer);
	}

	public async Task<IEnumerable<ConsumerGroupResponse>> GetConsumerGroupsAsync(int streamId, int topicId)
	{
		var message = TcpContracts.GetGroups(streamId, topicId);
		var payload = CreatePayload(message, CommandCodes.GET_GROUPS_CODE);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);

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
		await _stream.ReadExactlyAsync(responseBuffer);
		return BinaryMapper.MapConsumerGroups(responseBuffer);
	}

	public async Task<ConsumerGroupResponse?> GetConsumerGroupByIdAsync(int streamId, int topicId, int groupId)
	{
		var message = TcpContracts.GetGroup(streamId, topicId, groupId);
		var payload = CreatePayload(message, CommandCodes.GET_GROUP_CODE);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);

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
		await _stream.ReadExactlyAsync(responseBuffer);
		return BinaryMapper.MapConsumerGroup(responseBuffer);
	}

	public async Task CreateConsumerGroupAsync(int streamId, int topicId, CreateConsumerGroupRequest request)
	{
		var message = TcpContracts.CreateGroup(streamId, topicId, request);
		var payload = CreatePayload(message, CommandCodes.CREATE_GROUP_CODE);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);
		
		var status = GetResponseStatus(buffer);
		
		if (status != 0)
		{
			throw new InvalidResponseException($"Invalid response status code: {status}");
		}
	}

	public async Task DeleteConsumerGroupAsync(int streamId, int topicId, int groupId)
	{
		var message = TcpContracts.DeleteGroup(streamId, topicId, groupId);
		var payload = CreatePayload(message, CommandCodes.DELETE_GROUP_CODE);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);

		var status = GetResponseStatus(buffer);

		if (status != 0)
		{
			throw new InvalidResponseException($"Invalid response status code: {status}");
		}
	}

	public async Task JoinConsumerGroupAsync(JoinConsumerGroupRequest request)
	{
		var message = TcpContracts.JoinGroup(request);
		var payload = CreatePayload(message, CommandCodes.JOIN_GROUP_CODE);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);

		var status = GetResponseStatus(buffer);

		if (status != 0)
		{
			throw new InvalidResponseException($"Invalid response status code: {status}");
		}
	}

	public async Task LeaveConsumerGroupAsync(LeaveConsumerGroupRequest request)
	{
		var message = TcpContracts.LeaveGroup(request);
		var payload = CreatePayload(message, CommandCodes.LEAVE_GROUP_CODE);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);

		var status = GetResponseStatus(buffer);

		if (status != 0)
		{
			throw new InvalidResponseException($"Invalid response status code: {status}");
		}
	}

	private static (int Status, int Length) GetResponseLengthAndStatus(Span<byte> buffer)
	{
		var status = GetResponseStatus(buffer);
		var length = BinaryPrimitives.ReadInt32LittleEndian(buffer[4..]);
		
		return (status, length);
	}

	private static int GetResponseStatus(Span<byte> buffer) =>
		BinaryPrimitives.ReadInt32LittleEndian(buffer[..4]);
		
	private static byte[] CreatePayload(Span<byte> message, int command)
	{
		var messageLength = message.Length + 4;
		Span<byte> messageBytes = stackalloc byte[InitialBytesLength + messageLength];
		BinaryPrimitives.WriteInt32LittleEndian(messageBytes[..4], messageLength);
		BinaryPrimitives.WriteInt32LittleEndian(messageBytes[4..8], command);
		message.CopyTo(messageBytes[8..]);
		return messageBytes.ToArray();
	}
	public void Dispose()
	{
		_client.Dispose();
		_stream.Dispose();
	}
}