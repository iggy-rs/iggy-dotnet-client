using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;
using Iggy_SDK.Messages;

namespace Iggy_SDK.Contracts.Tcp;

internal static class TcpContracts
{
    internal static byte[] GetMessages(MessageFetchRequest request)
    {
        Span<byte> bytes = stackalloc byte[31];

        bytes[0] = request.ConsumerType switch
        {
            ConsumerType.Consumer => 0,
            ConsumerType.ConsumerGroup => 1,
            _ => throw new ArgumentOutOfRangeException()
        };
        BinaryPrimitives.WriteInt32LittleEndian(bytes[1..5], request.ConsumerId);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[5..9], request.StreamId);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[9..13], request.TopicId);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[13..17], request.PartitionId);
        bytes[sizeof(int) * 4 + 1] = request.PollingStrategy switch
        {
            MessagePolling.Offset => 0,
            MessagePolling.Timestamp => 1,
            MessagePolling.First => 2,
            MessagePolling.Last => 3,
            MessagePolling.Next => 4,
            _ => throw new ArgumentOutOfRangeException()
        };
        BinaryPrimitives.WriteUInt64LittleEndian(bytes[18..26], request.Value);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[26..30], request.Count);
        
        bytes[30] = request.AutoCommit ? (byte)1 : (byte)0;
        return bytes.ToArray();
        
    }
    internal static void CreateMessage(Span<byte> bytes, int streamId, int topicId, MessageSendRequest request)
    {
        var msgCountSuccess = request.Messages.TryGetNonEnumeratedCount(out var msgLength);
        if (!msgCountSuccess)
        {
            msgLength = request.Messages.Count();
        }
        
        BinaryPrimitives.WriteInt32LittleEndian(bytes[0..4], streamId);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[4..8], topicId);
        bytes[sizeof(int) * 2] = request.Key.Kind switch
        {
            KeyKind.None => 0,
            KeyKind.PartitionId => 1,
            KeyKind.EntityId => 2,
            _ => throw new ArgumentOutOfRangeException()
        };
        bytes[9] = (byte)request.Key.Length;
        request.Key.Value.CopyTo(bytes[10..(10 + request.Key.Length)]);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[(10 + request.Key.Length)..(10 + request.Key.Length + 4)], msgLength);


        var position = 10 + request.Key.Length + 4;
        var result = request.Messages switch
        {
            Message[] messagesArray => HandleMessagesArray(position, messagesArray, bytes),
            List<Message> messagesList => HandleMessagesList(position, messagesList, bytes),
            _ => HandleMessageEnumerable(position, request.Messages, bytes),
        };
        bytes = result;
    }
    private static Span<byte> HandleMessageEnumerable(int position, IEnumerable<Message> messages, Span<byte> bytes)
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
        Span<byte> bytes = stackalloc byte[sizeof(int) + request.Name.Length];
        BinaryPrimitives.WriteInt32LittleEndian(bytes[..4], request.StreamId);
        Encoding.UTF8.GetBytes(request.Name, bytes[4..]);
        return bytes.ToArray();
    }

    internal static byte[] CreateGroup(int streamId, int topicId, CreateConsumerGroupRequest request)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int) * 3];
        BinaryPrimitives.WriteInt32LittleEndian(bytes[..4], streamId);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[4..8], topicId);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[8..12], request.ConsumerGroupId);
        return bytes.ToArray();
    }

    internal static byte[] JoinGroup(JoinConsumerGroupRequest request)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int) * 3];
        BinaryPrimitives.WriteInt32LittleEndian(bytes[..4], request.StreamId);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[4..8], request.TopicId);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[8..12], request.ConsumerGroupId);
        return bytes.ToArray();
    }
    internal static byte[] LeaveGroup(LeaveConsumerGroupRequest request)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int) * 3];
        BinaryPrimitives.WriteInt32LittleEndian(bytes[..4], request.StreamId);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[4..8], request.TopicId);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[8..12], request.ConsumerGroupId);
        return bytes.ToArray();
    }
    internal static byte[] DeleteGroup(int streamId, int topicId, int groupId)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int) * 3];
        BinaryPrimitives.WriteInt32LittleEndian(bytes[..4], streamId);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[4..8], topicId);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[8..12], groupId);
        return bytes.ToArray();
    }

    internal static byte[] GetGroups(int streamId, int topicId)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int) * 2];
        BinaryPrimitives.WriteInt32LittleEndian(bytes[..4], streamId);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[4..8], topicId);
        return bytes.ToArray();
    }

    internal static byte[] GetGroup(int streamId, int topicId, int groupId)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int) * 3];
        BinaryPrimitives.WriteInt32LittleEndian(bytes[..4], streamId);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[4..8], topicId);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[8..12], groupId);
        return bytes.ToArray();
    }

    internal static byte[] CreateTopic(int streamId, TopicRequest request)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int) * 3 + request.Name.Length];
        BinaryPrimitives.WriteInt32LittleEndian(bytes[..4], streamId);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[4..8], request.TopicId);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[8..12], request.PartitionsCount);
        Encoding.UTF8.GetBytes(request.Name, bytes[12..]);
        return bytes.ToArray();
    }

    internal static byte[] GetTopicById(int streamId, int topicId)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int) * 2];
        BinaryPrimitives.WriteInt32LittleEndian(bytes[..4], streamId);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[4..8], topicId);
        return bytes.ToArray();
    }

    internal static byte[] DeleteTopic(int streamId, int topicId)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int) * 2];
        BinaryPrimitives.WriteInt32LittleEndian(bytes[..4], streamId);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[4..8], topicId);
        return bytes.ToArray();
    }

    internal static byte[] UpdateOffset(int streamId, int topicId, OffsetContract contract)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int) * 4 + sizeof(ulong) + 1];
        bytes[0] = contract.ConsumerType switch
        {
            ConsumerType.Consumer => 0,
            ConsumerType.ConsumerGroup => 1,
            _ => throw new ArgumentOutOfRangeException()
        };
        BinaryPrimitives.WriteInt32LittleEndian(bytes[1..5], contract.ConsumerId);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[5..9], streamId);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[9..13], topicId);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[13..17], contract.PartitionId);
        BinaryPrimitives.WriteUInt64LittleEndian(bytes[17..25], contract.Offset);
        return bytes.ToArray();
    }

    internal static byte[] GetOffset(OffsetRequest request)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int) * 4 + 1];
        bytes[0] = request.ConsumerType switch
        {
            ConsumerType.Consumer => 0,
            ConsumerType.ConsumerGroup => 1,
            _ => throw new ArgumentOutOfRangeException()
        };
        BinaryPrimitives.WriteInt32LittleEndian(bytes[1..5], request.ConsumerId);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[5..9], request.StreamId);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[9..13], request.TopicId);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[13..17], request.PartitionId);
        return bytes.ToArray();
    }

}