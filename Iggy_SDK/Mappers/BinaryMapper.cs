using System.Buffers.Binary;
using System.Text;
using Iggy_SDK.Contracts.Http;

namespace Iggy_SDK.Mappers;
internal static class BinaryMapper
{
    internal static OffsetResponse MapOffsets(ReadOnlySpan<byte> payload)
    {
        int consumerId = BinaryPrimitives.ReadInt32LittleEndian(payload[0..4]);
        int offset = BinaryPrimitives.ReadInt32LittleEndian(payload[4..8]);

        return new OffsetResponse
        {
            Offset = offset,
            ConsumerId = consumerId
        };
    }
    
    internal static IEnumerable<MessageResponse> MapMessages(ReadOnlySpan<byte> payload)
    {
        const int propertiesSize = 36;
        int length = payload.Length;
        int position = 4;
        List<MessageResponse> messages = new();

        while (position < length)
        {
            ulong offset = BinaryPrimitives.ReadUInt64LittleEndian(payload[position..(position + 8)]);
            ulong timestamp = BinaryPrimitives.ReadUInt64LittleEndian(payload[(position + 8)..(position + 16)]);
            var id = new Guid(payload[(position + 16)..(position + 32)]);
            uint messageLength = BinaryPrimitives.ReadUInt32LittleEndian(payload[(position + 32)..(position + 36)]);

            int payloadRangeStart = position + propertiesSize;
            int payloadRangeEnd = position + propertiesSize + (int)messageLength;
            if (payloadRangeStart > length || payloadRangeEnd > length)
            {
                break;
            }

            var payloadSlice = payload[payloadRangeStart..payloadRangeEnd];

            int totalSize = propertiesSize + (int)messageLength;
            position += totalSize;

            messages.Add(new MessageResponse
            {
                Offset = offset,
                Timestamp = timestamp,
                Id = id,
                Payload = payloadSlice.ToArray()
            });

            if (position + propertiesSize >= length)
            {
                break;
            }
        }

        return messages;
    }

    internal static IEnumerable<StreamResponse> MapStreams(ReadOnlySpan<byte> payload)
    {
        List<StreamResponse> streams = new();
        int length = payload.Length;
        int position = 0;
        
        while (position < length)
        {
            (StreamResponse stream, int readBytes) = MapToStreams(payload, position);
            streams.Add(stream);
            position += readBytes;
        }
        
        return streams;
    }

    internal static StreamResponse MapStream(ReadOnlySpan<byte> payload)
    {
        (StreamResponse stream, int position) = MapToStream(payload, 0);
        List<TopicResponse> topics = new();
        int length = payload.Length;
        
        while (position < length)
        {
            (TopicResponse topic, int readBytes) = MapToTopic(payload, position);
            topics.Add(topic);
            position += readBytes;
        }

        return new StreamResponse
        {
            Id = stream.Id,
            TopicsCount = stream.TopicsCount,
            Name = stream.Name,
            Topics = topics,
            MessagesCount = stream.MessagesCount,
            SizeBytes = stream.SizeBytes
        };
    }
    
    private static (StreamResponse stream, int readBytes) MapToStream(ReadOnlySpan<byte> payload, int position)
    {
        int id = BinaryPrimitives.ReadInt32LittleEndian(payload[position..(position + 4)]);
        int topicsCount = BinaryPrimitives.ReadInt32LittleEndian(payload[(position + 4)..(position + 8)]);
        ulong sizeBytes = BinaryPrimitives.ReadUInt64LittleEndian(payload[(position + 8)..(position + 16)]);
        ulong messagesCount = BinaryPrimitives.ReadUInt64LittleEndian(payload[(position + 16)..(position + 24)]);
        int nameLength = BinaryPrimitives.ReadInt32LittleEndian(payload[(position + 24)..(position + 28)]);

        string name = Encoding.UTF8.GetString(payload[(position + 28)..(position + 28 + nameLength)]);
        int readBytes = 4 + 4 + 8 + 8 + 4 + nameLength;

        return (
            new StreamResponse
            {
                Id = id, TopicsCount = topicsCount, Name = name, SizeBytes = sizeBytes, MessagesCount = messagesCount
            }, readBytes);
    }

    private static (StreamResponse stream, int readBytes) MapToStreams(ReadOnlySpan<byte> payload, int position)
    {
        int id = BinaryPrimitives.ReadInt32LittleEndian(payload[position..(position + 4)]);
        int topicsCount = BinaryPrimitives.ReadInt32LittleEndian(payload[(position + 4)..(position + 8)]);
        ulong sizeBytes = BinaryPrimitives.ReadUInt64LittleEndian(payload[(position + 8)..(position + 16)]); 
        ulong messagesCount = BinaryPrimitives.ReadUInt64LittleEndian(payload[(position + 16)..(position + 24)]);
        int nameLength = BinaryPrimitives.ReadInt32LittleEndian(payload[(position + 24)..(position + 28)]);
        string name = Encoding.UTF8.GetString(payload[(position + 28)..(position + 28 + nameLength)]);
        int readBytes = 4 + 4 + 8 + 8 + 4 + nameLength;

        var stream = new StreamResponse
        {
            Id = id,
            TopicsCount = topicsCount,
            Name = name,
            MessagesCount = messagesCount,
            SizeBytes = sizeBytes
        };

        return (stream, readBytes);
    }

    internal static IEnumerable<TopicResponse> MapTopics(ReadOnlySpan<byte> payload)
    {
        List<TopicResponse> topics = new();
        int length = payload.Length;
        int position = 0;
        
        while (position < length)
        {
            (TopicResponse topic, int readBytes) = MapToTopic(payload, position);
            topics.Add(topic);
            position += readBytes;
        }
        
        return topics;
    }

    internal static TopicResponse MapTopic(ReadOnlySpan<byte> payload)
    {
        (TopicResponse topic, int position) = MapToTopic(payload, 0);
        List<PartitionContract> partitions = new();
        int length = payload.Length;
        
        while (position < length)
        {
            (PartitionContract partition, int readBytes) = MapToPartition(payload, position);
            partitions.Add(partition);
            position += readBytes;
        }

        return new TopicResponse
        {
            Id = topic.Id,
            Name = topic.Name,
            PartitionsCount = topic.PartitionsCount,
            Partitions = partitions,
            MessagesCount = topic.MessagesCount,
            SizeBytes = topic.SizeBytes
        };
    }

    private static (TopicResponse topic, int readBytes) MapToTopic(ReadOnlySpan<byte> payload, int position)
    {
        int id = BinaryPrimitives.ReadInt32LittleEndian(payload[position..(position + 4)]);
        int partitionsCount = BinaryPrimitives.ReadInt32LittleEndian(payload[(position + 4)..(position + 8)]);
        ulong sizeBytes = BinaryPrimitives.ReadUInt64LittleEndian(payload[(position + 8)..(position + 16)]); 
        ulong messagesCount = BinaryPrimitives.ReadUInt64LittleEndian(payload[(position + 16)..(position + 24)]);
        int nameLength = BinaryPrimitives.ReadInt32LittleEndian(payload[(position + 24)..(position + 28)]);
        string name = Encoding.UTF8.GetString(payload[(position + 28)..(position + 28 + nameLength)]);
        int readBytes = 4 + 4 + 8 + 8 + 4 + nameLength;
        
        return (
            new TopicResponse
            {
                Id = id, PartitionsCount = partitionsCount, Name = name, MessagesCount = messagesCount,
                SizeBytes = sizeBytes
            }, readBytes);
    }

    private static (PartitionContract partition, int readBytes) MapToPartition(ReadOnlySpan<byte>
        payload, int position)
    {
        int id = BinaryPrimitives.ReadInt32LittleEndian(payload[position..(position + 4)]);
        int segmentsCount = BinaryPrimitives.ReadInt32LittleEndian(payload[(position + 4)..(position + 8)]);
        ulong currentOffset = BinaryPrimitives.ReadUInt64LittleEndian(payload[(position + 8)..(position + 16)]);
        ulong sizeBytes = BinaryPrimitives.ReadUInt64LittleEndian(payload[(position + 16)..(position + 24)]);
        ulong messagesCount = BinaryPrimitives.ReadUInt64LittleEndian(payload[(position + 24)..(position + 32)]); 
        int readBytes = 4 + 4 + 8 + 8 + 8;
        
        return (
            new PartitionContract
            {
                Id = id, SegmentsCount = segmentsCount, CurrentOffset = currentOffset, SizeBytes = sizeBytes,
                MessagesCount = messagesCount
            }, readBytes);
    }

   internal static List<ConsumerGroupResponse> MapConsumerGroups(ReadOnlySpan<byte> payload)
    {
        List<ConsumerGroupResponse> consumerGroups = new();
        int length = payload.Length;
        int position = 0;
        while (position < length)
        {
            (ConsumerGroupResponse consumerGroup, int readBytes) = MapToConsumerGroup(payload, position);
            consumerGroups.Add(consumerGroup);
            position += readBytes;
        }
        
        return consumerGroups;
    }

    internal static ConsumerGroupResponse MapConsumerGroup(ReadOnlySpan<byte> payload)
    {
        (ConsumerGroupResponse consumerGroup, int position) = MapToConsumerGroup(payload, 0);
        
        return consumerGroup;
    }
    private static (ConsumerGroupResponse consumerGroup, int readBytes) MapToConsumerGroup(ReadOnlySpan<byte> payload,
        int position)
    {
        int id = BinaryPrimitives.ReadInt32LittleEndian(payload[position..(position + 4)]);
        int membersCount = BinaryPrimitives.ReadInt32LittleEndian(payload[(position + 4)..(position + 8)]);
        int partitionsCount = BinaryPrimitives.ReadInt32LittleEndian(payload[(position + 8)..(position + 12)]);
        
        return (new ConsumerGroupResponse { Id = id, MembersCount = membersCount, PartitionsCount = partitionsCount}, 12);
    }
}
