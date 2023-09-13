using Iggy_SDK;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;
using Iggy_SDK.Extensions;
using Iggy_SDK.Headers;
using Iggy_SDK.Kinds;
using Iggy_SDK.Messages;
using Iggy_SDK_Tests.Utils.DummyObj;
using System.Buffers.Binary;
using System.Text;
using System.Text.Json;

namespace Iggy_SDK_Tests.Utils.Messages;

internal static class MessageFactory
{
    internal static (ulong offset, ulong timestamp, Guid guid, int headersLength, uint checkSum, byte[] payload) CreateMessageResponseFields()
    {
        ulong offset = (ulong)Random.Shared.Next(6, 69);
        var timestamp = (ulong)Random.Shared.Next(420, 69420);
        var guid = Guid.NewGuid();
        var checkSum = (uint)Random.Shared.Next(42069, 69042);
        var bytes = Encoding.UTF8.GetBytes(Utility.RandomString(Random.Shared.Next(6, 69)));
        int headersLength = Random.Shared.Next(1, 69);
        return (offset, timestamp, guid, headersLength, checkSum, bytes);
    }
    internal static Func<DummyMessage, byte[]> Serializer = msg =>
    {
        Span<byte> bytes = stackalloc byte[4 + 4 + msg.Text.Length];
        BinaryPrimitives.WriteInt32LittleEndian(bytes[..4], msg.Id);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[4..8], msg.Text.Length);
        Encoding.UTF8.GetBytes(msg.Text).CopyTo(bytes[8..]);
        return bytes.ToArray();
    };
    internal static (ulong offset, ulong timestamp, Guid guid, int headersLength, uint checkSum, byte[] payload) CreateMessageResponseFieldsTMessage()
    {
        var msg = new DummyMessage
        {
            Id = Random.Shared.Next(1, 69),
            Text = "Hello"
        };
        ulong offset = (ulong)Random.Shared.Next(6, 69);
        int headersLength = Random.Shared.Next(1, 69);
        var timestamp = (ulong)Random.Shared.Next(420, 69420);
        var checkSum = (uint)Random.Shared.Next(42069, 69420);
        var guid = Guid.NewGuid();
        var bytes = Serializer(msg);
        return (offset, timestamp, guid, headersLength, checkSum, bytes);
    }
    internal static (ulong offset, ulong timestamp, Guid guid, byte[] payload) CreateMessageResponseGenerics()
    {
        ulong offset = (ulong)Random.Shared.Next(6, 69);
        var timestamp = (ulong)Random.Shared.Next(420, 69420);
        var guid = Guid.NewGuid();
        var bytes = Encoding.UTF8.GetBytes("Hello");
        return (offset, timestamp, guid, bytes);
    }
    internal static (ulong offset, ulong timestamp, Guid guid, byte[] payload) CreateMessageResponseFields<TMessage>(TMessage message, Func<TMessage, byte[]> serializer)
    {
        ulong offset = (ulong)Random.Shared.Next(6, 69);
        var timestamp = (ulong)Random.Shared.Next(420, 69420);
        var guid = Guid.NewGuid();
        var bytes = serializer(message);
        return (offset, timestamp, guid, bytes);
    }

    internal static MessageSendRequest CreateMessageSendRequest()
    {
        var valBytes = new byte[4];
        var streamId = Identifier.Numeric(1);
        var topicId = Identifier.Numeric(1);
        BinaryPrimitives.WriteInt32LittleEndian(valBytes, Random.Shared.Next(1, 69));
        return new MessageSendRequest
        {

            StreamId = streamId,
            TopicId = topicId,
            Partitioning = new Partitioning
            {
                Kind = PartitioningKind.PartitionId,
                Length = 4,
                Value = valBytes,
            },
            Messages = new List<Message>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Payload = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(DummyObjFactory.CreateDummyObject())),
                    Headers = null,
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Payload =  Encoding.UTF8.GetBytes(JsonSerializer.Serialize(DummyObjFactory.CreateDummyObject())),
                    Headers = null,
                }

            },
        };
    }
    internal static MessageSendRequest CreateMessageSendRequest(int streamId, int topicId, int partitionId, IList<Message>? messages = null)
    {
        return new MessageSendRequest
        {

            StreamId = Identifier.Numeric(streamId),
            TopicId = Identifier.Numeric(topicId),
            Partitioning = Partitioning.PartitionId(partitionId),
            Messages = messages ?? GenerateDummyMessages(Random.Shared.Next(1, 69), Random.Shared.Next(69, 420))
        };
    }

    private static byte[] SerializeDummyMessage(DummyMessage message)
    {
        var bytes = new byte[4 + 4 + message.Text.Length];
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan()[..4], message.Id);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan()[4..8], message.Text.Length);
        Encoding.UTF8.GetBytes(message.Text).CopyTo(bytes.AsSpan()[8..]);
        return bytes;
    }
     internal static IList<DummyMessage> GenerateDummyMessages(int count)
     {
         return Enumerable.Range(1, count).Select(i => new DummyMessage
         {
             Id = Random.Shared.Next(1, 69),
             Text = Utility.RandomString(Random.Shared.Next(69, 100))
         }).ToList();
     }

    internal static Func<byte[], DummyMessage> DeserializeDummyMessage
        => bytes =>
        {
            var id = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan()[..4]);
            var textLength = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan()[4..8]);
            var text = Encoding.UTF8.GetString(bytes.AsSpan()[8..(8 + textLength)]);
            return new DummyMessage { Id = id, Text = text };
        };
    internal static IList<Message> GenerateMessages(int count, Dictionary<HeaderKey, HeaderValue>? Headers = null)
    {
        return Enumerable.Range(1, count).Select(i => new Message
        {
            Id = Guid.NewGuid(),
            Headers = Headers,
            Payload = SerializeDummyMessage(new DummyMessage { Id = Random.Shared.Next(1, 69), Text = Utility.RandomString(Random.Shared.Next(20, 69)) })
        }).ToList();
    }
    internal static IList<Message> GenerateDummyMessages(int count, int payloadLen, Dictionary<HeaderKey, HeaderValue>? Headers = null)
    {
        return Enumerable.Range(1, count).Select(i => new Message
        {
            Id = Guid.NewGuid(),
            Headers = Headers,
            Payload = Enumerable.Range(1, payloadLen).Select(x => (byte)x).ToArray()
        }).ToList();
    }
    internal static MessageFetchRequest CreateMessageFetchRequestConsumer()
    {
        return new MessageFetchRequest
        {
            Count = Random.Shared.Next(1, 10),
            AutoCommit = true,
            Consumer = Consumer.New(1),
            PartitionId = Random.Shared.Next(1, 10),
            PollingStrategy = PollingStrategy.Offset(69420),
            StreamId = Identifier.Numeric(Random.Shared.Next(1, 10)),
            TopicId = Identifier.Numeric(Random.Shared.Next(1, 10)),
        };
    }
    internal static MessageFetchRequest CreateMessageFetchRequestConsumer(int count, int streamId, int topicId, int partitionId, int consumerId = 1)
    {
        return new MessageFetchRequest
        {
            Count = count,
            AutoCommit = true,
            Consumer = Consumer.New(consumerId),
            PartitionId = partitionId,
            PollingStrategy = PollingStrategy.Next(),
            StreamId = Identifier.Numeric(streamId),
            TopicId = Identifier.Numeric(topicId),
        };
    }
    internal static MessageFetchRequest CreateMessageFetchRequestConsumerGroup(int count, int streamId, int topicId, int partitionId, int consumerGroupId)
    {
        return new MessageFetchRequest
        {
            Count = count,
            AutoCommit = true,
            Consumer = Consumer.Group(consumerGroupId),
            PartitionId = partitionId,
            PollingStrategy = PollingStrategy.Next(),
            StreamId = Identifier.Numeric(streamId),
            TopicId = Identifier.Numeric(topicId),
        };
    }
    internal static Dictionary<HeaderKey, HeaderValue> GenerateMessageHeaders(int count)
    {
        var headers = new Dictionary<HeaderKey, HeaderValue>();
        for (int i = 0; i < count; i++)
        {
            headers.Add(
                HeaderKey.New(Utility.RandomString(Random.Shared.Next(50, 254))),
                Random.Shared.Next(1, 12) switch
                {
                    1 => HeaderValue.FromBytes(Encoding.UTF8.GetBytes(Utility.RandomString(Random.Shared.Next(50, 254)))),
                    2 => HeaderValue.FromString(Utility.RandomString(Random.Shared.Next(25, 254))),
                    3 => HeaderValue.FromBool(Random.Shared.Next(0, 1) switch { 0 => false, 1 => true, _ => false }),
                    4 => HeaderValue.FromInt32(Random.Shared.Next(69, 420)),
                    5 => HeaderValue.FromInt64(Random.Shared.NextInt64(6942023, 98723131)),
                    6 => HeaderValue.FromInt128(Guid.NewGuid().ToByteArray().ToInt128()),
                    7 => HeaderValue.FromGuid(Guid.NewGuid()),
                    8 => HeaderValue.FromUInt32((uint)Random.Shared.Next(1, 69)),
                    9 => HeaderValue.FromUInt64((ulong)Random.Shared.Next(1, 69)),
                    10 => HeaderValue.FromUInt128(Guid.NewGuid().ToUInt128()),
                    11 => HeaderValue.FromFloat(Random.Shared.NextSingle()),
                    12 => HeaderValue.FromDouble(Random.Shared.NextDouble()),
                    _ => HeaderValue.FromUInt64((ulong)Random.Shared.Next(1, 69))
                });
        }
        return headers;
    }
    internal static MessageResponseHttp CreateMessageResponse()
    {
        return new MessageResponseHttp
        {
            Offset = (ulong)Random.Shared.Next(1, 10),
            Payload = Convert.ToBase64String("TROLOLO"u8.ToArray()),
            Timestamp = 12371237821L,
            State = MessageState.Available,
            Checksum = (uint)Random.Shared.Next(42069, 69420),
            Id = new UInt128(69, 420),
            Headers = null
        };
    }
}
internal class MessageResponseHttp
{
    public required ulong Offset { get; init; }
    public required uint Checksum { get; init; }
    public required ulong Timestamp { get; init; }
    public UInt128 Id { get; init; }
    public required string Payload { get; init; }

    public Dictionary<HeaderKey, HeaderValue>? Headers { get; init; }
    public required MessageState State { get; init; }
}
internal class DummyObject
{
    public required int Id { get; set; }
    public required string Text { get; set; }
}