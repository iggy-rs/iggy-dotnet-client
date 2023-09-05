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
        var bytes = Encoding.UTF8.GetBytes(RandomString(Random.Shared.Next(6, 69)));
        int headersLength = Random.Shared.Next(1, 69);
        return (offset, timestamp, guid, headersLength, checkSum, bytes);
    }
    internal static (ulong offset, ulong timestamp, Guid guid, int headersLength, uint checkSum, byte[] payload) CreateMessageResponseFieldsTMessage()
    {
        var msg = new DummyMessage
        {
            Id = Random.Shared.Next(1, 69),
            Text = "Hello"
        };
        Func<DummyMessage, byte[]> serializer = msg =>
        {
            Span<byte> bytes = stackalloc byte[4 + 4 + msg.Text.Length];
            BinaryPrimitives.WriteInt32LittleEndian(bytes[..4], msg.Id);
            BinaryPrimitives.WriteInt32LittleEndian(bytes[4..8], msg.Text.Length);
            Encoding.UTF8.GetBytes(msg.Text).CopyTo(bytes[8..]);
            return bytes.ToArray();
        };
        ulong offset = (ulong)Random.Shared.Next(6, 69);
        int headersLength = Random.Shared.Next(1, 69);
        var timestamp = (ulong)Random.Shared.Next(420, 69420);
        var checkSum = (uint)Random.Shared.Next(42069, 69420);
        var guid = Guid.NewGuid();
        var bytes = serializer(msg);
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
            Messages = messages ?? GenerateDummyMessages(Random.Shared.Next(1, 69), Random.Shared.Next(69 , 420))
        };
    }

    internal static IList<Message> GenerateDummyMessages(int count, int paylaodLen, Dictionary<HeaderKey, HeaderValue>? Headers = null)
    {
        return Enumerable.Range(1, count).Select(i => new Message{
            Id = Guid.NewGuid(),
            Headers = Headers,
            Payload = Enumerable.Range(1, paylaodLen).Select(x => (byte)x).ToArray()
        }).ToList();
    }
    internal static MessageFetchRequest CreateMessageFetchRequest()
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
    internal static Dictionary<HeaderKey, HeaderValue> GenerateMessageHeaders(int count)
    {
        var headers = new Dictionary<HeaderKey, HeaderValue>();
        for(int i = 0; i < count; i++)
        {
        headers.Add(
            HeaderKey.New(RandomString(Random.Shared.Next(50, 254))),
            Random.Shared.Next(1, 12) switch

            {
                1 => HeaderValue.Raw(Encoding.UTF8.GetBytes(RandomString(Random.Shared.Next(50, 254)))),
                2 => HeaderValue.String(RandomString(Random.Shared.Next(25, 254))),
                3 => HeaderValue.Bool(Random.Shared.Next(0,1) switch { 0 => false, 1 => true, _ => false}),
                4 => HeaderValue.Int32(Random.Shared.Next(69, 420)),
                5 => HeaderValue.Int64(Random.Shared.NextInt64(6942023, 98723131)),
                6 => HeaderValue.Int128(Guid.NewGuid().ToByteArray().ToInt128()),
                7 => HeaderValue.Guid(Guid.NewGuid()),
                8 => HeaderValue.UInt32((uint)Random.Shared.Next(1, 69)),
                9 => HeaderValue.UInt64((ulong)Random.Shared.Next(1, 69)),
                10 => HeaderValue.UInt128(Guid.NewGuid().ToUInt128()),
                11 => HeaderValue.Float32(Random.Shared.NextSingle()),
                12 => HeaderValue.Float64(Random.Shared.NextDouble()),
                _ =>  HeaderValue.UInt64((ulong)Random.Shared.Next(1, 69))
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
    private static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
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