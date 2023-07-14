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

    internal static byte[] CreateMessagePayload(int offset, int timestamp, int id, string payload)
    {
        int messageLength = payload.Length;
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        var totalSize = 36 + payloadBytes.Length;
        var payloadBuffer = new byte[totalSize];
        BinaryPrimitives.WriteUInt64LittleEndian(payloadBuffer, (ulong)offset);
        BinaryPrimitives.WriteUInt64LittleEndian(payloadBuffer.AsSpan(8), (ulong)timestamp);
        BinaryPrimitives.WriteUInt64LittleEndian(payloadBuffer.AsSpan(16), (ulong)id);
        BinaryPrimitives.WriteUInt32LittleEndian(payloadBuffer.AsSpan(32), (uint)messageLength);
        payloadBytes.CopyTo(payloadBuffer.AsSpan(36));
        return payloadBuffer;
    }

    internal static byte[] CreateStreamPayload(int id, int topicsCount, string name)
    {
        var nameBytes = Encoding.UTF8.GetBytes(name);
        var totalSize = 12 + nameBytes.Length;
        var payload = new byte[totalSize];
        BinaryPrimitives.WriteInt32LittleEndian(payload, id);
        BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(4), topicsCount);
        BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(8), nameBytes.Length);
        nameBytes.CopyTo(payload.AsSpan(12));
        return payload;
    }

    internal static byte[] CreateTopicPayload(int id, int partitionsCount, string name)
    {
        var nameBytes = Encoding.UTF8.GetBytes(name);
        var totalSize = 12 + nameBytes.Length;
        var payload = new byte[totalSize];
        BinaryPrimitives.WriteInt32LittleEndian(payload, id);
        BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(4), partitionsCount);
        BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(8), nameBytes.Length);
        nameBytes.CopyTo(payload.AsSpan(12));
        return payload;
    }

    internal static byte[] CreatePartitionPayload(int id, int segmentsCount, int currentOffset, int sizeBytes)
    {
        var payload = new byte[16];
        BinaryPrimitives.WriteInt32LittleEndian(payload, id);
        BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(4), segmentsCount);
        BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(8), currentOffset);
        BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(12), sizeBytes);
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