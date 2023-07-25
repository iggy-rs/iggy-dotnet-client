using System.Buffers.Binary;
using System.Text;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;

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
        };
        BinaryPrimitives.WriteUInt64LittleEndian(bytes[18..26], request.Value);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[26..30], request.Count);
        
        bytes[30] = request.AutoCommit ? (byte)1 : (byte)0;
        return bytes.ToArray();
        
    }
    internal static byte[] CreateMessage(int streamId, int topicId, MessageSendRequest request)
    {
        int messageBytesCount = request.Messages.Sum(message => 16 + 4 + message.Payload.Length);

        Span<byte> bytes = stackalloc byte[17 + messageBytesCount];
        BinaryPrimitives.WriteInt32LittleEndian(bytes[0..4], streamId);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[4..8], topicId);
        bytes[sizeof(int) * 2] = request.KeyKind switch
        {
            Keykind.PartitionId => 0,
            Keykind.EntityId => 1
        };
        BinaryPrimitives.WriteInt32LittleEndian(bytes[9..13], request.KeyValue);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[13..17], request.Messages.Count());

        int position = 17;
        foreach (var message in request.Messages)
        {

            Span<byte> id = message.Id.ToByteArray();
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
        
        return bytes.ToArray();
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
        bytes[0] = 0;
        BinaryPrimitives.WriteInt32LittleEndian(bytes[1..5], streamId);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[5..9], topicId);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[9..13], contract.ConsumerId);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[13..17], contract.PartitionId);
        BinaryPrimitives.WriteUInt64LittleEndian(bytes[17..25], contract.Offset);
        return bytes.ToArray();
    }

    internal static byte[] GetOffset(OffsetRequest request)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int) * 4 + 1];
        bytes[0] = 0;
        BinaryPrimitives.WriteInt32LittleEndian(bytes[1..5], request.StreamId);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[5..9], request.TopicId);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[9..13], request.ConsumerId);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[13..17], request.PartitionId);
        return bytes.ToArray();
    }

}