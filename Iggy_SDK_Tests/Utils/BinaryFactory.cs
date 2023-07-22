using System.Buffers.Binary;
using System.Text;

namespace Iggy_SDK_Tests.Utils;

internal sealed class BinaryFactory
{
	 internal static byte[] CreateOffsetPayload(int consumerId, int offset)
    {
        var payload = new byte[8];
        BinaryPrimitives.WriteInt32LittleEndian(payload, consumerId);
        BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(4), offset);
        return payload;
    }

    internal static byte[] CreateMessagePayload(ulong offset, ulong timestamp, Guid id, byte[] payload)
    {
        int messageLength = payload.Length;
        var totalSize = 36 + payload.Length;
        var payloadBuffer = new byte[totalSize];
        
        
        BinaryPrimitives.WriteUInt64LittleEndian(payloadBuffer, (ulong)offset);
        BinaryPrimitives.WriteUInt64LittleEndian(payloadBuffer.AsSpan(8), (ulong)timestamp);
        var idBytes = id.ToByteArray();
        for (int i = 16; i < 32; i++)
        {
            payloadBuffer[i] = idBytes[i - 16];
        }
        BinaryPrimitives.WriteUInt32LittleEndian(payloadBuffer.AsSpan(32), (uint)messageLength);
        payload.CopyTo(payloadBuffer.AsSpan(36));
        return payloadBuffer;
    }

    internal static byte[] CreateStreamPayload(int id, int topicsCount, string name, ulong sizeBytes, ulong messagesCount)
    {
        var nameBytes = Encoding.UTF8.GetBytes(name);
        var totalSize = 4 + 4 + 8 + 8 + 4 + nameBytes.Length;
        var payload = new byte[totalSize];
        BinaryPrimitives.WriteInt32LittleEndian(payload, id);
        BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(4), topicsCount);
        BinaryPrimitives.WriteUInt64LittleEndian(payload.AsSpan(8), sizeBytes);
        BinaryPrimitives.WriteUInt64LittleEndian(payload.AsSpan(16), messagesCount);
        BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(24), nameBytes.Length);
        nameBytes.CopyTo(payload.AsSpan(28));
        return payload;
    }

    internal static byte[] CreateTopicPayload(int id, int partitionsCount, string name, ulong sizeBytes, ulong messagesCount)
    {
        var nameBytes = Encoding.UTF8.GetBytes(name);
        var totalSize = 4 + 4 + 8 + 8 + 4 + nameBytes.Length;
        var payload = new byte[totalSize];
        BinaryPrimitives.WriteInt32LittleEndian(payload, id);
        BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(4), partitionsCount);
        BinaryPrimitives.WriteUInt64LittleEndian(payload.AsSpan(8), sizeBytes);
        BinaryPrimitives.WriteUInt64LittleEndian(payload.AsSpan(16), messagesCount);
        BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(24), nameBytes.Length);
        nameBytes.CopyTo(payload.AsSpan(28));
        return payload;
    }

    internal static byte[] CreatePartitionPayload(int id, int segmentsCount, int currentOffset, ulong sizeBytes, ulong messagesCount)
    {
        var payload = new byte[16];
        BinaryPrimitives.WriteInt32LittleEndian(payload, id);
        BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(4), segmentsCount);
        BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(8), currentOffset);
        BinaryPrimitives.WriteUInt64LittleEndian(payload.AsSpan(12), sizeBytes);
        BinaryPrimitives.WriteUInt64LittleEndian(payload.AsSpan(16), messagesCount);
        return payload;
    }

    internal static byte[] CreateGroupPayload(int id, int membersCount, int partitionsCount)
    {
        var payload = new byte[12];
        BinaryPrimitives.WriteInt32LittleEndian(payload, id);
        BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(4), membersCount);
        BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(8), partitionsCount);
        return payload;
    }
}