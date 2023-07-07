using System.Text;
using Iggy_SDK.Contracts;

namespace Iggy_SDK.Mappers;
public static class BinaryMapper
{
    public static OffsetResponse MapOffsets(byte[] payload)
    {
        int consumerId = BitConverter.ToInt32(payload, 0);
        int offset = BitConverter.ToInt32(payload, 4);

        return new OffsetResponse
        {
            Offset = offset,
            ConsumerId = consumerId
        };
    }
    public static IEnumerable<MessageResponse> MapMessages(byte[] payload)
    {
        const int PROPERTIES_SIZE = 36;
        int length = payload.Length;
        int position = 4;
        List<MessageResponse> messages = new();
        while (position < length)
        {
            int offset = BitConverter.ToInt32(payload, position);
            ulong timestamp = BitConverter.ToUInt64(payload, position + 8);
            byte[] idBytes = new byte[16];
            Array.Copy(payload, position + 16, idBytes, 0, 16);
            UInt128 id = BitConverter.ToUInt64(idBytes);
            uint messageLength = BitConverter.ToUInt32(payload, position + 32);

            int payloadRangeStart = position + PROPERTIES_SIZE;
            int payloadRangeEnd = payloadRangeStart + (int)messageLength;
            if (payloadRangeStart > length || payloadRangeEnd > length)
                break;

            byte[] messagePayload = new byte[messageLength];
            Array.Copy(payload, payloadRangeStart, messagePayload, 0, messageLength);
            int totalSize = PROPERTIES_SIZE + (int)messageLength;
            
            position += totalSize;

            messages.Add(new MessageResponse
            {
                Offset = offset,
                Timestamp = timestamp,
                Id = id,
                Payload = Encoding.ASCII.GetString(messagePayload)
            });

            if (position + PROPERTIES_SIZE >= length)
                break;
        }

        return messages;
    }

    public static IEnumerable<StreamsResponse> MapStreams(byte[] payload)
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

    public static StreamResponse MapStream(byte[] payload)
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
    
    private static (StreamResponse stream, int readBytes) MapToStream(byte[] payload, int position)
    {
        int id = BitConverter.ToInt16(payload, position);
        int topicsCount = BitConverter.ToInt16(payload, position + 4);
        int nameLength = BitConverter.ToInt16(payload, position + 8);
        
        string name = Encoding.UTF8.GetString(payload, position + 12, (int)nameLength);
        int readBytes = 4 + 4 + 4 + nameLength;
        
        return (new StreamResponse { Id = id, TopicsCount = topicsCount, Name = name }, readBytes);
    }

    private static Tuple<StreamsResponse, int> MapToStreams(byte[] payload, int position)
    {
        int id = BitConverter.ToInt32(payload, position);
        int topicsCount = BitConverter.ToInt32(payload, position + 4);
        int nameLength = BitConverter.ToInt32(payload, position + 8);
        string name = Encoding.UTF8.GetString(payload, position + 12, nameLength);
        int readBytes = 4 + 4 + 4 + nameLength;

        var stream = new StreamsResponse
        {
            Id = id,
            TopicsCount = topicsCount,
            Name = name
        };

        return Tuple.Create(stream, readBytes);
    } 

    public static IEnumerable<TopicsResponse> MapTopics(byte[] payload)
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

    public static TopicsResponse MapTopic(byte[] payload)
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

    private static (TopicsResponse topic, int readBytes) MapToTopic(byte[] payload, int position)
    {
        int id = BitConverter.ToInt32(payload, position);
        int partitionsCount = BitConverter.ToInt16(payload, position + 4);
        int nameLength = BitConverter.ToInt32(payload, position + 8);
        string name = Encoding.UTF8.GetString(payload, position + 12, nameLength);
        int readBytes = 4 + 4 + 4 + nameLength;
        
        return (new TopicsResponse { Id = id, PartitionsCount = partitionsCount, Name = name }, readBytes);
    }

    private static (PartitionContract partition, int readBytes) MapToPartition(byte[] payload, int position)
    {
        int id = BitConverter.ToInt32(payload, position);
        int segmentsCount = BitConverter.ToInt32(payload, position + 4);
        int currentOffset = BitConverter.ToInt32(payload, position + 8);
        int sizeBytes = BitConverter.ToInt32(payload, position + 16);
        int readBytes = 4 + 4 + 8 + 8;
        return (new PartitionContract { Id = id, SegmentsCount = segmentsCount, CurrentOffset = currentOffset, SizeBytes = sizeBytes }, readBytes);
    }

   public static List<GroupResponse> MapConsumerGroups(byte[] payload)
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

    public static GroupResponse MapConsumerGroup(byte[] payload)
    {
        (GroupResponse consumerGroup, int position) = MapToConsumerGroup(payload, 0);
        return consumerGroup;
    }
    private static (GroupResponse consumerGroup, int readBytes) MapToConsumerGroup(byte[] payload, int position)
    {
        int id = BitConverter.ToInt32(payload, position);
        int membersCount = BitConverter.ToInt32(payload, position + 4);
        return (new GroupResponse { Id = id, MembersCount = membersCount }, 8);
    }
}
