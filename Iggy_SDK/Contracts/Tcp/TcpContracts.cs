using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;
using Iggy_SDK.Identifiers;
using Iggy_SDK.Messages;

namespace Iggy_SDK.Contracts.Tcp;

internal static class TcpContracts
{
    internal static void GetMessages(Span<byte> bytes, MessageFetchRequest request)
    {

        bytes[0] = request.Consumer.Type switch
        {
            ConsumerType.Consumer => 1,
            ConsumerType.ConsumerGroup => 2,
            _ => throw new ArgumentOutOfRangeException()
        };
        BinaryPrimitives.WriteInt32LittleEndian(bytes[1..5], request.Consumer.Id);
        WriteBytesFromStreamAndTopicIdToSpan(request.StreamId, request.TopicId, bytes, 5);
        var position = 5 + 2 + request.StreamId.Length + 2 + request.TopicId.Length;
        BinaryPrimitives.WriteInt32LittleEndian(bytes[position..(position + 4)], request.PartitionId);
        bytes[position + 4] = request.PollingStrategy switch
        {
            MessagePolling.Offset => 0,
            MessagePolling.Timestamp => 1,
            MessagePolling.First => 2,
            MessagePolling.Last => 3,
            MessagePolling.Next => 4,
            _ => throw new ArgumentOutOfRangeException()
        };
        BinaryPrimitives.WriteUInt64LittleEndian(bytes[(position + 5)..(position + 13)], request.Value);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[(position + 13)..(position + 17)], request.Count);
        
        bytes[position + 17] = request.AutoCommit ? (byte)1 : (byte)0;
    }
    internal static void CreateMessage(Span<byte> bytes, Identifier streamId, Identifier topicId,
        Partitioning partitioning, ICollection<Message> messages)
    {
        WriteBytesFromStreamAndTopicIdToSpan(streamId , topicId , bytes);
        int streamTopicIdOffset = 2 + streamId.Length + 2 + topicId.Length;
        bytes[streamTopicIdOffset] = partitioning.Kind switch
        {
            PartitioningKind.None => 0,
            PartitioningKind.PartitionId => 1,
            PartitioningKind.EntityId => 2,
            _ => throw new ArgumentOutOfRangeException()
        };
        bytes[streamTopicIdOffset + 1] = (byte)partitioning.Length;
        partitioning.Value.CopyTo(bytes[(streamTopicIdOffset + 2)..(streamTopicIdOffset + partitioning.Length + 2)]);

        var position = 2 + partitioning.Length + streamTopicIdOffset;
        bytes = messages switch
        {
            Message[] messagesArray => HandleMessagesArray(position, messagesArray, bytes),
            List<Message> messagesList => HandleMessagesList(position, messagesList, bytes),
            _ => HandleMessagesEnumerable(position, messages, bytes),
        };
    }
    private static Span<byte> HandleMessagesEnumerable(int position, IEnumerable<Message> messages, Span<byte> bytes)
    {
        Span<byte> id = stackalloc byte[16];
        foreach (var message in messages)
        {
            var guid = message.Id;
            MemoryMarshal.TryWrite(id, ref guid);
            for (int i = position; i < position + 16; i++)
            {
                bytes[i] = id[i - position];
            }

            BinaryPrimitives.WriteInt32LittleEndian(bytes[(position + 16)..(position + 20)], message.Payload.Length);
            var payloadBytes = message.Payload.AsSpan();
            var slice = bytes[(position + 16 + 4)..];
            payloadBytes.CopyTo(slice);
            position += payloadBytes.Length + 16 + 4;
        }

        return bytes;
    }
    private static Span<byte> HandleMessagesArray(int position, Message[] messages, Span<byte> bytes)
    {
        Span<byte> id = stackalloc byte[16];
        ref var start = ref MemoryMarshal.GetArrayDataReference(messages);
        ref var end = ref Unsafe.Add(ref start, messages.Length);
        while (Unsafe.IsAddressLessThan(ref start, ref end))
        {
            var guid = start.Id;
            MemoryMarshal.TryWrite(id, ref guid);
            
            for (int j = position; j < position + 16; j++)
            {
                bytes[j] = id[j - position];
            }

            BinaryPrimitives.WriteInt32LittleEndian(bytes[(position + 16)..(position + 20)], start.Payload.Length);
            var payloadBytes = start.Payload.AsSpan();
            var slice = bytes[(position + 16 + 4)..];
            payloadBytes.CopyTo(slice);
            position += payloadBytes.Length + 16 + 4;

            start = ref Unsafe.Add(ref start, 1);
        }

        return bytes;
    }
    private static Span<byte> HandleMessagesList(int position, List<Message> messages, Span<byte> bytes)
    {
        Span<byte> id = stackalloc byte[16];
        Span<Message> listAsSpan = CollectionsMarshal.AsSpan(messages);
        ref var start = ref MemoryMarshal.GetReference(listAsSpan);
        ref var end = ref Unsafe.Add(ref start, listAsSpan.Length);
        while (Unsafe.IsAddressLessThan(ref start, ref end))
        {
            var guid = start.Id;
            MemoryMarshal.TryWrite(id, ref guid);
            
            for (int j = position; j < position + 16; j++)
            {
                bytes[j] = id[j - position];
            }

            BinaryPrimitives.WriteInt32LittleEndian(bytes[(position + 16)..(position + 20)], start.Payload.Length);
            var payloadBytes = start.Payload.AsSpan();
            var slice = bytes[(position + 16 + 4)..];
            payloadBytes.CopyTo(slice);
            position += payloadBytes.Length + 16 + 4;

            start = ref Unsafe.Add(ref start, 1);
        }

        return bytes;
    }
    internal static byte[] CreateStream(StreamRequest request)
    {
        Span<byte> bytes = stackalloc byte[4 + request.Name.Length];
        BinaryPrimitives.WriteInt32LittleEndian(bytes[..4] , request.StreamId);
        Encoding.UTF8.GetBytes(request.Name, bytes[4..]);
        return bytes.ToArray();
    }

    internal static byte[] CreateGroup(Identifier streamId, Identifier topicId, CreateConsumerGroupRequest request)
    {
        Span<byte> bytes = stackalloc byte[2 + streamId.Length + 2 + topicId.Length + sizeof(int)];
        WriteBytesFromStreamAndTopicIdToSpan(streamId, topicId, bytes);
        int position = 2 + streamId.Length + 2 + topicId.Length;
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
        WriteBytesFromStreamAndTopicIdToSpan(streamId , topicId , bytes);
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
        Span<byte> bytes = stackalloc byte[2 + streamId.Length + 8 + request.Name.Length];
        WriteBytesFromIdentifierToSpan(streamId, bytes);
        var streamIdBytesLength = 2 + streamId.Length; 
        BinaryPrimitives.WriteInt32LittleEndian(bytes[streamIdBytesLength..(streamIdBytesLength + 4)], request.TopicId);
        int position = 4 + streamIdBytesLength;
        BinaryPrimitives.WriteInt32LittleEndian(bytes[position..(position + 4)], request.PartitionsCount);
        Encoding.UTF8.GetBytes(request.Name, bytes[(position + 4)..]);
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
        WriteBytesFromStreamAndTopicIdToSpan(streamId , topicId , bytes);
        return bytes.ToArray();
    }

    internal static byte[] UpdateOffset(Identifier streamId, Identifier topicId, OffsetContract contract)
    {
        Span<byte> bytes =
            stackalloc byte[2 + streamId.Length + 2 + topicId.Length + 17];
        bytes[0] = contract.Consumer.Type switch
        {
            ConsumerType.Consumer => 1,
            ConsumerType.ConsumerGroup => 2,
            _ => throw new ArgumentOutOfRangeException()
        };
        BinaryPrimitives.WriteInt32LittleEndian(bytes[1..5], contract.Consumer.Id);
        WriteBytesFromStreamAndTopicIdToSpan(streamId , topicId, bytes, 5);
        var position = 5 + 2 + streamId.Length + 2 + topicId.Length; 
        BinaryPrimitives.WriteInt32LittleEndian(bytes[position..(position + 4)], contract.PartitionId);
        BinaryPrimitives.WriteUInt64LittleEndian(bytes[(position + 4)..(position + 12)], contract.Offset);
        return bytes.ToArray();
    }

    internal static byte[] GetOffset(OffsetRequest request)
    {
        Span<byte> bytes =
            stackalloc byte[2 + request.StreamId.Length + 2 + request.TopicId.Length + sizeof(int) * 2 + 1];
        bytes[0] = request.Consumer.Type switch
        {
            ConsumerType.Consumer => 1,
            ConsumerType.ConsumerGroup => 2,
            _ => throw new ArgumentOutOfRangeException()
        };
        BinaryPrimitives.WriteInt32LittleEndian(bytes[1..5], request.Consumer.Id);
        WriteBytesFromStreamAndTopicIdToSpan(request.StreamId , request.TopicId, bytes, 5);
        var position = 5 + 2 + request.StreamId.Length + 2 + request.TopicId.Length; 
        BinaryPrimitives.WriteInt32LittleEndian(bytes[position..(position + 4)], request.PartitionId);
        return bytes.ToArray();
    }

    internal static byte[] CreatePartitions(Identifier streamId, Identifier topicId, CreatePartitionsRequest request)
    {
        Span<byte> bytes = stackalloc byte[2 + streamId.Length + 2 + topicId.Length + sizeof(int)];
        WriteBytesFromStreamAndTopicIdToSpan(streamId, topicId, bytes);
        int position = 2 + streamId.Length + 2 + topicId.Length;
        BinaryPrimitives.WriteInt32LittleEndian(bytes[position..(position + 4)], request.PartitionsCount);
        return bytes.ToArray();
    }
    internal static byte[] DeletePartitions(Identifier streamId, Identifier topicId, DeletePartitionsRequest request)
    {
        Span<byte> bytes = stackalloc byte[2 + streamId.Length + 2 + topicId.Length + sizeof(int)];
        WriteBytesFromStreamAndTopicIdToSpan(streamId, topicId, bytes);
        int position = 2 + streamId.Length + 2 + topicId.Length;
        BinaryPrimitives.WriteInt32LittleEndian(bytes[position..(position + 4)], request.PartitionsCount);
        return bytes.ToArray();
    }
	private static void WriteBytesFromIdentifierToSpan(Identifier identifier, Span<byte> bytes)
	{
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
	}
    private static void WriteBytesFromStreamAndTopicIdToSpan(Identifier streamId, Identifier topicId, Span<byte> bytes, int startPos = 0)
    {
        bytes[startPos] = streamId.Kind switch
        {
            IdKind.Numeric => 1,
            IdKind.String => 2,
            _ => throw new ArgumentOutOfRangeException()
        };
        bytes[startPos + 1] = (byte)streamId.Length;
        for (int i = 0; i < streamId.Length; i++)
        {
            bytes[i + startPos + 2] = streamId.Value[i];
        }

        int position = startPos + 2 + streamId.Length;
        bytes[position] = topicId.Kind switch
        {
            IdKind.Numeric => 1,
            IdKind.String => 2,
            _ => throw new ArgumentOutOfRangeException()
        };
        bytes[position + 1] = (byte)topicId.Length;
        for (int i = 0; i < topicId.Length; i++)
        {
            bytes[i + position + 2] = topicId.Value[i];
        }
    }

}