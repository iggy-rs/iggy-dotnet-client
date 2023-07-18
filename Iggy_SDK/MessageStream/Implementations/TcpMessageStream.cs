using System.Buffers.Binary;
using System.Net.Sockets;
using System.Security.AccessControl;
using Iggy_SDK.Contracts;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Contracts.Tcp;
using Iggy_SDK.Exceptions;
using Iggy_SDK.Mappers;
using Iggy_SDK.Utils;

namespace Iggy_SDK.MessageStream.Implementations;

//#TODO - refactor methods to not throw InvalidTcpStatus Exception
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
	public async Task<Result> CreateStreamAsync(StreamRequest request)
	{
		byte[] message = TcpContracts.CreateStream(request);

		byte commandByte = CommandCodes.CREATE_STREAM_CODE;
		var payload = CreatePayload(message, commandByte);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);
		
		var status = buffer[0];
		return status switch
		{
			0 => Result.Success(),
			6 => new Result { IsSuccess = false, Error = ErrorFactory.CannotCreateStreamDirectory(request.StreamId) },
			7 => new Result { IsSuccess = false, Error = ErrorFactory.CannotCreateStreamInfo(request.StreamId) },
			9 => new Result { IsSuccess = false, Error = ErrorFactory.CannotOpenStreamInfo(request.StreamId) },
			10 => new Result { IsSuccess = false, Error = ErrorFactory.CannotReadStreamInfo(request.StreamId) },
			11 => new Result { IsSuccess = false, Error = ErrorFactory.CannotCreateStream(request.StreamId) },
			15 => new Result { IsSuccess = false, Error = ErrorFactory.StreamAlreadyExists(request.StreamId) },
			16 => new Result { IsSuccess = false, Error = ErrorFactory.InvalidStreamName },
			_ => new Result { IsSuccess = false, Error = ErrorFactory.Error},
		};
	}

	public async Task<StreamResponse?> GetStreamByIdAsync(int streamId)
	{
		var message = BitConverter.GetBytes(streamId);

		byte commandByte = CommandCodes.GET_STREAM_CODE;
		var payload = CreatePayload(message, commandByte);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);
		

		var status = buffer[0];
		var length = BinaryPrimitives.ReadInt32LittleEndian(buffer.AsSpan()[1..]);
		if (status != 0)
		{
			throw new TcpInvalidResponseException();
		}
		if (length <= 1)
		{
			return null;
		}
		
		var responseBuffer = new byte[length];
		await _stream.ReadExactlyAsync(responseBuffer);
		return BinaryMapper.MapStream(responseBuffer);
	}
	
	public async Task<IEnumerable<StreamResponse>> GetStreamsAsync()
	{
		var message = Enumerable.Empty<byte>().ToArray();

		byte commandByte = CommandCodes.GET_STREAMS_CODE;
		var payload = CreatePayload(message, commandByte);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);
		
		var status = buffer[0];
		var length = BinaryPrimitives.ReadInt32LittleEndian(buffer.AsSpan()[1..]);
		if (status != 0)
		{
			throw new TcpInvalidResponseException();
		}
		if (length <= 1)
		{
			return Enumerable.Empty<StreamResponse>();
		}
		
		var responseBuffer = new byte[length];
		await _stream.ReadExactlyAsync(responseBuffer);
		return BinaryMapper.MapStreams(responseBuffer);
	}

	public async Task<Result> DeleteStreamAsync(int streamId)
	{
		var message = BitConverter.GetBytes(streamId);

		byte commandByte = CommandCodes.DELETE_STREAM_CODE;
		var payload = CreatePayload(message, commandByte);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);
		
		var status = buffer[0];
		return status switch
		{
			0 => Result.Success(),
			12 => new Result { IsSuccess = false, Error = ErrorFactory.CannotDeleteStream(streamId) },
			13 => new Result { IsSuccess = false, Error = ErrorFactory.CannotDeleteStreamDirectory(streamId) },
			14 => new Result { IsSuccess = false, Error = ErrorFactory.StreamNotFound(streamId) },
			_ => new Result { IsSuccess = false, Error = ErrorFactory.Error},
		};
	}

	public async Task<IEnumerable<TopicResponse>> GetTopicsAsync(int streamId)
	{
		var message = BitConverter.GetBytes(streamId);

		byte commandByte = CommandCodes.GET_TOPICS_CODE;
		var payload = CreatePayload(message, commandByte);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);
		
		var status = buffer[0];
		var length = BinaryPrimitives.ReadInt32LittleEndian(buffer.AsSpan()[1..]);
		if (status != 0)
		{
			throw new TcpInvalidResponseException();
		}
		if (length <= 1)
		{
			return Enumerable.Empty<TopicResponse>();
		}
		
		var responseBuffer = new byte[length];
		await _stream.ReadExactlyAsync(responseBuffer);
		return BinaryMapper.MapTopics(responseBuffer);
	}

	public async Task<TopicResponse?> GetTopicByIdAsync(int streamId, int topicId)
	{
		var message = TcpContracts.GetTopicById(streamId, topicId);

		byte commandByte = CommandCodes.GET_TOPIC_CODE;
		var payload = CreatePayload(message, commandByte);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);
		
		var status = buffer[0];
		var length = BinaryPrimitives.ReadInt32LittleEndian(buffer.AsSpan()[1..]);
		if (status != 0)
		{
			throw new TcpInvalidResponseException();
		}
		if (length <= 1)
		{
			return null;
		}
		
		var responseBuffer = new byte[length];
		await _stream.ReadExactlyAsync(responseBuffer);
		return BinaryMapper.MapTopic(responseBuffer);
	}


	public async Task<Result> CreateTopicAsync(int streamId, TopicRequest topic)
	{
		var message = TcpContracts.CreateTopic(streamId, topic);

		byte commandByte = CommandCodes.CREATE_TOPIC_CODE;
		var payload = CreatePayload(message, commandByte);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);
		
		var status = buffer[0];
		return status switch
		{
			0 => Result.Success(),
				14 => new Result { IsSuccess = false, Error = ErrorFactory.StreamNotFound(streamId) },			
			17 => new Result { IsSuccess = false, Error = ErrorFactory.CannotCreateTopicsDirectory(streamId) },
			18 => new Result { IsSuccess = false, Error = ErrorFactory.CannotCreateTopicDirectory(streamId,topic.TopicId) },
			19 => new Result { IsSuccess = false, Error = ErrorFactory.CannotCreateTopicInfo(streamId, topic.TopicId) },
			23 => new Result { IsSuccess = false, Error = ErrorFactory.CannotCreateTopic(streamId, topic.TopicId) },
			28 => new Result { IsSuccess = false, Error = ErrorFactory.TopicAlreadyExists(streamId, topic.TopicId) },
			29 => new Result { IsSuccess = false, Error = ErrorFactory.InvalidTopicName },
			30 => new Result { IsSuccess = false, Error = ErrorFactory.InvalidTopicPartitions },
			_ => new Result { IsSuccess = false, Error = ErrorFactory.Error},
		};
	}

	public async Task<Result> DeleteTopicAsync(int streamId, int topicId)
	{
		var message = TcpContracts.DeleteTopic(streamId, topicId);

		byte commandByte = CommandCodes.DELETE_TOPIC_CODE;
		var payload = CreatePayload(message, commandByte);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);
		
		var status = buffer[0];
		return status switch
		{
			0 => Result.Success(),
			24 => new Result { IsSuccess = false, Error = ErrorFactory.CannotDeleteTopic(streamId,topicId) },
			25 => new Result { IsSuccess = false, Error = ErrorFactory.CannotDeleteTopicDirectory(streamId,topicId) },
			27 => new Result { IsSuccess = false, Error = ErrorFactory.TopicNotFound(streamId,topicId) },
			_ => new Result { IsSuccess = false, Error = ErrorFactory.Error},
		};
	}

	public async Task<Result> SendMessagesAsync(MessageSendRequest request)
	{
		var message = TcpContracts.CreateMessage(request);

		byte commandByte = CommandCodes.SEND_MESSAGES_CODE;
		var payload = CreatePayload(message, commandByte);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);
		
		var status = buffer[0];
		return status switch
		{
			0 => Result.Success(),
			40 => new Result { IsSuccess = false, Error = ErrorFactory.InvalidMessagesCount },
			41 => new Result { IsSuccess = false, Error = ErrorFactory.InvalidStreamId },
			42 => new Result { IsSuccess = false, Error = ErrorFactory.InvalidTopicId },
			43 => new Result { IsSuccess = false, Error = ErrorFactory.SegmentNotFound },
			55 => new Result { IsSuccess = false, Error = ErrorFactory.CannotParseUtf8() },
			56 => new Result { IsSuccess = false, Error = ErrorFactory.CannotParseInt() },
			57 => new Result { IsSuccess = false, Error = ErrorFactory.CannotParseSlice() },
			58 => new Result { IsSuccess = false, Error = ErrorFactory.TooBigMessagePayload },
			59 => new Result { IsSuccess = false, Error = ErrorFactory.TooManyMessages },
			60 => new Result { IsSuccess = false, Error = ErrorFactory.WriteError() },
			_ => new Result { IsSuccess = false, Error = ErrorFactory.Error}, 
		};
	}

	public async Task<IEnumerable<MessageResponse>> PollMessagesAsync(MessageFetchRequest request)
	{
		byte[] message = TcpContracts.GetMessages(request);

		byte commandByte = CommandCodes.POLL_MESSAGES_CODE;
		var payload = CreatePayload(message, commandByte);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);
		
		var status = buffer[0];
		var length = BinaryPrimitives.ReadInt32LittleEndian(buffer.AsSpan()[1..]);
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

	public async Task<Result> StoreOffsetAsync(int streamId, int topicId, OffsetContract contract)
	{
		var message = TcpContracts.UpdateOffset(streamId, topicId, contract);

		byte commandByte = CommandCodes.STORE_OFFSET_CODE;
		var payload = CreatePayload(message, commandByte);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);

		var status = buffer[0];
		
		return status switch
		{
			0 => Result.Success(),
			27 => new Result { IsSuccess = false, Error = ErrorFactory.TopicNotFound(streamId,topicId) },
			28 => new Result { IsSuccess = false, Error = ErrorFactory.InvalidTopicId },
			14 => new Result { IsSuccess = false, Error = ErrorFactory.StreamNotFound(streamId) },			
			33 => new Result { IsSuccess = false, Error = ErrorFactory.CannotCreatePartition(streamId, topicId, contract.PartitionId) },
			34 => new Result { IsSuccess = false, Error = ErrorFactory.CannotCreatePartitionDirectory(streamId, topicId, contract.PartitionId) },
			61 => new Result { IsSuccess = false, Error = ErrorFactory.InvalidOffset(contract.Offset) },
			62 => new Result { IsSuccess = false, Error = ErrorFactory.CannotReadConsumerOffsets(contract.PartitionId) }, 
			68 => new Result { IsSuccess = false, Error = ErrorFactory.CannotReadStreams },
			69 => new Result { IsSuccess = false, Error = ErrorFactory.CannotReadTopics(streamId) }, 
			_ => new Result { IsSuccess = false, Error = ErrorFactory.Error}, 
		};
	}

	public async Task<OffsetResponse?> GetOffsetAsync(OffsetRequest request)
	{
		var message = TcpContracts.GetOffset(request);

		byte commandByte = CommandCodes.GET_OFFSET_CODE;
		var payload = CreatePayload(message, commandByte);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);
		
		var status = buffer[0];
		var length = BinaryPrimitives.ReadInt32LittleEndian(buffer.AsSpan()[1..]);
		if (status != 0)
		{
			throw new TcpInvalidResponseException();
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

		byte commandByte = CommandCodes.GET_GROUPS_CODE;
		var payload = CreatePayload(message, commandByte);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);

		var status = buffer[0];
		var length = BinaryPrimitives.ReadInt32LittleEndian(buffer.AsSpan()[1..]);
		if (status != 0)
		{
			throw new TcpInvalidResponseException();
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

		byte commandByte = CommandCodes.GET_GROUP_CODE;
		var payload = CreatePayload(message, commandByte);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);

		var status = buffer[0];
		var length = BinaryPrimitives.ReadInt32LittleEndian(buffer.AsSpan()[1..]);
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

	public async Task<Result> CreateGroupAsync(int streamId, int topicId, CreateGroupRequest request)
	{
		var message = TcpContracts.CreateGroup(streamId, topicId, request);

		byte commandByte = CommandCodes.CREATE_GROUP_CODE;
		var payload = CreatePayload(message, commandByte);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);

		var status = buffer[0];
		return status switch
		{
			0 => Result.Success(),
			14 => new Result { IsSuccess = false, Error = ErrorFactory.StreamNotFound(streamId) },
			27 => new Result { IsSuccess = false, Error = ErrorFactory.TopicNotFound(streamId, topicId) },
			42 => new Result { IsSuccess = false, Error = ErrorFactory.InvalidStreamId },
			43 => new Result { IsSuccess = false, Error = ErrorFactory.InvalidTopicId },
			73 => new Result
			{
				IsSuccess = false, Error = ErrorFactory.ConsumerGroupAlreadyExists(streamId, request.ConsumerGroupId)
			},
			78 => new Result { IsSuccess = false, Error = ErrorFactory.CannotCreateConsumerGroupsDirectory(streamId, topicId) },
			79 => new Result { IsSuccess = false, Error = ErrorFactory.CannotReadConsumerGroups(streamId, topicId) },
			80 => new Result { IsSuccess = false, Error = ErrorFactory.CannotCreateConsumerGroupInfo(streamId, topicId, request.ConsumerGroupId) },
			75 => new Result { IsSuccess = false, Error = ErrorFactory.InvalidConsumerGroupId },
			_ => new Result { IsSuccess = false, Error = ErrorFactory.Error },

		};
	}

	public async Task<Result> DeleteGroupAsync(int streamId, int topicId, int groupId)
	{
		var message = TcpContracts.DeleteGroup(streamId, topicId, groupId);

		byte commandByte = CommandCodes.DELETE_GROUP_CODE;
		var payload = CreatePayload(message, commandByte);

		await _stream.WriteAsync(payload, 0, payload.Length);

		var buffer = new byte[ExpectedResponseSize];
		await _stream.ReadExactlyAsync(buffer);

		var status = buffer[0];
		return status switch
		{
			0 => Result.Success(),
			14 => new Result { IsSuccess = false, Error = ErrorFactory.StreamNotFound(streamId) },
			27 => new Result { IsSuccess = false, Error = ErrorFactory.TopicNotFound(streamId, topicId) },
			42 => new Result { IsSuccess = false, Error = ErrorFactory.InvalidStreamId },
			43 => new Result { IsSuccess = false, Error = ErrorFactory.InvalidTopicId },
			72 => new Result
			{
				IsSuccess = false, Error = ErrorFactory.ConsumerGroupNotFound(streamId, groupId)
			},
			75 => new Result { IsSuccess = false, Error = ErrorFactory.InvalidConsumerGroupId },
			81 => new Result { IsSuccess = false, Error = ErrorFactory.CannotDeleteConsumerGroupInfo(streamId, topicId, groupId) },
			_ => new Result { IsSuccess = false, Error = ErrorFactory.Error },

		};
	}
	
	private static byte[] CreatePayload(Span<byte> message, byte commandByte)
	{
		var messageLength = message.Length + 1; 
		Span<byte> messageBytes = stackalloc byte[InitialBytesLength + messageLength];
		BinaryPrimitives.WriteInt32LittleEndian(messageBytes[0..4],messageLength);
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