using System.Net.Sockets;
using ConsoleApp;
using Iggy_SDK.Contracts;
using Iggy_SDK.Exceptions;
using Iggy_SDK.Mappers;

namespace Iggy_SDK.MessageStream.Implementations;

public sealed class TcpMessageStream : IMessageStream, IDisposable
{
	private const int InitialBytesLength = 4;
	private const int ExpectedResponseSize = 5;
	private readonly TcpClient _client;
	private readonly NetworkStream _stream;

	public TcpMessageStream(TcpClient client)
	{
		_client = client;
		_stream = client.GetStream();
	}
	public async Task<bool> CreateStreamAsync(StreamRequest request)
	{
		var message = TcpContracts.CreateStream(request);
		var messageLength = message.Length + 1;

		byte commandByte = CommandCodes.CREATE_STREAM_CODE;
		byte[] messageLengthBytes = BitConverter.GetBytes(messageLength);
		var payload = CreatePayload(messageLengthBytes, message, commandByte, messageLength);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);
		if (buffer.Length != ExpectedResponseSize)
		{
			throw new TcpInvalidResponseException();
		}
		
		var status = buffer[0];
		return status == 0;
	}

	public async Task<StreamResponse?> GetStreamByIdAsync(int streamId)
	{
		var message = BitConverter.GetBytes(streamId);
		var messageLength = message.Length + 1;

		byte commandByte = CommandCodes.GET_STREAM_CODE;
		byte[] messageLengthBytes = BitConverter.GetBytes(messageLength);
		var payload = CreatePayload(messageLengthBytes, message, commandByte, messageLength);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);
		if (buffer.Length != ExpectedResponseSize)
		{
			throw new TcpInvalidResponseException();
		}
		
		var status = buffer[0];
		var length = buffer[1];
		if (status != 0)
		{
			throw new TcpInvalidStatus();
		}
		if (length <= 1)
		{
			return null;
		}
		
		var responseBuffer = new byte[length];
		await _stream.ReadExactlyAsync(responseBuffer);
		return BinaryMapper.MapStream(responseBuffer);
	}

	public async Task<IEnumerable<StreamsResponse>> GetStreamsAsync()
	{
		var message = Enumerable.Empty<byte>().ToArray();
		var messageLength = message.Length + 1;

		byte commandByte = CommandCodes.GET_STREAMS_CODE;
		byte[] messageLengthBytes = BitConverter.GetBytes(messageLength);
		var payload = CreatePayload(messageLengthBytes, message, commandByte, messageLength);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);
		if (buffer.Length != ExpectedResponseSize)
		{
			throw new TcpInvalidResponseException();
		}
		
		var status = buffer[0];
		var length = buffer[1];
		if (status != 0)
		{
			throw new TcpInvalidStatus();
		}
		if (length <= 1)
		{
			return Enumerable.Empty<StreamsResponse>();
		}
		
		var responseBuffer = new byte[length];
		await _stream.ReadExactlyAsync(responseBuffer);
		return BinaryMapper.MapStreams(responseBuffer);
	}

	public async Task<bool> DeleteStreamAsync(int streamId)
	{
		var message = BitConverter.GetBytes(streamId);
		var messageLength = message.Length + 1;

		byte commandByte = CommandCodes.DELETE_STREAM_CODE;
		byte[] messageLengthBytes = BitConverter.GetBytes(messageLength);
		var payload = CreatePayload(messageLengthBytes, message, commandByte, messageLength);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);
		if (buffer.Length != ExpectedResponseSize)
		{
			throw new TcpInvalidResponseException();
		}
		
		var status = buffer[0];
		return status == 0;
	}

	public async Task<IEnumerable<TopicsResponse>> GetTopicsAsync(int streamId)
	{
		var message = BitConverter.GetBytes(streamId);
		var messageLength = message.Length + 1;

		byte commandByte = CommandCodes.GET_TOPICS_CODE;
		byte[] messageLengthBytes = BitConverter.GetBytes(messageLength);
		var payload = CreatePayload(messageLengthBytes, message, commandByte, messageLength);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);
		if (buffer.Length != ExpectedResponseSize)
		{
			throw new TcpInvalidResponseException();
		}
		
		var status = buffer[0];
		var length = buffer[1];
		if (status != 0)
		{
			throw new TcpInvalidStatus();
		}
		if (length <= 1)
		{
			return Enumerable.Empty<TopicsResponse>();
		}
		
		var responseBuffer = new byte[length];
		await _stream.ReadExactlyAsync(responseBuffer);
		return BinaryMapper.MapTopics(responseBuffer);
	}

	public async Task<TopicsResponse?> GetTopicByIdAsync(int streamId, int topicId)
	{
		var message = TcpContracts.GetTopicById(streamId, topicId);
		var messageLength = message.Length + 1;

		byte commandByte = CommandCodes.GET_TOPIC_CODE;
		byte[] messageLengthBytes = BitConverter.GetBytes(messageLength);
		var payload = CreatePayload(messageLengthBytes, message, commandByte, messageLength);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);
		if (buffer.Length != ExpectedResponseSize)
		{
			throw new TcpInvalidResponseException();
		}
		
		var status = buffer[0];
		var length = buffer[1];
		if (status != 0)
		{
			throw new TcpInvalidStatus();
		}
		if (length <= 1)
		{
			return null;
		}
		
		var responseBuffer = new byte[length];
		await _stream.ReadExactlyAsync(responseBuffer);
		return BinaryMapper.MapTopic(responseBuffer);
	}


	public async Task<bool> CreateTopicAsync(int streamId, TopicRequest topic)
	{
		var message = TcpContracts.CreateTopic(streamId, topic);
		var messageLength = message.Length + 1;

		byte commandByte = CommandCodes.CREATE_TOPIC_CODE;
		byte[] messageLengthBytes = BitConverter.GetBytes(messageLength);
		var payload = CreatePayload(messageLengthBytes, message, commandByte, messageLength);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);
		if (buffer.Length != ExpectedResponseSize)
		{
			throw new TcpInvalidResponseException();
		}
		
		var status = buffer[0];
		return status == 0;
	}

	public async Task<bool> DeleteTopicAsync(int streamId, int topicId)
	{
		var message = TcpContracts.DeleteTopic(streamId, topicId);
		var messageLength = message.Length + 1;

		byte commandByte = CommandCodes.DELETE_TOPIC_CODE;
		byte[] messageLengthBytes = BitConverter.GetBytes(messageLength);
		var payload = CreatePayload(messageLengthBytes, message, commandByte, messageLength);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);
		if (buffer.Length != ExpectedResponseSize)
		{
			throw new TcpInvalidResponseException();
		}
		
		var status = buffer[0];
		return status == 0;
	}

	public Task<bool> SendMessagesAsync(MessageSendRequest request)
	{
		throw new NotImplementedException();
	}

	public Task<IEnumerable<MessageResponse>> GetMessagesAsync(MessageFetchRequest request)
	{
		throw new NotImplementedException();
	}

	public async Task<bool> UpdateOffsetAsync(int streamId, int topicId, OffsetContract contract)
	{
		var message = TcpContracts.UpdateOffset(streamId, topicId, contract);
		var messageLength = message.Length + 1;

		byte commandByte = CommandCodes.STORE_OFFSET_CODE;
		byte[] messageLengthBytes = BitConverter.GetBytes(messageLength);
		var payload = CreatePayload(messageLengthBytes, message, commandByte, messageLength);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);
		if (buffer.Length != ExpectedResponseSize)
		{
			throw new TcpInvalidResponseException();
		}

		var status = buffer[0];
		return status == 0;
	}

	public async Task<OffsetResponse?> GetOffsetAsync(OffsetRequest request)
	{
		var message = TcpContracts.GetOffset(request);
		var messageLength = message.Length + 1;

		byte commandByte = CommandCodes.GET_OFFSET_CODE;
		byte[] messageLengthBytes = BitConverter.GetBytes(messageLength);
		var payload = CreatePayload(messageLengthBytes, message, commandByte, messageLength);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);
		if (buffer.Length != ExpectedResponseSize)
		{
			throw new TcpInvalidResponseException();
		}
		
		var status = buffer[0];
		var length = buffer[1];
		if (status != 0)
		{
			throw new TcpInvalidStatus();
		}
		if (length <= 1)
		{
			return null;
		}
		
		var responseBuffer = new byte[length];
		await _stream.ReadExactlyAsync(responseBuffer);
		return BinaryMapper.MapOffsets(responseBuffer);
	}

	public async Task<IEnumerable<GroupResponse>> GetGroupsAsync(int streamId, int topicId)
	{
		var message = TcpContracts.GetGroups(streamId, topicId);
		var messageLength = message.Length + 1;

		byte commandByte = CommandCodes.GET_GROUPS_CODE;
		byte[] messageLengthBytes = BitConverter.GetBytes(messageLength);
		var payload = CreatePayload(messageLengthBytes, message, commandByte, messageLength);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);
		if (buffer.Length != ExpectedResponseSize)
		{
			throw new TcpInvalidResponseException();
		}

		var status = buffer[0];
		var length = buffer[1];
		if (status != 0)
		{
			throw new TcpInvalidStatus();
		}

		if (length <= 1)
		{
			return Enumerable.Empty<GroupResponse>();
		}

		var responseBuffer = new byte[length];
		await _stream.ReadExactlyAsync(responseBuffer);
		return BinaryMapper.MapConsumerGroups(responseBuffer);
	}

	public async Task<GroupResponse?> GetGroupByIdAsync(int streamId, int topicId, int groupId)
	{
		var message = TcpContracts.GetGroup(streamId, topicId, groupId);
		var messageLength = message.Length + 1;

		byte commandByte = CommandCodes.GET_GROUP_CODE;
		byte[] messageLengthBytes = BitConverter.GetBytes(messageLength);
		var payload = CreatePayload(messageLengthBytes, message, commandByte, messageLength);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);
		if (buffer.Length != ExpectedResponseSize)
		{
			throw new TcpInvalidResponseException();
		}

		var status = buffer[0];
		var length = buffer[1];
		if (status != 0)
		{
			throw new TcpInvalidStatus();
		}

		if (length <= 1)
		{
			return null;
		}

		var responseBuffer = new byte[length];
		await _stream.ReadExactlyAsync(responseBuffer);
		return BinaryMapper.MapConsumerGroup(responseBuffer);
	}

	public async Task<bool> CreateGroupAsync(int streamId, int topicId, GroupRequest request)
	{
		var message = TcpContracts.CreateGroup(streamId, topicId, request);
		var messageLength = message.Length + 1;

		byte commandByte = CommandCodes.CREATE_GROUP_CODE;
		byte[] messageLengthBytes = BitConverter.GetBytes(messageLength);
		var payload = CreatePayload(messageLengthBytes, message, commandByte, messageLength);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);
		if (buffer.Length != ExpectedResponseSize)
		{
			throw new TcpInvalidResponseException();
		}

		var status = buffer[0];
		return status == 0;
	}

	public async Task<bool> DeleteGroupAsync(int streamId, int topicId, int groupId)
	{
		var message = TcpContracts.DeleteGroup(streamId, topicId, groupId);
		var messageLength = message.Length + 1;

		byte commandByte = CommandCodes.DELETE_GROUP_CODE;
		byte[] messageLengthBytes = BitConverter.GetBytes(messageLength);
		var payload = CreatePayload(messageLengthBytes, message, commandByte, messageLength);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);
		if (buffer.Length != ExpectedResponseSize)
		{
			throw new TcpInvalidResponseException();
		}

		var status = buffer[0];
		return status == 0;
	}
	
	private static byte[] CreatePayload(byte[] messageLengthBytes, byte[] message, byte commandByte, int messageLength)
	{
		Span<byte> messageBytes = stackalloc byte[InitialBytesLength + messageLength];
		messageLengthBytes.CopyTo(messageBytes);
		messageBytes[InitialBytesLength] = commandByte;
		message.CopyTo(messageBytes[(InitialBytesLength + 1)..]);
		return messageBytes.ToArray();
	}
	
	public void Dispose()
	{
		_client.Dispose();
		_stream.Dispose();
	}
}