using System.ComponentModel;
using System.Text;
using Iggy_SDK.Contracts;

namespace ConsoleApp;

internal static class TcpContracts
{
    internal static byte[] CreateStream(StreamRequest request)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int) + request.Name.Length];
        BitConverter.TryWriteBytes(bytes.Slice(0, sizeof(int)), request.StreamId);
        Encoding.UTF8.GetBytes(request.Name, bytes.Slice(sizeof(int)));
        return bytes.ToArray();
    }

    internal static byte[] CreateGroup(int streamId, int topicId, GroupRequest request)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int) * 3];
        BitConverter.TryWriteBytes(bytes.Slice(0, sizeof(int)), streamId);
        BitConverter.TryWriteBytes(bytes.Slice(sizeof(int), sizeof(int)), topicId);
        BitConverter.TryWriteBytes(bytes.Slice(sizeof(int) * 2, sizeof(int)), request.GroupId);
        return bytes.ToArray();
    }

    internal static byte[] DeleteGroup(int streamId, int topicId, int groupId)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int) * 3];
        BitConverter.TryWriteBytes(bytes.Slice(0, sizeof(int)), streamId);
        BitConverter.TryWriteBytes(bytes.Slice(sizeof(int), sizeof(int)), topicId);
        BitConverter.TryWriteBytes(bytes.Slice(sizeof(int) * 2, sizeof(int)), groupId);
        return bytes.ToArray();
    }

    internal static byte[] GetGroups(int streamId, int topicId)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int) * 2];
        BitConverter.TryWriteBytes(bytes.Slice(0, sizeof(int)), streamId);
        BitConverter.TryWriteBytes(bytes.Slice(sizeof(int), sizeof(int)), topicId);
        return bytes.ToArray();
    }

    internal static byte[] GetGroup(int streamId, int topicId, int groupId)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int) * 3];
        BitConverter.TryWriteBytes(bytes.Slice(0, sizeof(int)), streamId);
        BitConverter.TryWriteBytes(bytes.Slice(sizeof(int), sizeof(int)), topicId);
        BitConverter.TryWriteBytes(bytes.Slice(sizeof(int) * 2, sizeof(int)), groupId);
        return bytes.ToArray();
    }

    internal static byte[] CreateTopic(int streamId, TopicRequest request)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int) * 3 + request.Name.Length];
        BitConverter.TryWriteBytes(bytes.Slice(0, sizeof(int)), streamId);
        BitConverter.TryWriteBytes(bytes.Slice(sizeof(int), sizeof(int)), request.TopicId);
        BitConverter.TryWriteBytes(bytes.Slice(sizeof(int) * 2, sizeof(int)), request.PartitionsCount);
        Encoding.UTF8.GetBytes(request.Name, bytes.Slice(sizeof(int) * 3));
        return bytes.ToArray();
    }

    internal static byte[] GetTopicById(int streamId, int topicId)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int) * 2];
        BitConverter.TryWriteBytes(bytes.Slice(0, sizeof(int)), streamId);
        BitConverter.TryWriteBytes(bytes.Slice(sizeof(int), sizeof(int)), topicId);
        return bytes.ToArray();
    }

    internal static byte[] DeleteTopic(int streamId, int topicId)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int) * 2];
        BitConverter.TryWriteBytes(bytes.Slice(0, sizeof(int)), streamId);
        BitConverter.TryWriteBytes(bytes.Slice(sizeof(int), sizeof(int)), topicId);
        return bytes.ToArray();
    }

    internal static byte[] UpdateOffset(int streamId, int topicId, OffsetContract contract)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int) * 4 + sizeof(ulong)];
        bytes[0] = 0;
        BitConverter.TryWriteBytes(bytes.Slice(1, sizeof(int)), streamId);
        BitConverter.TryWriteBytes(bytes.Slice(sizeof(int) + 1, sizeof(int)), topicId);
        BitConverter.TryWriteBytes(bytes.Slice(sizeof(int) * 2 + 1, sizeof(int)), contract.ConsumerId);
        BitConverter.TryWriteBytes(bytes.Slice(sizeof(int) * 3 + 1, sizeof(int)), contract.PartitionId);
        BitConverter.TryWriteBytes(bytes.Slice(sizeof(int) * 4 + 1, sizeof(ulong)), (ulong)contract.Offset);
        return bytes.ToArray();
    }

    internal static byte[] GetOffset(OffsetRequest request)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int) * 4];
        bytes[0] = 0;
        BitConverter.TryWriteBytes(bytes.Slice(1, sizeof(int)), request.StreamId);
        BitConverter.TryWriteBytes(bytes.Slice(sizeof(int) + 1, sizeof(int)), request.TopicId);
        BitConverter.TryWriteBytes(bytes.Slice(sizeof(int) * 2 + 1, sizeof(int)), request.ConsumerId);
        BitConverter.TryWriteBytes(bytes.Slice(sizeof(int) * 3 + 1, sizeof(int)), request.PartitionId);
        return bytes.ToArray();
    }

}