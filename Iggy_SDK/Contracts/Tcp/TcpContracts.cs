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
    internal static byte[] GetClient(uint clientId)
    {
        var bytes = new byte[4];
        BinaryPrimitives.WriteUInt32LittleEndian(bytes, clientId);
        return bytes;
    }
    internal static byte[] GetUser(Identifier userId)
    {
        Span<byte> bytes = stackalloc byte[userId.Length + 2];
        WriteBytesFromIdentifierToSpan(userId, bytes);
        return bytes.ToArray();
    }
    internal static byte[] DeleteUser(Identifier userId)
    {
        Span<byte> bytes = stackalloc byte[userId.Length + 2];
        WriteBytesFromIdentifierToSpan(userId, bytes);
        return bytes.ToArray();
    }
    internal static byte[] UpdateUser(UpdateUserRequest request)
    {
        var length = request.UserId.Length + 2 + (request.Username?.Length ?? 0)
                     + (request.UserStatus is not null ? 2 : 1) + 1 + 4;
        Span<byte> bytes = stackalloc byte[length];
        
        WriteBytesFromIdentifierToSpan(request.UserId, bytes);
        int position = request.UserId.Length + 2;
        if (request.Username is not null)
        {
            bytes[position] = 1;
            position += 1;
            BinaryPrimitives.WriteInt32LittleEndian(bytes[position..(position + 4)], 
                request.Username.Length);
            Encoding.UTF8.GetBytes(request.Username, 
                bytes[(position + 4)..(position + 4 + request.Username.Length)]);
            position += 4 + request.Username.Length;
        }
        else
        {
           bytes[request.UserId.Length] = 0;
           position += 1;
        }
        
        if (request.UserStatus is not null)
        {
            bytes[position++] = 1;
            bytes[position++] = request.UserStatus switch
            {
                UserStatus.Active => 1,
                UserStatus.Inactive => 2,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        else
        {
            bytes[position++] = 0;
        }
        return bytes.ToArray();
    }
    internal static byte[] CreateUser(CreateUserRequest request)
    {
        int capacity = 4 + request.Username.Length + request.Password.Length 
            + (request.Permissions is not null ? 1 + 4 + CalculatePermissionsSize(request.Permissions) : 0); // +1 for status byte

        Span<byte> bytes = stackalloc byte[capacity];
        int index = 0;

        bytes[index++] = (byte)request.Username.Length;
        index += Encoding.UTF8.GetBytes(request.Username, bytes[index..(index + request.Username.Length)]);

        bytes[index++] = (byte)request.Password.Length;
        index += Encoding.UTF8.GetBytes(request.Password, bytes[index..(index + request.Password.Length)]);

        bytes[index++] = request.Status switch
        {
            UserStatus.Active => (byte)1,
            UserStatus.Inactive => (byte)2,
            _ => throw new ArgumentOutOfRangeException()
        }; 

        if (request.Permissions is not null)
        {
            bytes[index++] = 1;
            var permissions = GetBytesFromPermissions(request.Permissions);
            BinaryPrimitives.WriteInt32LittleEndian(bytes[index..(index + 4)], CalculatePermissionsSize(request.Permissions));
            index += 4;
            permissions.CopyTo(bytes[index..(index + permissions.Length)]);
            index += permissions.Length;
        }
        else
        {
            bytes[index++] = 0;
        }

        return bytes.ToArray();
    }
    private static byte[] GetBytesFromPermissions(Permissions data)
    {
        int size = CalculatePermissionsSize(data);
        Span<byte> bytes = stackalloc byte[size];

        bytes[0] = data.Global.ManageServers ? (byte)1 : (byte)0;
        bytes[1] = data.Global.ReadServers ? (byte)1 : (byte)0;
        bytes[2] = data.Global.ManageUsers ? (byte)1 : (byte)0;
        bytes[3] = data.Global.ReadUsers ? (byte)1 : (byte)0;
        bytes[4] = data.Global.ManageStreams ? (byte)1 : (byte)0;
        bytes[5] = data.Global.ReadStreams ? (byte)1 : (byte)0;
        bytes[6] = data.Global.ManageTopics ? (byte)1 : (byte)0;
        bytes[7] = data.Global.ReadTopics ? (byte)1 : (byte)0;
        bytes[8] = data.Global.PollMessages ? (byte)1 : (byte)0;
        bytes[9] = data.Global.SendMessages ? (byte)1 : (byte)0;


        if (data.Streams is not null)
        {
            bytes[10] = (byte)1;
            int position = 11;
            foreach (var (streamId, stream) in data.Streams)
            {
                BinaryPrimitives.WriteInt32LittleEndian(bytes[position..(position + 4)], streamId);
                position += 4;

                bytes[position] = stream.ManageStream ? (byte)1 : (byte)0;
                bytes[position + 1] = stream.ReadStream ? (byte)1 : (byte)0;
                bytes[position + 2] = stream.ManageTopics ? (byte)1 : (byte)0;
                bytes[position + 3] = stream.ReadTopics ? (byte)1 : (byte)0;
                bytes[position + 4] = stream.PollMessages ? (byte)1 : (byte)0;
                bytes[position + 5] = stream.SendMessages ? (byte)1 : (byte)0;
                position += 6;

                if (stream.Topics != null)
                {
                    bytes[position] = (byte)1;
                    position += 1;

                    foreach (var (topicId, topic) in stream.Topics)
                    {
                        BinaryPrimitives.WriteInt32LittleEndian(bytes[position..(position + 4)], topicId);
                        position += 4;
                        
                        bytes[position] = topic.ManageTopic ? (byte)1 : (byte)0;
                        bytes[position + 1] = topic.ReadTopic ? (byte)1 : (byte)0;
                        bytes[position + 2] = topic.PollMessages ? (byte)1 : (byte)0;
                        bytes[position + 3] = topic.SendMessages ? (byte)1 : (byte)0;
                        position += 4;
                    }
                }
                else
                {
                    bytes[0] = (byte)0;
                    bytes = bytes[1..];
                }
            }
        }
        else
        {
            bytes[0] = (byte)0;
        }

        return bytes.ToArray();
    }
    private static int CalculatePermissionsSize(Permissions data)
    {
        int size = 10; 

        if (data.Streams != null)
        {
            size += 3; 
            foreach (var (_, stream) in data.Streams)
            {
                size += 4; 
                size += 6; 

                if (stream.Topics is not null)
                {
                    size += 3; 
                    size += stream.Topics.Count * 8; 
                }
                else
                {
                    size += 1; 
                }
            }
        }
        else
        {
            size += 1; 
        }

        return size;
    }
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
            _ => HandleMessagesIList(position, messages, bytes),
        };
    }
    private static Span<byte> HandleMessagesIList(int position, IList<Message> messages, Span<byte> bytes)
    {
        Span<byte> emptyHeaders = stackalloc byte[4];
        
        foreach (var message in messages)
        {
            var idSlice = bytes[position..(position + 16)];
            //TODO - this required testing on different cpu architectures
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
            //TODO - this required testing on different cpu architectures
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
            //TODO - this required testing on different cpu architectures
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
            HeaderKind.Float => 14,
            HeaderKind.Double => 15,
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
    internal static byte[] UpdateStream(Identifier streamId, UpdateStreamRequest request)
    {
        Span<byte> bytes = stackalloc byte[streamId.Length + request.Name.Length + 3];
        WriteBytesFromIdentifierToSpan(streamId, bytes);
        int position = 2 + streamId.Length;
        bytes[position] = (byte)request.Name.Length;
        Encoding.UTF8.GetBytes(request.Name, bytes[(position + 1)..]);
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

    internal static byte[] UpdateTopic(Identifier streamId, Identifier topicId, UpdateTopicRequest request)
    {
        Span<byte> bytes = stackalloc byte[streamId.Length + topicId.Length + 9 + request.Name.Length];
        WriteBytesFromStreamAndTopicIdToSpan(streamId, topicId, bytes);
        var position = 4 + streamId.Length + topicId.Length;
        BinaryPrimitives.WriteInt32LittleEndian(bytes[position..(position + 4)],
            request.MessageExpiry ?? 0);
        bytes[position + 4] = (byte)request.Name.Length;
        Encoding.UTF8.GetBytes(request.Name, bytes[(position + 5)..]);
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