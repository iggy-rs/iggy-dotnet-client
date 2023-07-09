using System.Buffers.Binary;
using System.Text;
using Iggy_SDK.Contracts;

namespace Iggy_SDK.Mappers;
public static class BinaryMapper
{
    public static OffsetResponse MapOffsets(ReadOnlySpan<byte> payload)
    {
        int consumerId = BinaryPrimitives.ReadInt32LittleEndian(payload[0..4]);
        int offset = BinaryPrimitives.ReadInt32LittleEndian(payload[4..8]);

        return new OffsetResponse
        {
            Offset = offset,
            ConsumerId = consumerId
        };
    }
    
    public static IEnumerable<MessageResponse> MapMessages(ReadOnlySpan<byte> payload)
    {
        const int propertiesSize = 36;
        int length = payload.Length;
        int position = 4;
        List<MessageResponse> messages = new();

        while (position < length)
        {
            ulong offset = BinaryPrimitives.ReadUInt64LittleEndian(payload[position..(position + 8)]);
            ulong timestamp = BinaryPrimitives.ReadUInt64LittleEndian(payload[(position + 8)..(position + 16)]);
            ulong id = BinaryPrimitives.ReadUInt64LittleEndian(payload[(position + 16)..(position + 24)]);
            uint messageLength = BinaryPrimitives.ReadUInt32LittleEndian(payload[(position + 32)..(position + 36)]);

            int payloadRangeStart = position + propertiesSize;
            int payloadRangeEnd = position + propertiesSize + (int)messageLength;
            if (payloadRangeStart > length || payloadRangeEnd > length)
            {
                break;
            }

            var payloadSlice = payload[payloadRangeStart..payloadRangeEnd];
            var payloadStringify = Encoding.UTF8.GetString(payloadSlice);

            int totalSize = propertiesSize + (int)messageLength;
            position += totalSize;

            messages.Add(new MessageResponse
            {
                Offset = (int)offset,
                Timestamp = timestamp,
                Id = id,
                Payload = payloadStringify
            });

            if (position + propertiesSize >= length)
            {
                break;
            }
        }

        return messages;
    }

    public static IEnumerable<StreamsResponse> MapStreams(ReadOnlySpan<byte> payload)
    {
        List<StreamsResponse> streams = new();
        int length = payload.Length;
        int position = 0;
        
        while (position < length)
        {
            (StreamsResponse stream, int readBytes) = MapToStreams(payload, position);
            streams.Add(stream);
            position += readBytes;
        }
        
        return streams;
    }

    public static StreamResponse MapStream(ReadOnlySpan<byte> payload)
    {
        (StreamResponse stream, int position) = MapToStream(payload, 0);
        List<TopicsResponse> topics = new();
        int length = payload.Length;
        
        while (position < length)
        {
            (TopicsResponse topic, int readBytes) = MapToTopic(payload, position);
            topics.Add(topic);
            position += readBytes;
        }

        return new StreamResponse
        {
            Id = stream.Id,
            TopicsCount = stream.TopicsCount,
            Name = stream.Name,
            Topics = topics
        };
    }
    
    private static (StreamResponse stream, int readBytes) MapToStream(ReadOnlySpan<byte> payload, int position)
    {
        int id = BinaryPrimitives.ReadInt32LittleEndian(payload[position..(position + 4)]);
        int topicsCount = BinaryPrimitives.ReadInt32LittleEndian(payload[(position + 4)..(position + 8)]);
        int nameLength = BinaryPrimitives.ReadInt32LittleEndian(payload[(position + 8)..(position + 12)]);

        string name = Encoding.UTF8.GetString(payload[(position + 12)..(position + 12 + nameLength)]);
        int readBytes = 4 + 4 + 4 + nameLength;

        return (new StreamResponse { Id = id, TopicsCount = topicsCount, Name = name }, readBytes);
    }

    private static (StreamsResponse stream, int readBytes) MapToStreams(ReadOnlySpan<byte> payload, int position)
    {
        int id = BinaryPrimitives.ReadInt32LittleEndian(payload[position..(position + 4)]);
        int topicsCount = BinaryPrimitives.ReadInt32LittleEndian(payload[(position + 4)..(position + 8)]);
        int nameLength = BinaryPrimitives.ReadInt32LittleEndian(payload[(position + 8)..(position + 12)]);
        string name = Encoding.UTF8.GetString(payload[(position + 12)..(position + 12 + nameLength)]);
        int readBytes = 4 + 4 + 4 + nameLength;

        var stream = new StreamsResponse
        {
            Id = id,
            TopicsCount = topicsCount,
            Name = name
        };

        return (stream, readBytes);
    }

    public static IEnumerable<TopicsResponse> MapTopics(ReadOnlySpan<byte> payload)
    {
        List<TopicsResponse> topics = new();
        int length = payload.Length;
        int position = 0;
        
        while (position < length)
        {
            (TopicsResponse topic, int readBytes) = MapToTopic(payload, position);
            topics.Add(topic);
            position += readBytes;
        }
        
        return topics;
    }

    public static TopicsResponse MapTopic(ReadOnlySpan<byte> payload)
    {
        (TopicsResponse topic, int position) = MapToTopic(payload, 0);
        List<PartitionContract> partitions = new();
        int length = payload.Length;
        
        while (position < length)
        {
            (PartitionContract partition, int readBytes) = MapToPartition(payload, position);
            partitions.Add(partition);
            position += readBytes;
        }

        return new TopicsResponse
        {
            Id = topic.Id,
            Name = topic.Name,
            PartitionsCount = partitions.Count,
            Partitions = partitions
        };
    }

    private static (TopicsResponse topic, int readBytes) MapToTopic(ReadOnlySpan<byte> payload, int position)
    {
        int id = BinaryPrimitives.ReadInt32LittleEndian(payload[position..(position + 4)]);
        int partitionsCount = BinaryPrimitives.ReadInt32LittleEndian(payload[(position + 4)..(position + 8)]);
        int nameLength = BinaryPrimitives.ReadInt32LittleEndian(payload[(position + 8)..(position + 12)]);
        string name = Encoding.UTF8.GetString(payload[(position + 12)..(position + 12 + nameLength)]);
        int readBytes = 4 + 4 + 4 + nameLength;
        
        return (new TopicsResponse { Id = id, PartitionsCount = partitionsCount, Name = name }, readBytes);
    }

    private static (PartitionContract partition, int readBytes) MapToPartition(ReadOnlySpan<byte>
        payload, int position)
    {
        int id = BinaryPrimitives.ReadInt32LittleEndian(payload[position..(position + 4)]);
        int segmentsCount = BinaryPrimitives.ReadInt32LittleEndian(payload[(position + 4)..(position + 8)]);
        int currentOffset = BinaryPrimitives.ReadInt32LittleEndian(payload[(position + 8)..(position + 12)]);
        int sizeBytes = BinaryPrimitives.ReadInt32LittleEndian(payload[(position + 12)..(position + 16)]);
        int readBytes = 4 + 4 + 8 + 8;
        
        return (new PartitionContract { Id = id, SegmentsCount = segmentsCount, CurrentOffset = currentOffset, SizeBytes = sizeBytes }, readBytes);
    }

   public static List<GroupResponse> MapConsumerGroups(ReadOnlySpan<byte> payload)
    {
        List<GroupResponse> consumerGroups = new();
        int length = payload.Length;
        int position = 0;
        while (position < length)
        {
            (GroupResponse consumerGroup, int readBytes) = MapToConsumerGroup(payload, position);
            consumerGroups.Add(consumerGroup);
            position += readBytes;
        }
        
        return consumerGroups;
    }

    public static GroupResponse MapConsumerGroup(ReadOnlySpan<byte> payload)
    {
        (GroupResponse consumerGroup, int position) = MapToConsumerGroup(payload, 0);
        
        return consumerGroup;
    }
    private static (GroupResponse consumerGroup, int readBytes) MapToConsumerGroup(ReadOnlySpan<byte> payload,
        int position)
    {
        int id = BinaryPrimitives.ReadInt32LittleEndian(payload[position..(position + 4)]);
        int membersCount = BinaryPrimitives.ReadInt32LittleEndian(payload[(position + 4)..(position + 8)]);
        int partitionsCount = BinaryPrimitives.ReadInt32LittleEndian(payload[(position + 8)..(position + 12)]);
        
        return (new GroupResponse { Id = id, MembersCount = membersCount, PartitionsCount = partitionsCount}, 12);
    }
}
