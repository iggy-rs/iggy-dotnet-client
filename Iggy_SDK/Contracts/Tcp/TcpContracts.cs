using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;
using Iggy_SDK.Headers;
using Iggy_SDK.Kinds;
using Iggy_SDK.Messages;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Iggy_SDK.Contracts.Tcp;

internal static class TcpContracts
{
    internal static void GetMessages(Span<byte> bytes, MessageFetchRequest request)
    {
        bytes[0] = GetConsumerTypeByte(request.Consumer.Type);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[1..5], request.Consumer.Id);
        WriteBytesFromStreamAndTopicIdToSpan(request.StreamId, request.TopicId, bytes, 5);
        var position = 5 + 2 + request.StreamId.Length + 2 + request.TopicId.Length;
        BinaryPrimitives.WriteInt32LittleEndian(bytes[position..(position + 4)], request.PartitionId);
        bytes[position + 4] = GetPollingStrategyByte(request.PollingStrategy.Kind);
        BinaryPrimitives.WriteUInt64LittleEndian(bytes[(position + 5)..(position + 13)], request.PollingStrategy.Value);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[(position + 13)..(position + 17)], request.Count);

        bytes[position + 17] = request.AutoCommit ? (byte)1 : (byte)0;
    }
    internal static void GetMessagesLazy(Span<byte> bytes, MessageFetchRequest request)
    {
        bytes[0] = GetConsumerTypeByte(request.Consumer.Type);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[1..5], request.Consumer.Id);
        WriteBytesFromStreamAndTopicIdToSpan(request.StreamId, request.TopicId, bytes, 5);
        var position = 5 + 2 + request.StreamId.Length + 2 + request.TopicId.Length;
        BinaryPrimitives.WriteInt32LittleEndian(bytes[position..(position + 4)], request.PartitionId);
        bytes[position + 4] = GetPollingStrategyByte(request.PollingStrategy.Kind);
        BinaryPrimitives.WriteUInt64LittleEndian(bytes[(position + 5)..(position + 13)], request.PollingStrategy.Value);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[(position + 13)..(position + 17)], 1);

        bytes[position + 17] = request.AutoCommit ? (byte)1 : (byte)0;
    }
    //TODO - since message is of type IList maybe I can simplife the HandleMessages methods.
    internal static void CreateMessage(Span<byte> bytes, Identifier streamId, Identifier topicId,
        Partitioning partitioning, IList<Message> messages)
    {
        WriteBytesFromStreamAndTopicIdToSpan(streamId, topicId, bytes);
        int streamTopicIdPosition = 2 + streamId.Length + 2 + topicId.Length;
        bytes[streamTopicIdPosition] = GetPartitioningKindByte(partitioning.Kind);
        bytes[streamTopicIdPosition + 1] = (byte)partitioning.Length;
        partitioning.Value.CopyTo(bytes[(streamTopicIdPosition + 2)..(streamTopicIdPosition + partitioning.Length + 2)]);

        var position = 2 + partitioning.Length + streamTopicIdPosition;
        bytes = messages switch
        {
            Message[] messagesArray => HandleMessagesArray(position, messagesArray, bytes),
            List<Message> messagesList => HandleMessagesList(position, messagesList, bytes),
            _ => HandleMessagesEnumerable(position, messages, bytes),
        };
    }
    private static Span<byte> HandleMessagesEnumerable(int position, IEnumerable<Message> messages, Span<byte> bytes)
    {
        Span<byte> emptyHeaders = stackalloc byte[4];

        foreach (var message in messages)
        {
            var idSlice = bytes[position..(position + 16)];
            Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(idSlice), message.Id);

            if (message.Headers is not null)
            {
                var headersBytes = GetHeadersBytes(message.Headers);
                BinaryPrimitives.WriteInt32LittleEndian(bytes[(position + 16)..(position + 20)], headersBytes.Length);
                headersBytes.CopyTo(bytes[(position + 20)..(position + 20 + headersBytes.Length)]);
                position += headersBytes.Length + 20;
            }
            else
            {
                emptyHeaders.CopyTo(bytes[(position + 16)..(position + 16 + emptyHeaders.Length)]);
                position += 20;
            }

            BinaryPrimitives.WriteInt32LittleEndian(bytes[(position)..(position + 4)], message.Payload.Length);
            var payloadBytes = message.Payload;
            var slice = bytes[(position + 4)..];
            payloadBytes.CopyTo(slice);
            position += payloadBytes.Length + 4;
        }

        return bytes;
    }
    private static Span<byte> HandleMessagesArray(int position, Message[] messages, Span<byte> bytes)
    {
        Span<byte> emptyHeaders = stackalloc byte[4];

        ref var start = ref MemoryMarshal.GetArrayDataReference(messages);
        ref var end = ref Unsafe.Add(ref start, messages.Length);
        while (Unsafe.IsAddressLessThan(ref start, ref end))
        {
            var idSlice = bytes[position..(position + 16)];
            Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(idSlice), start.Id);

            if (start.Headers is not null)
            {
                var headersBytes = GetHeadersBytes(start.Headers);
                BinaryPrimitives.WriteInt32LittleEndian(bytes[(position + 16)..(position + 20)], headersBytes.Length);
                headersBytes.CopyTo(bytes[(position + 20)..(position + 20 + headersBytes.Length)]);
                position += headersBytes.Length + 20;
            }
            else
            {
                emptyHeaders.CopyTo(bytes[(position + 16)..(position + 16 + emptyHeaders.Length)]);
                position += 20;
            }

            BinaryPrimitives.WriteInt32LittleEndian(bytes[(position)..(position + 4)], start.Payload.Length);
            var payloadBytes = start.Payload;
            var slice = bytes[(position + 4)..];
            payloadBytes.CopyTo(slice);
            position += payloadBytes.Length + 4;

            start = ref Unsafe.Add(ref start, 1);
        }

        return bytes;
    }

    private static Span<byte> HandleMessagesList(int position, List<Message> messages, Span<byte> bytes)
    {
        Span<byte> emptyHeaders = stackalloc byte[4];

        Span<Message> listAsSpan = CollectionsMarshal.AsSpan(messages);
        ref var start = ref MemoryMarshal.GetReference(listAsSpan);
        ref var end = ref Unsafe.Add(ref start, listAsSpan.Length);
        while (Unsafe.IsAddressLessThan(ref start, ref end))
        {
            var idSlice = bytes[position..(position + 16)];
            Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(idSlice), start.Id);

            if (start.Headers is not null)
            {
                var headersBytes = GetHeadersBytes(start.Headers);
                BinaryPrimitives.WriteInt32LittleEndian(bytes[(position + 16)..(position + 20)], headersBytes.Length);
                headersBytes.CopyTo(bytes[(position + 20)..(position + 20 + headersBytes.Length)]);
                position += headersBytes.Length + 20;
            }
            else
            {
                emptyHeaders.CopyTo(bytes[(position + 16)..(position + 16 + emptyHeaders.Length)]);
                position += 20;
            }

            BinaryPrimitives.WriteInt32LittleEndian(bytes[(position)..(position + 4)], start.Payload.Length);
            var payloadBytes = start.Payload;
            var slice = bytes[(position + 4)..];
            payloadBytes.CopyTo(slice);
            position += payloadBytes.Length + 4;

            start = ref Unsafe.Add(ref start, 1);
        }

        return bytes;
    }

    private static byte[] GetHeadersBytes(Dictionary<HeaderKey, HeaderValue> headers)
    {
        var headersLength = headers.Sum(header => 4 + header.Key.Value.Length + 1 + 4 + header.Value.Value.Length);
        Span<byte> headersBytes = stackalloc byte[headersLength];
        int position = 0;
        foreach (var (headerKey, headerValue) in headers)
        {
            var headerBytes = GetBytesFromHeader(headerKey, headerValue);
            headerBytes.CopyTo(headersBytes[position..(position + headerBytes.Length)]);
            position += headerBytes.Length;

        }
        return headersBytes.ToArray();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte HeaderKindToByte(HeaderKind kind)
    {
        return kind switch
        {
            HeaderKind.Raw => 1,
            HeaderKind.String => 2,
            HeaderKind.Bool => 3,
            HeaderKind.Int32 => 6,
            HeaderKind.Int64 => 7,
            HeaderKind.Int128 => 8,
            HeaderKind.Uint32 => 11,
            HeaderKind.Uint64 => 12,
            HeaderKind.Uint128 => 13,
            HeaderKind.Float32 => 14,
            HeaderKind.Float64 => 15,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
    }
    private static byte[] GetBytesFromHeader(HeaderKey headerKey, HeaderValue headerValue)
    {
        var headerBytesLength = 4 + headerKey.Value.Length + 1 + 4 + headerValue.Value.Length;
        Span<byte> headerBytes = stackalloc byte[headerBytesLength];

        BinaryPrimitives.WriteInt32LittleEndian(headerBytes[..4], headerKey.Value.Length);
        var headerKeyBytes = Encoding.UTF8.GetBytes(headerKey.Value);
        headerKeyBytes.CopyTo(headerBytes[4..(4 + headerKey.Value.Length)]);

        headerBytes[4 + headerKey.Value.Length] = HeaderKindToByte(headerValue.Kind);

        BinaryPrimitives.WriteInt32LittleEndian(
            headerBytes[(4 + headerKey.Value.Length + 1)..(4 + headerKey.Value.Length + 1 + 4)],
            headerValue.Value.Length);
        headerValue.Value.CopyTo(headerBytes[(4 + headerKey.Value.Length + 1 + 4)..]);

        return headerBytes.ToArray();
    }
    internal static byte[] CreateStream(StreamRequest request)
    {
        Span<byte> bytes = stackalloc byte[4 + request.Name.Length + 1];
        BinaryPrimitives.WriteInt32LittleEndian(bytes[..4], request.StreamId);
        bytes[4] = (byte)request.Name.Length;
        Encoding.UTF8.GetBytes(request.Name, bytes[5..]);
        return bytes.ToArray();
    }

    internal static byte[] CreateGroup(CreateConsumerGroupRequest request)
    {
        Span<byte> bytes = stackalloc byte[2 + request.StreamId.Length + 2 + request.TopicId.Length + sizeof(int)];
        WriteBytesFromStreamAndTopicIdToSpan(request.StreamId, request.TopicId, bytes);
        int position = 2 + request.StreamId.Length + 2 + request.TopicId.Length;
        BinaryPrimitives.WriteInt32LittleEndian(bytes[position..(position + 4)], request.ConsumerGroupId);
        return bytes.ToArray();
    }

    internal static byte[] JoinGroup(JoinConsumerGroupRequest request)
    {
        Span<byte> bytes = stackalloc byte[2 + request.StreamId.Length + 2 + request.TopicId.Length + sizeof(int)];
        WriteBytesFromStreamAndTopicIdToSpan(request.StreamId, request.TopicId, bytes);
        int position = 2 + request.StreamId.Length + 2 + request.TopicId.Length;
        BinaryPrimitives.WriteInt32LittleEndian(bytes[position..(position + 4)], request.ConsumerGroupId);
        return bytes.ToArray();
    }
    internal static byte[] LeaveGroup(LeaveConsumerGroupRequest request)
    {
        Span<byte> bytes = stackalloc byte[2 + request.StreamId.Length + 2 + request.TopicId.Length + sizeof(int)];
        WriteBytesFromStreamAndTopicIdToSpan(request.StreamId, request.TopicId, bytes);
        int position = 2 + request.StreamId.Length + 2 + request.TopicId.Length;
        BinaryPrimitives.WriteInt32LittleEndian(bytes[position..(position + 4)], request.ConsumerGroupId);
        return bytes.ToArray();
    }
    internal static byte[] DeleteGroup(Identifier streamId, Identifier topicId, int groupId)
    {
        Span<byte> bytes = stackalloc byte[2 + streamId.Length + 2 + topicId.Length + sizeof(int)];
        WriteBytesFromStreamAndTopicIdToSpan(streamId, topicId, bytes);
        int position = 2 + streamId.Length + 2 + topicId.Length;
        BinaryPrimitives.WriteInt32LittleEndian(bytes[position..(position + 4)], groupId);
        return bytes.ToArray();
    }

    internal static byte[] GetGroups(Identifier streamId, Identifier topicId)
    {
        Span<byte> bytes = stackalloc byte[2 + streamId.Length + 2 + topicId.Length];
        WriteBytesFromStreamAndTopicIdToSpan(streamId, topicId, bytes);
        return bytes.ToArray();
    }

    internal static byte[] GetGroup(Identifier streamId, Identifier topicId, int groupId)
    {
        Span<byte> bytes = stackalloc byte[2 + streamId.Length + 2 + topicId.Length + sizeof(int)];
        WriteBytesFromStreamAndTopicIdToSpan(streamId, topicId, bytes);
        int position = 2 + streamId.Length + 2 + topicId.Length;
        BinaryPrimitives.WriteInt32LittleEndian(bytes[position..(position + 4)], groupId);
        return bytes.ToArray();
    }

    internal static byte[] CreateTopic(Identifier streamId, TopicRequest request)
    {
        Span<byte> bytes = stackalloc byte[2 + streamId.Length + 13 + request.Name.Length];
        WriteBytesFromIdentifierToSpan(streamId, bytes);
        var streamIdBytesLength = 2 + streamId.Length;
        BinaryPrimitives.WriteInt32LittleEndian(bytes[streamIdBytesLength..(streamIdBytesLength + 4)], request.TopicId);
        int position = 4 + streamIdBytesLength;
        BinaryPrimitives.WriteInt32LittleEndian(bytes[position..(position + 4)], request.PartitionsCount);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[(position + 4)..(position + 8)], request.MessageExpiry ?? 0);
        bytes[position + 8] = (byte)request.Name.Length;
        Encoding.UTF8.GetBytes(request.Name, bytes[(position + 9)..]);
        return bytes.ToArray();
    }

    internal static byte[] GetTopicById(Identifier streamId, Identifier topicId)
    {
        Span<byte> bytes = stackalloc byte[2 + streamId.Length + 2 + topicId.Length];
        WriteBytesFromStreamAndTopicIdToSpan(streamId, topicId, bytes);
        return bytes.ToArray();
    }


    internal static byte[] DeleteTopic(Identifier streamId, Identifier topicId)
    {
        Span<byte> bytes = stackalloc byte[2 + streamId.Length + 2 + topicId.Length];
        WriteBytesFromStreamAndTopicIdToSpan(streamId, topicId, bytes);
        return bytes.ToArray();
    }

    internal static byte[] UpdateOffset(StoreOffsetRequest request)
    {
        Span<byte> bytes =
            stackalloc byte[2 + request.StreamId.Length + 2 + request.TopicId.Length + 17];
        bytes[0] = GetConsumerTypeByte(request.Consumer.Type);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[1..5], request.Consumer.Id);
        WriteBytesFromStreamAndTopicIdToSpan(request.StreamId, request.TopicId, bytes, 5);
        var position = 5 + 2 + request.StreamId.Length + 2 + request.TopicId.Length;
        BinaryPrimitives.WriteInt32LittleEndian(bytes[position..(position + 4)], request.PartitionId);
        BinaryPrimitives.WriteUInt64LittleEndian(bytes[(position + 4)..(position + 12)], request.Offset);
        return bytes.ToArray();
    }

    internal static byte[] GetOffset(OffsetRequest request)
    {
        Span<byte> bytes =
            stackalloc byte[2 + request.StreamId.Length + 2 + request.TopicId.Length + sizeof(int) * 2 + 1];
        bytes[0] = GetConsumerTypeByte(request.Consumer.Type);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[1..5], request.Consumer.Id);
        WriteBytesFromStreamAndTopicIdToSpan(request.StreamId, request.TopicId, bytes, 5);
        var position = 5 + 2 + request.StreamId.Length + 2 + request.TopicId.Length;
        BinaryPrimitives.WriteInt32LittleEndian(bytes[position..(position + 4)], request.PartitionId);
        return bytes.ToArray();
    }

    internal static byte[] CreatePartitions(CreatePartitionsRequest request)
    {
        Span<byte> bytes = stackalloc byte[2 + request.StreamId.Length + 2 + request.TopicId.Length + sizeof(int)];
        WriteBytesFromStreamAndTopicIdToSpan(request.StreamId, request.TopicId, bytes);
        int position = 2 + request.StreamId.Length + 2 + request.TopicId.Length;
        BinaryPrimitives.WriteInt32LittleEndian(bytes[position..(position + 4)], request.PartitionsCount);
        return bytes.ToArray();
    }

    internal static byte[] DeletePartitions(DeletePartitionsRequest request)
    {
        Span<byte> bytes = stackalloc byte[2 + request.StreamId.Length + 2 + request.TopicId.Length + sizeof(int)];
        WriteBytesFromStreamAndTopicIdToSpan(request.StreamId, request.TopicId, bytes);
        int position = 2 + request.StreamId.Length + 2 + request.TopicId.Length;
        BinaryPrimitives.WriteInt32LittleEndian(bytes[position..(position + 4)], request.PartitionsCount);
        return bytes.ToArray();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte GetConsumerTypeByte(ConsumerType type)
    {
        return type switch
        {
            ConsumerType.Consumer => 1,
            ConsumerType.ConsumerGroup => 2,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte GetIdKindByte(IdKind kind)
    {
        return kind switch
        {
            IdKind.Numeric => 1,
            IdKind.String => 2,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte GetPartitioningKindByte(PartitioningKind kind)
    {
        return kind switch
        {
            PartitioningKind.Balanced => 1,
            PartitioningKind.PartitionId => 2,
            PartitioningKind.MessageKey => 3,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte GetPollingStrategyByte(MessagePolling pollingStrategy)
    {
        return pollingStrategy switch
        {
            MessagePolling.Offset => 1,
            MessagePolling.Timestamp => 2,
            MessagePolling.First => 3,
            MessagePolling.Last => 4,
            MessagePolling.Next => 5,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteBytesFromIdentifierToSpan(Identifier identifier, Span<byte> bytes)
    {
        bytes[0] = GetIdKindByte(identifier.Kind);
        bytes[1] = (byte)identifier.Length;
        identifier.Value.CopyTo(bytes[2..]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteBytesFromStreamAndTopicIdToSpan(Identifier streamId, Identifier topicId, Span<byte> bytes, int startPos = 0)
    {
        bytes[startPos] = GetIdKindByte(streamId.Kind);
        bytes[startPos + 1] = (byte)streamId.Length;
        streamId.Value.CopyTo(bytes[(startPos + 2)..(startPos + 2 + streamId.Length)]);

        var position = startPos + 2 + streamId.Length;
        bytes[position] = GetIdKindByte(topicId.Kind);
        bytes[position + 1] = (byte)topicId.Length;
        topicId.Value.CopyTo(bytes[(position + 2)..(position + 2 + topicId.Length)]);
    }
}