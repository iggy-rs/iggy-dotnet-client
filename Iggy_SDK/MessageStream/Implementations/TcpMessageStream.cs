using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Contracts.Tcp;
using Iggy_SDK.Exceptions;
using Iggy_SDK.Headers;
using Iggy_SDK.Kinds;
using Iggy_SDK.Mappers;
using Iggy_SDK.Messages;
using Iggy_SDK.Utils;
using System.Buffers;
using System.Net.Sockets;
using System.Threading.Channels;

namespace Iggy_SDK.MessageStream.Implementations;

public sealed class TcpMessageStream : IMessageStream, IDisposable
{
    private readonly Socket _socket;
    private readonly Channel<MessageSendRequest> _channel;

    private readonly Memory<byte> _responseBuffer = new(new byte[BufferSizes.ExpectedResponseSize]);

    internal TcpMessageStream(Socket socket, Channel<MessageSendRequest> channel)
    {
        _socket = socket;
        _channel = channel;
    }
    public async Task CreateStreamAsync(StreamRequest request, CancellationToken token = default)
    {
        var message = TcpContracts.CreateStream(request);
        var payload = new byte[4 + BufferSizes.InitialBytesLength + message.Length];
        TcpMessageStreamHelpers.CreatePayload(payload, message, CommandCodes.CREATE_STREAM_CODE);

        await _socket.SendAsync(payload, token);

        var buffer = new byte[BufferSizes.ExpectedResponseSize];
        await _socket.ReceiveAsync(buffer, token);

        var status = TcpMessageStreamHelpers.GetResponseStatus(buffer);
        if (status != 0)
        {
            throw new InvalidResponseException($"Invalid response status code: {status}");
        }
    }

    public async Task<StreamResponse?> GetStreamByIdAsync(Identifier streamId, CancellationToken token = default)
    {
        var message = TcpMessageStreamHelpers.GetBytesFromIdentifier(streamId);
        var payload = new byte[4 + BufferSizes.InitialBytesLength + message.Length];
        TcpMessageStreamHelpers.CreatePayload(payload, message, CommandCodes.GET_STREAM_CODE);

        await _socket.SendAsync(payload, token);

        var buffer = new byte[BufferSizes.ExpectedResponseSize];
        await _socket.ReceiveAsync(buffer, token);

        var response = TcpMessageStreamHelpers.GetResponseLengthAndStatus(buffer);
        var responseBuffer = new byte[response.Length];

        if (response.Status != 0)
        {
            throw new InvalidResponseException($"Invalid response status code: {response.Status}");
        }

        if (response.Length <= 1)
        {
            return null;
        }

        await _socket.ReceiveAsync(responseBuffer, token);
        return BinaryMapper.MapStream(responseBuffer);
    }

    public async Task UpdateStreamAsync(Identifier streamId, UpdateStreamRequest request, CancellationToken token = default)
    {
        var message = TcpContracts.UpdateStream(streamId, request);
        var payload = new byte[4 + BufferSizes.InitialBytesLength + message.Length];
        TcpMessageStreamHelpers.CreatePayload(payload, message, CommandCodes.UPDATE_STREAM_CODE);

        await _socket.SendAsync(payload, token);

        var buffer = new byte[BufferSizes.ExpectedResponseSize];
        await _socket.ReceiveAsync(buffer, token);

        var status = TcpMessageStreamHelpers.GetResponseStatus(buffer);
        if (status != 0)
        {
            throw new InvalidResponseException($"Invalid response status code: {status}");
        }
    }

    public async Task<IReadOnlyList<StreamResponse>> GetStreamsAsync(CancellationToken token = default)
    {
        var message = Array.Empty<byte>();
        var payload = new byte[4 + BufferSizes.InitialBytesLength + message.Length];
        TcpMessageStreamHelpers.CreatePayload(payload, message, CommandCodes.GET_STREAMS_CODE);

        await _socket.SendAsync(payload, token);

        var buffer = new byte[BufferSizes.ExpectedResponseSize];
        await _socket.ReceiveAsync(buffer, token);

        var response = TcpMessageStreamHelpers.GetResponseLengthAndStatus(buffer);
        var responseBuffer = new byte[response.Length];

        if (response.Status != 0)
        {
            throw new InvalidResponseException($"Invalid response status code: {response.Status}");
        }

        if (response.Length <= 1)
        {
            return EmptyList<StreamResponse>.Instance;
        }

        await _socket.ReceiveAsync(responseBuffer, token);
        return BinaryMapper.MapStreams(responseBuffer);
    }

    public async Task DeleteStreamAsync(Identifier streamId, CancellationToken token = default)
    {
        var message = TcpMessageStreamHelpers.GetBytesFromIdentifier(streamId);
        var payload = new byte[4 + BufferSizes.InitialBytesLength + message.Length];
        TcpMessageStreamHelpers.CreatePayload(payload, message, CommandCodes.DELETE_STREAM_CODE);

        await _socket.SendAsync(payload, token);

        var buffer = new byte[BufferSizes.ExpectedResponseSize];
        await _socket.ReceiveAsync(buffer, token);

        var status = TcpMessageStreamHelpers.GetResponseStatus(buffer);

        if (status != 0)
        {
            throw new InvalidResponseException($"Invalid response status code: {status}");
        }
    }

    public async Task<IReadOnlyList<TopicResponse>> GetTopicsAsync(Identifier streamId, CancellationToken token = default)
    {
        var message = TcpMessageStreamHelpers.GetBytesFromIdentifier(streamId);
        var payload = new byte[4 + BufferSizes.InitialBytesLength + message.Length];
        TcpMessageStreamHelpers.CreatePayload(payload, message, CommandCodes.GET_TOPICS_CODE);

        await _socket.SendAsync(payload, token);

        var buffer = new byte[BufferSizes.ExpectedResponseSize];
        await _socket.ReceiveAsync(buffer, token);

        var response = TcpMessageStreamHelpers.GetResponseLengthAndStatus(buffer);
        if (response.Status != 0)
        {
            throw new InvalidResponseException($"Invalid response status code: {response.Status}");
        }

        if (response.Length <= 1)
        {
            return EmptyList<TopicResponse>.Instance;
        }

        var responseBuffer = new byte[response.Length];
        await _socket.ReceiveAsync(responseBuffer, token);
        return BinaryMapper.MapTopics(responseBuffer);
    }

    public async Task<TopicResponse?> GetTopicByIdAsync(Identifier streamId, Identifier topicId, CancellationToken token = default)
    {
        var message = TcpContracts.GetTopicById(streamId, topicId);
        var payload = new byte[4 + BufferSizes.InitialBytesLength + message.Length];
        TcpMessageStreamHelpers.CreatePayload(payload, message, CommandCodes.GET_TOPIC_CODE);

        await _socket.SendAsync(payload, token);

        var buffer = new byte[BufferSizes.ExpectedResponseSize];
        await _socket.ReceiveAsync(buffer, token);

        var response = TcpMessageStreamHelpers.GetResponseLengthAndStatus(buffer);

        if (response.Status != 0)
        {
            throw new InvalidResponseException($"Invalid response status code: {response.Status}");
        }

        if (response.Length <= 1)
        {
            return null;
        }

        var responseBuffer = new byte[response.Length];
        await _socket.ReceiveAsync(responseBuffer, token);
        return BinaryMapper.MapTopic(responseBuffer);
    }


    public async Task CreateTopicAsync(Identifier streamId, TopicRequest topic, CancellationToken token = default)
    {
        var message = TcpContracts.CreateTopic(streamId, topic);
        var payload = new byte[4 + BufferSizes.InitialBytesLength + message.Length];
        TcpMessageStreamHelpers.CreatePayload(payload, message, CommandCodes.CREATE_TOPIC_CODE);

        await _socket.SendAsync(payload, token);

        var buffer = new byte[BufferSizes.ExpectedResponseSize];
        await _socket.ReceiveAsync(buffer, token);

        var status = TcpMessageStreamHelpers.GetResponseStatus(buffer);

        if (status != 0)
        {
            throw new InvalidResponseException($"Invalid response status code: {status}");
        }
    }

    public async Task UpdateTopicAsync(Identifier streamId, Identifier topicId, UpdateTopicRequest request, CancellationToken token = default)
    {
        var message = TcpContracts.UpdateTopic(streamId, topicId, request);
        var payload = new byte[4 + BufferSizes.InitialBytesLength + message.Length];
        TcpMessageStreamHelpers.CreatePayload(payload, message, CommandCodes.UPDATE_TOPIC_CODE);

        await _socket.SendAsync(payload, token);

        var buffer = new byte[BufferSizes.ExpectedResponseSize];
        await _socket.ReceiveAsync(buffer, token);

        var status = TcpMessageStreamHelpers.GetResponseStatus(buffer);
        if (status != 0)
        {
            throw new InvalidResponseException($"Invalid response status code: {status}");
        }
    }

    public async Task DeleteTopicAsync(Identifier streamId, Identifier topicId, CancellationToken token = default)
    {
        var message = TcpContracts.DeleteTopic(streamId, topicId);
        var payload = new byte[4 + BufferSizes.InitialBytesLength + message.Length];
        TcpMessageStreamHelpers.CreatePayload(payload, message, CommandCodes.DELETE_TOPIC_CODE);

        await _socket.SendAsync(payload, token);

        var buffer = new byte[BufferSizes.ExpectedResponseSize];
        await _socket.ReceiveAsync(buffer, token);

        var status = TcpMessageStreamHelpers.GetResponseStatus(buffer);

        if (status != 0)
        {
            throw new InvalidResponseException($"Invalid response status code: {status}");
        }
    }

    public async Task SendMessagesAsync(MessageSendRequest request,
        Func<byte[], byte[]>? encryptor = null,
        CancellationToken token = default)
    {
        if (request.Messages.Count == 0)
        {
            return;
        }

        //TODO - explore making fields of Message class mutable, so there is no need to create em from scratch
        if (encryptor is not null)
        {
            for (var i = 0; i < request.Messages.Count || token.IsCancellationRequested; i++)
            {
                request.Messages[i] = request.Messages[i] with { Payload = encryptor(request.Messages[i].Payload) };
            }
        }

        await _channel.Writer.WriteAsync(request, token);
    }
    public async Task SendMessagesAsync<TMessage>(Identifier streamId, Identifier topicId, Partitioning partitioning,
        IList<TMessage> messages, Func<TMessage, byte[]> serializer,
        Func<byte[], byte[]>? encryptor = null, Dictionary<HeaderKey, HeaderValue>? headers = null,
        CancellationToken token = default)
    {
        if (messages.Count == 0)
        {
            return;
        }

        //TODO - explore making fields of Message class mutable, so there is no need to create em from scratch
        var messagesPool = MemoryPool<Message>.Shared.Rent(messages.Count);
        var messagesBuffer = messagesPool.Memory;
        try
        {
            for (var i = 0; i < messages.Count || token.IsCancellationRequested; i++)
            {
                messagesBuffer.Span[i] = new Message
                {
                    Payload = encryptor is not null ? encryptor(serializer(messages[i])) : serializer(messages[i]),
                    Headers = headers,
                    Id = Guid.NewGuid()
                };
            }

            var request = new MessageSendRequest
            {
                StreamId = streamId,
                TopicId = topicId,
                Partitioning = partitioning,
                Messages = messagesBuffer.Span[..messages.Count].ToArray()
            };
            await _channel.Writer.WriteAsync(request, token);
        }
        finally
        {
            messagesPool.Dispose();
        }
    }
    
    public async Task<PolledMessages<TMessage>> PollMessagesAsync<TMessage>(MessageFetchRequest request,
        Func<byte[], TMessage> serializer, Func<byte[], byte[]>? decryptor = null, CancellationToken token = default)
    {
        await SendMessagesPayload(request, token);
        var buffer = ArrayPool<byte>.Shared.Rent(BufferSizes.ExpectedResponseSize);
        try
        {
            await _socket.ReceiveAsync(buffer.AsMemory()[..BufferSizes.ExpectedResponseSize], token);
            var response = TcpMessageStreamHelpers.GetResponseLengthAndStatus(buffer);
            if (response.Status != 0)
            {
                throw new InvalidResponseException($"Invalid response status code: {response.Status}");
            }

            if (response.Length <= 1)
            {
                return PolledMessages<TMessage>.Empty;
            }

            var responseBuffer = ArrayPool<byte>.Shared.Rent(response.Length);

            try
            {
                await _socket.ReceiveAsync(responseBuffer.AsMemory()[..response.Length], token);
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
    public async Task<PolledMessages> PollMessagesAsync(MessageFetchRequest request,
        Func<byte[], byte[]>? decryptor = null, CancellationToken token = default)
    {
        await SendMessagesPayload(request, token);
        var buffer = ArrayPool<byte>.Shared.Rent(BufferSizes.ExpectedResponseSize);
        try
        {
            await _socket.ReceiveAsync(buffer.AsMemory()[..BufferSizes.ExpectedResponseSize], token);

            var response = TcpMessageStreamHelpers.GetResponseLengthAndStatus(buffer);
            if (response.Status != 0)
            {
                throw new InvalidResponseException($"Invalid response status code: {response.Status}");
            }

            if (response.Length <= 1)
            {
                return PolledMessages.Empty;
            }

            var responseBuffer = ArrayPool<byte>.Shared.Rent(response.Length);

            try
            {
                await _socket.ReceiveAsync(responseBuffer.AsMemory()[..response.Length], token);
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
    private async Task SendMessagesPayload(MessageFetchRequest request, CancellationToken token)
    {
        var messageBufferSize = CalculateMessageBufferSize(request);
        var payloadBufferSize = CalculatePayloadBufferSize(messageBufferSize);
        var message = ArrayPool<byte>.Shared.Rent(messageBufferSize);
        var payload = ArrayPool<byte>.Shared.Rent(payloadBufferSize);
        
        try
        {
            TcpContracts.GetMessages(message.AsSpan()[..messageBufferSize], request);
            TcpMessageStreamHelpers.CreatePayload(payload, message.AsSpan()[..messageBufferSize], CommandCodes.POLL_MESSAGES_CODE);

            await _socket.SendAsync(payload.AsMemory()[..payloadBufferSize], token);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(message);
            ArrayPool<byte>.Shared.Return(payload);
        }
    }
    private static int CalculatePayloadBufferSize(int messageBufferSize)
        => messageBufferSize + 4 + BufferSizes.InitialBytesLength;
    private static int CalculateMessageBufferSize(MessageFetchRequest request)
        => 18 + 5 + 2 + request.StreamId.Length + 2 + request.TopicId.Length;
    
    public async Task StoreOffsetAsync(StoreOffsetRequest request, CancellationToken token = default)
    {
        var message = TcpContracts.UpdateOffset(request);
        var payload = new byte[4 + BufferSizes.InitialBytesLength + message.Length];
        TcpMessageStreamHelpers.CreatePayload(payload, message, CommandCodes.STORE_OFFSET_CODE);

        await _socket.SendAsync(payload, token);

        var buffer = new byte[BufferSizes.ExpectedResponseSize];
        await _socket.ReceiveAsync(buffer, token);

        var status = TcpMessageStreamHelpers.GetResponseStatus(buffer);

        if (status != 0)
        {
            throw new InvalidResponseException($"Invalid response status code: {status}");
        }
    }

    public async Task<OffsetResponse?> GetOffsetAsync(OffsetRequest request, CancellationToken token = default)
    {
        var message = TcpContracts.GetOffset(request);
        var payload = new byte[4 + BufferSizes.InitialBytesLength + message.Length];
        TcpMessageStreamHelpers.CreatePayload(payload, message, CommandCodes.GET_OFFSET_CODE);

        await _socket.SendAsync(payload, token);

        var buffer = new byte[BufferSizes.ExpectedResponseSize];
        await _socket.ReceiveAsync(buffer, token);

        var response = TcpMessageStreamHelpers.GetResponseLengthAndStatus(buffer);

        if (response.Status != 0)
        {
            throw new InvalidResponseException($"Invalid response status code: {response.Status}");
        }

        if (response.Length <= 1)
        {
            return null;
        }

        var responseBuffer = new byte[response.Length];
        await _socket.ReceiveAsync(responseBuffer, token);
        return BinaryMapper.MapOffsets(responseBuffer);
    }

    public async Task<IReadOnlyList<ConsumerGroupResponse>> GetConsumerGroupsAsync(Identifier streamId, Identifier topicId,
        CancellationToken token = default)
    {
        var message = TcpContracts.GetGroups(streamId, topicId);
        var payload = new byte[4 + BufferSizes.InitialBytesLength + message.Length];
        TcpMessageStreamHelpers.CreatePayload(payload, message, CommandCodes.GET_GROUPS_CODE);

        await _socket.SendAsync(payload, token);

        var buffer = new byte[BufferSizes.ExpectedResponseSize];
        await _socket.ReceiveAsync(buffer, token);

        var response = TcpMessageStreamHelpers.GetResponseLengthAndStatus(buffer);

        if (response.Status != 0)
        {
            throw new InvalidResponseException($"Invalid response status code: {response.Status}");
        }

        if (response.Length <= 1)
        {
            return EmptyList<ConsumerGroupResponse>.Instance;
        }

        var responseBuffer = new byte[response.Length];
        await _socket.ReceiveAsync(responseBuffer, token);
        return BinaryMapper.MapConsumerGroups(responseBuffer);
    }

    public async Task<ConsumerGroupResponse?> GetConsumerGroupByIdAsync(Identifier streamId, Identifier topicId,
        int groupId, CancellationToken token = default)
    {
        var message = TcpContracts.GetGroup(streamId, topicId, groupId);
        var payload = new byte[4 + BufferSizes.InitialBytesLength + message.Length];
        TcpMessageStreamHelpers.CreatePayload(payload, message, CommandCodes.GET_GROUP_CODE);

        await _socket.SendAsync(payload, token);

        var buffer = new byte[BufferSizes.ExpectedResponseSize];
        await _socket.ReceiveAsync(buffer, token);

        var response = TcpMessageStreamHelpers.GetResponseLengthAndStatus(buffer);

        if (response.Status != 0)
        {
            throw new InvalidResponseException($"Invalid response status code: {response.Status}");
        }

        if (response.Length <= 1)
        {
            return null;
        }

        var responseBuffer = new byte[response.Length];
        await _socket.ReceiveAsync(responseBuffer, token);
        return BinaryMapper.MapConsumerGroup(responseBuffer);
    }

    public async Task CreateConsumerGroupAsync(CreateConsumerGroupRequest request, CancellationToken token = default)
    {
        var message = TcpContracts.CreateGroup(request);
        var payload = new byte[4 + BufferSizes.InitialBytesLength + message.Length];
        TcpMessageStreamHelpers.CreatePayload(payload, message, CommandCodes.CREATE_GROUP_CODE);

        await _socket.SendAsync(payload, token);

        var buffer = new byte[BufferSizes.ExpectedResponseSize];
        await _socket.ReceiveAsync(buffer, token);

        var status = TcpMessageStreamHelpers.GetResponseStatus(buffer);

        if (status != 0)
        {
            throw new InvalidResponseException($"Invalid response status code: {status}");
        }
    }

    public async Task DeleteConsumerGroupAsync(DeleteConsumerGroupRequest request, CancellationToken token = default)

    {
        var message = TcpContracts.DeleteGroup(request.StreamId, request.TopicId, request.ConsumerGroupId);
        var payload = new byte[4 + BufferSizes.InitialBytesLength + message.Length];
        TcpMessageStreamHelpers.CreatePayload(payload, message, CommandCodes.DELETE_GROUP_CODE);

        await _socket.SendAsync(payload, token);

        var buffer = new byte[BufferSizes.ExpectedResponseSize];
        await _socket.ReceiveAsync(buffer, token);

        var status = TcpMessageStreamHelpers.GetResponseStatus(buffer);

        if (status != 0)
        {
            throw new InvalidResponseException($"Invalid response status code: {status}");
        }
    }

    public async Task JoinConsumerGroupAsync(JoinConsumerGroupRequest request, CancellationToken token = default)
    {
        var message = TcpContracts.JoinGroup(request);
        var payload = new byte[4 + BufferSizes.InitialBytesLength + message.Length];
        TcpMessageStreamHelpers.CreatePayload(payload, message, CommandCodes.JOIN_GROUP_CODE);

        await _socket.SendAsync(payload, token);

        var buffer = new byte[BufferSizes.ExpectedResponseSize];
        await _socket.ReceiveAsync(buffer, token);

        var status = TcpMessageStreamHelpers.GetResponseStatus(buffer);

        if (status != 0)
        {
            throw new InvalidResponseException($"Invalid response status code: {status}");
        }
    }

    public async Task LeaveConsumerGroupAsync(LeaveConsumerGroupRequest request, CancellationToken token = default)
    {
        var message = TcpContracts.LeaveGroup(request);
        var payload = new byte[4 + BufferSizes.InitialBytesLength + message.Length];
        TcpMessageStreamHelpers.CreatePayload(payload, message, CommandCodes.LEAVE_GROUP_CODE);

        await _socket.SendAsync(payload, token);

        var buffer = new byte[BufferSizes.ExpectedResponseSize];
        await _socket.ReceiveAsync(buffer, token);

        var status = TcpMessageStreamHelpers.GetResponseStatus(buffer);

        if (status != 0)
        {
            throw new InvalidResponseException($"Invalid response status code: {status}");
        }
    }
    public async Task DeletePartitionsAsync(DeletePartitionsRequest request,
        CancellationToken token = default)
    {
        var message = TcpContracts.DeletePartitions(request);
        var payload = new byte[4 + BufferSizes.InitialBytesLength + message.Length];
        TcpMessageStreamHelpers.CreatePayload(payload, message, CommandCodes.DELETE_PARTITIONS_CODE);

        await _socket.SendAsync(payload, token);

        var buffer = new byte[BufferSizes.ExpectedResponseSize];
        await _socket.ReceiveAsync(buffer, token);

        var status = TcpMessageStreamHelpers.GetResponseStatus(buffer);

        if (status != 0)
        {
            throw new InvalidResponseException($"Invalid response status code: {status}");
        }
    }

    public async Task CreatePartitionsAsync(CreatePartitionsRequest request,
        CancellationToken token = default)
    {
        var message = TcpContracts.CreatePartitions(request);
        var payload = new byte[4 + BufferSizes.InitialBytesLength + message.Length];
        TcpMessageStreamHelpers.CreatePayload(payload, message, CommandCodes.CREATE_PARTITIONS_CODE);

        await _socket.SendAsync(payload, token);

        var buffer = new byte[BufferSizes.ExpectedResponseSize];
        await _socket.ReceiveAsync(buffer, token);

        var status = TcpMessageStreamHelpers.GetResponseStatus(buffer);

        if (status != 0)
        {
            throw new InvalidResponseException($"Invalid response status code: {status}");
        }
    }
    public async Task<Stats?> GetStatsAsync(CancellationToken token = default)
    {
        var message = Array.Empty<byte>();
        var payload = new byte[4 + BufferSizes.InitialBytesLength + message.Length];
        TcpMessageStreamHelpers.CreatePayload(payload, message, CommandCodes.GET_STATS_CODE);

        await _socket.SendAsync(payload, token);

        var buffer = new byte[BufferSizes.ExpectedResponseSize];
        await _socket.ReceiveAsync(buffer, token);

        var response = TcpMessageStreamHelpers.GetResponseLengthAndStatus(buffer);

        if (response.Status != 0)
        {
            throw new InvalidResponseException($"Invalid response status code: {response.Status}");
        }

        if (response.Length <= 1)
        {
            return null;
        }

        var responseBuffer = new byte[response.Length];
        await _socket.ReceiveAsync(responseBuffer, token);

        return BinaryMapper.MapStats(responseBuffer);
    }

    public void Dispose()
    {
        _socket.Close();
        _socket.Dispose();
    }

}