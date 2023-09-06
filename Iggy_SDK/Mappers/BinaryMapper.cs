using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;
using Iggy_SDK.Extensions;
using Iggy_SDK.Headers;
using System.Buffers;
using System.Buffers.Binary;
using System.Text;

namespace Iggy_SDK.Mappers;
internal static class BinaryMapper
{
    private const int PROPERTIES_SIZE = 45;
    internal static OffsetResponse MapOffsets(ReadOnlySpan<byte> payload)
    {
        var partitionId = BinaryPrimitives.ReadInt32LittleEndian(payload[0..4]);
        var currentOffset = BinaryPrimitives.ReadUInt64LittleEndian(payload[4..12]);
        var offset = BinaryPrimitives.ReadUInt64LittleEndian(payload[12..20]);

        return new OffsetResponse
        {
            CurrentOffset = currentOffset,
            StoredOffset = offset,
            PartitionId = partitionId
        };
    }
    private static MessageState MapMessageState(ReadOnlySpan<byte> payload, int position)
    {
        var state = payload[position + 8] switch
        {
            1 => MessageState.Available,
            10 => MessageState.Unavailable,
            20 => MessageState.Poisoned,
            30 => MessageState.MarkedForDeletion,
            _ => throw new ArgumentOutOfRangeException()
        };
        return state;
    }
    internal static PolledMessages MapMessages(ReadOnlySpan<byte> payload,
        Func<byte[], byte[]>? decryptor = null)
    {
        int length = payload.Length;
        var partitionId = BinaryPrimitives.ReadInt32LittleEndian(payload[0..4]);
        var currentOffset = BinaryPrimitives.ReadUInt64LittleEndian(payload[4..12]);
        var messagesCount = BinaryPrimitives.ReadUInt32LittleEndian(payload[12..16]);
        int position = 16;
        if (position >= length)
        {
            return PolledMessages.Empty;
        }
        List<MessageResponse> messages = new();

        while (position < length)
        {
            ulong offset = BinaryPrimitives.ReadUInt64LittleEndian(payload[position..(position + 8)]);
            var state = MapMessageState(payload, position);
            ulong timestamp = BinaryPrimitives.ReadUInt64LittleEndian(payload[(position + 9)..(position + 17)]);
            var id = new Guid(payload[(position + 17)..(position + 33)]);
            var checksum = BinaryPrimitives.ReadUInt32LittleEndian(payload[(position + 33)..(position + 37)]);
            int headersLength = BinaryPrimitives.ReadInt32LittleEndian(payload[(position + 37)..(position + 41)]);

            var headers = headersLength switch
            {
                0 => null,
                > 0 => MapHeaders(payload[(position + 41)..(position + 41 + headersLength)]),
                < 0 => throw new ArgumentOutOfRangeException()
            };
            position += headersLength;
            uint messageLength = BinaryPrimitives.ReadUInt32LittleEndian(payload[(position + 41)..(position + 45)]);

            int payloadRangeStart = position + PROPERTIES_SIZE;
            int payloadRangeEnd = position + PROPERTIES_SIZE + (int)messageLength;
            if (payloadRangeStart > length || payloadRangeEnd > length)
            {
                break;
            }

            var payloadSlice = payload[payloadRangeStart..payloadRangeEnd];
            var messagePayload = ArrayPool<byte>.Shared.Rent(payloadSlice.Length);
            var payloadSliceLen = payloadSlice.Length;

            try
            {
                payloadSlice.CopyTo(messagePayload.AsSpan()[..payloadSliceLen]);

                int totalSize = PROPERTIES_SIZE + (int)messageLength;
                position += totalSize;

                messages.Add(new MessageResponse
                {
                    Offset = offset,
                    Timestamp = timestamp,
                    Id = id,
                    Checksum = checksum,
                    State = state,
                    Headers = headers,
                    Payload = decryptor is not null
                        ? decryptor(messagePayload[..payloadSliceLen])
                        : messagePayload[..payloadSliceLen]
                });
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(messagePayload);
            }

            if (position + PROPERTIES_SIZE >= length)
            {
                break;
            }
        }

        return new PolledMessages
        {
            PartitionId = partitionId,
            CurrentOffset = currentOffset,
            Messages = messages.AsReadOnly()
        };
    }
    internal static PolledMessages<TMessage> MapMessages<TMessage>(ReadOnlySpan<byte> payload,
        Func<byte[], TMessage> serializer, Func<byte[], byte[]>? decryptor = null)
    {
        int length = payload.Length;
        var partitionId = BinaryPrimitives.ReadInt32LittleEndian(payload[0..4]);
        var currentOffset = BinaryPrimitives.ReadUInt64LittleEndian(payload[4..12]);
        var messagesCount = BinaryPrimitives.ReadUInt32LittleEndian(payload[12..16]);
        int position = 16;
        if (position >= length)
        {
            return PolledMessages<TMessage>.Empty;
        }

        List<MessageResponse<TMessage>> messages = new();
        while (position < length)
        {
            ulong offset = BinaryPrimitives.ReadUInt64LittleEndian(payload[position..(position + 8)]);
            var state = MapMessageState(payload, position);
            ulong timestamp = BinaryPrimitives.ReadUInt64LittleEndian(payload[(position + 9)..(position + 17)]);
            var id = new Guid(payload[(position + 17)..(position + 33)]);
            var checksum = BinaryPrimitives.ReadUInt32LittleEndian(payload[(position + 33)..(position + 37)]);
            int headersLength = BinaryPrimitives.ReadInt32LittleEndian(payload[(position + 37)..(position + 41)]);

            var headers = headersLength switch
            {
                0 => null,
                > 0 => MapHeaders(payload[(position + 41)..(position + 41 + headersLength)]),
                < 0 => throw new ArgumentOutOfRangeException()
            };
            position += headersLength;
            uint messageLength = BinaryPrimitives.ReadUInt32LittleEndian(payload[(position + 41)..(position + 45)]);

            int payloadRangeStart = position + PROPERTIES_SIZE;
            int payloadRangeEnd = position + PROPERTIES_SIZE + (int)messageLength;
            if (payloadRangeStart > length || payloadRangeEnd > length)
            {
                break;
            }

            var payloadSlice = payload[payloadRangeStart..payloadRangeEnd];
            var messagePayload = ArrayPool<byte>.Shared.Rent(payloadSlice.Length);
            var payloadSliceLen = payloadSlice.Length;
            try
            {
                payloadSlice.CopyTo(messagePayload.AsSpan()[..payloadSliceLen]);

                int totalSize = PROPERTIES_SIZE + (int)messageLength;
                position += totalSize;

                messages.Add(new MessageResponse<TMessage>
                {
                    Offset = offset,
                    Timestamp = timestamp,
                    Checksum = checksum,
                    Id = id,
                    Headers = headers,
                    State = state,
                    Message = decryptor is not null
                        ? serializer(decryptor(messagePayload[..payloadSliceLen]))
                        : serializer(messagePayload[..payloadSliceLen])
                });
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(messagePayload);
            }

            if (position + PROPERTIES_SIZE >= length)
            {
                break;
            }
        }

        return new PolledMessages<TMessage>
        {
            PartitionId = partitionId,
            CurrentOffset = currentOffset,
            Messages = messages.AsReadOnly()
        };
    }
    private static Dictionary<HeaderKey, HeaderValue> MapHeaders(ReadOnlySpan<byte> payload)
    {
        var headers = new Dictionary<HeaderKey, HeaderValue>();
        int position = 0;

        while (position < payload.Length)
        {
            var keyLength = BinaryPrimitives.ReadInt32LittleEndian(payload[position..(position + 4)]);
            if (keyLength is 0 or > 255)
            {
                throw new ArgumentException("Key has incorrect size, must be between 1 and 255", nameof(keyLength));
            }
            var key = Encoding.UTF8.GetString(payload[(position + 4)..(position + 4 + keyLength)]);
            position += 4 + keyLength;

            var headerKind = MapHeaderKind(payload, position);
            position++;
            var valueLength = BinaryPrimitives.ReadInt32LittleEndian(payload[position..(position + 4)]);
            if (valueLength is 0 or > 255)
            {
                throw new ArgumentException("Value has incorrect size, must be between 1 and 255", nameof(valueLength));
            }
            position += 4;
            var value = payload[position..(position + valueLength)];
            position += valueLength;
            headers.Add(HeaderKey.New(key), new HeaderValue
            {
                Kind = headerKind,
                Value = value.ToArray()
            }
            );
        }

        return headers;
    }

    private static HeaderKind MapHeaderKind(ReadOnlySpan<byte> payload, int position)
    {
        var headerKind = payload[position] switch
        {
            1 => HeaderKind.Raw,
            2 => HeaderKind.String,
            3 => HeaderKind.Bool,
            6 => HeaderKind.Int32,
            7 => HeaderKind.Int64,
            8 => HeaderKind.Int128,
            11 => HeaderKind.Uint32,
            12 => HeaderKind.Uint64,
            13 => HeaderKind.Uint128,
            14 => HeaderKind.Float32,
            15 => HeaderKind.Float64,
            _ => throw new ArgumentOutOfRangeException()
        };
        return headerKind;
    }

    internal static IReadOnlyList<StreamResponse> MapStreams(ReadOnlySpan<byte> payload)
    {
        List<StreamResponse> streams = new();
        int length = payload.Length;
        int position = 0;

        while (position < length)
        {
            (StreamResponse stream, int readBytes) = MapToStream(payload, position);
            streams.Add(stream);
            position += readBytes;
        }

        return streams.AsReadOnly();
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
            CreatedAt = stream.CreatedAt,
            MessagesCount = stream.MessagesCount,
            SizeBytes = stream.SizeBytes
        };
    }

    private static (StreamResponse stream, int readBytes) MapToStream(ReadOnlySpan<byte> payload, int position)
    {
        int id = BinaryPrimitives.ReadInt32LittleEndian(payload[position..(position + 4)]);
        ulong createdAt = BinaryPrimitives.ReadUInt64LittleEndian(payload[(position + 4)..(position + 12)]);
        int topicsCount = BinaryPrimitives.ReadInt32LittleEndian(payload[(position + 12)..(position + 16)]);
        ulong sizeBytes = BinaryPrimitives.ReadUInt64LittleEndian(payload[(position + 16)..(position + 24)]);
        ulong messagesCount = BinaryPrimitives.ReadUInt64LittleEndian(payload[(position + 24)..(position + 32)]);
        int nameLength = (int)payload[position + 32];

        string name = Encoding.UTF8.GetString(payload[(position + 33)..(position + 33 + nameLength)]);
        int readBytes = 4 + 4 + 8 + 8 + 8 + 1 + nameLength;

        return (
            new StreamResponse
            {
                Id = id,
                TopicsCount = topicsCount,
                Name = name,
                SizeBytes = sizeBytes,
                MessagesCount = messagesCount,
                CreatedAt = DateTimeOffsetUtils.FromUnixTimeMicroSeconds(createdAt)
            }, readBytes);
    }
    internal static IReadOnlyList<TopicResponse> MapTopics(ReadOnlySpan<byte> payload)
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

        return topics.AsReadOnly();
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
            CreatedAt = topic.CreatedAt,
            MessageExpiry = topic.MessageExpiry,
            MessagesCount = topic.MessagesCount,
            SizeBytes = topic.SizeBytes
        };
    }

    private static (TopicResponse topic, int readBytes) MapToTopic(ReadOnlySpan<byte> payload, int position)
    {
        int id = BinaryPrimitives.ReadInt32LittleEndian(payload[position..(position + 4)]);
        ulong createdAt = BinaryPrimitives.ReadUInt64LittleEndian(payload[(position + 4)..(position + 12)]);
        int partitionsCount = BinaryPrimitives.ReadInt32LittleEndian(payload[(position + 12)..(position + 16)]);
        int messageExpiry = BinaryPrimitives.ReadInt32LittleEndian(payload[(position + 16)..(position + 20)]);
        ulong sizeBytes = BinaryPrimitives.ReadUInt64LittleEndian(payload[(position + 20)..(position + 28)]);
        ulong messagesCount = BinaryPrimitives.ReadUInt64LittleEndian(payload[(position + 28)..(position + 36)]);
        int nameLength = (int)payload[position + 36];
        string name = Encoding.UTF8.GetString(payload[(position + 37)..(position + 37 + nameLength)]);
        int readBytes = 4 + 4 + 4 + 8 + 8 + 1 + 8 + nameLength;

        return (
            new TopicResponse
            {
                Id = id,
                PartitionsCount = partitionsCount,
                Name = name,
                MessagesCount = messagesCount,
                SizeBytes = sizeBytes,
                CreatedAt = DateTimeOffsetUtils.FromUnixTimeMicroSeconds(createdAt),
                MessageExpiry = messageExpiry
            }, readBytes);
    }

    private static (PartitionContract partition, int readBytes) MapToPartition(ReadOnlySpan<byte>
        payload, int position)
    {
        int id = BinaryPrimitives.ReadInt32LittleEndian(payload[position..(position + 4)]);
        ulong createdAt = BinaryPrimitives.ReadUInt64LittleEndian(payload[(position + 4)..(position + 12)]);
        int segmentsCount = BinaryPrimitives.ReadInt32LittleEndian(payload[(position + 12)..(position + 16)]);
        ulong currentOffset = BinaryPrimitives.ReadUInt64LittleEndian(payload[(position + 16)..(position + 24)]);
        ulong sizeBytes = BinaryPrimitives.ReadUInt64LittleEndian(payload[(position + 24)..(position + 32)]);
        ulong messagesCount = BinaryPrimitives.ReadUInt64LittleEndian(payload[(position + 32)..(position + 40)]);
        int readBytes = 4 + 4 + 8 + 8 + 8 + 8;

        return (
            new PartitionContract
            {
                Id = id,
                SegmentsCount = segmentsCount,
                CurrentOffset = currentOffset,
                SizeBytes = sizeBytes,
                CreatedAt = DateTimeOffsetUtils.FromUnixTimeMicroSeconds(createdAt),
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
    internal static Stats MapStats(ReadOnlySpan<byte> payload)
    {
        int processId = BinaryPrimitives.ReadInt32LittleEndian(payload[0..4]);
        float cpuUsage = BitConverter.ToSingle(payload[4..8]);
        ulong memoryUsage = BinaryPrimitives.ReadUInt64LittleEndian(payload[8..16]);
        ulong totalMemory = BinaryPrimitives.ReadUInt64LittleEndian(payload[16..24]);
        ulong availableMemory = BinaryPrimitives.ReadUInt64LittleEndian(payload[24..32]);
        ulong runTime = BinaryPrimitives.ReadUInt64LittleEndian(payload[32..40]);
        ulong startTime = BinaryPrimitives.ReadUInt64LittleEndian(payload[40..48]);
        ulong readBytes = BinaryPrimitives.ReadUInt64LittleEndian(payload[48..56]);
        ulong writtenBytes = BinaryPrimitives.ReadUInt64LittleEndian(payload[56..64]);
        ulong totalSizeBytes = BinaryPrimitives.ReadUInt64LittleEndian(payload[64..72]);
        int streamsCount = BinaryPrimitives.ReadInt32LittleEndian(payload[72..76]);
        int topicsCount = BinaryPrimitives.ReadInt32LittleEndian(payload[76..80]);
        int partitionsCount = BinaryPrimitives.ReadInt32LittleEndian(payload[80..84]);
        int segmentsCount = BinaryPrimitives.ReadInt32LittleEndian(payload[84..88]);
        ulong messagesCount = BinaryPrimitives.ReadUInt64LittleEndian(payload[88..96]);
        int clientsCount = BinaryPrimitives.ReadInt32LittleEndian(payload[96..100]);
        int consumerGroupsCount = BinaryPrimitives.ReadInt32LittleEndian(payload[100..104]);
        int position = 104;

        int hostnameLength = BinaryPrimitives.ReadInt32LittleEndian(payload[position..(position + 4)]);
        string hostname = Encoding.UTF8.GetString(payload[(position + 4)..(position + 4 + hostnameLength)]);
        position += 4 + hostnameLength;
        int osNameLength = BinaryPrimitives.ReadInt32LittleEndian(payload[position..(position + 4)]);
        string osName = Encoding.UTF8.GetString(payload[(position + 4)..(position + 4 + osNameLength)]);
        position += 4 + osNameLength;
        int osVersionLength = BinaryPrimitives.ReadInt32LittleEndian(payload[position..(position + 4)]);
        string osVersion = Encoding.UTF8.GetString(payload[(position + 4)..(position + 4 + osVersionLength)]);
        position += 4 + osVersionLength;
        int kernelVersionLength = BinaryPrimitives.ReadInt32LittleEndian(payload[position..(position + 4)]);
        string kernelVersion = Encoding.UTF8.GetString(payload[(position + 4)..(position + 4 + kernelVersionLength)]);

        return new Stats
        {
            ProcessId = processId,
            Hostname = hostname,
            ClientsCount = clientsCount,
            CpuUsage = cpuUsage,
            MemoryUsage = memoryUsage,
            TotalMemory = totalMemory,
            AvailableMemory = availableMemory,
            RunTime = runTime,
            StartTime = DateTimeOffset.FromUnixTimeSeconds((long)startTime),
            ReadBytes = readBytes,
            WrittenBytes = writtenBytes,
            StreamsCount = streamsCount,
            KernelVersion = kernelVersion,
            MessagesCount = messagesCount,
            TopicsCount = topicsCount,
            PartitionsCount = partitionsCount,
            SegmentsCount = segmentsCount,
            OsName = osName,
            OsVersion = osVersion,
            ConsumerGroupsCount = consumerGroupsCount,
            MessagesSizeBytes = totalSizeBytes
        };
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
        int partitionsCount = BinaryPrimitives.ReadInt32LittleEndian(payload[(position + 4)..(position + 8)]);
        int membersCount = BinaryPrimitives.ReadInt32LittleEndian(payload[(position + 8)..(position + 12)]);

        return (new ConsumerGroupResponse { Id = id, MembersCount = membersCount, PartitionsCount = partitionsCount }, 12);
    }
}
