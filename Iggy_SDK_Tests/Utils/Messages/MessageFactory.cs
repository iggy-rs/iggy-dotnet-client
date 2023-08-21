using System.Buffers.Binary;
using System.Text;
using System.Text.Json;
using Iggy_SDK;
using Iggy_SDK_Tests.Utils.DummyObj;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;
using Iggy_SDK.Headers;
using Iggy_SDK.Kinds;
using Iggy_SDK.Messages;

namespace Iggy_SDK_Tests.Utils.Messages;

internal static class MessageFactory
{
	internal static (ulong offset, ulong timestamp, Guid guid, int headersLength, byte[] payload) CreateMessageResponseFields()
	{
		ulong offset = (ulong)Random.Shared.Next(6, 69);
		var timestamp = (ulong)Random.Shared.Next(420, 69420);
		var guid = Guid.NewGuid();
		var bytes = Encoding.UTF8.GetBytes(RandomString(Random.Shared.Next(6, 69)));
		int headersLength = Random.Shared.Next(1, 69);
		return (offset, timestamp, guid, headersLength, bytes);
	}
	internal static (ulong offset, ulong timestamp, Guid guid, int headersLength, byte[] payload) CreateMessageResponseFieldsTMessage()
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
		var guid = Guid.NewGuid();
		var bytes = serializer(msg);
		return (offset, timestamp, guid, headersLength, bytes);
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
		BinaryPrimitives.WriteInt32LittleEndian(valBytes, Random.Shared.Next(1,69));
		return new MessageSendRequest
		{
			
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
	internal static MessageFetchRequest CreateMessageFetchRequest()
	{
		return new MessageFetchRequest
		{
			Count = Random.Shared.Next(1, 10),
			AutoCommit = true,
			Consumer = Consumer.New(1),
			PartitionId = Random.Shared.Next(1, 10),
			PollingStrategy = PollingStrategy.Offset(69420),
			StreamId = Identifier.Numeric(Random.Shared.Next(1,10)),
			TopicId = Identifier.Numeric(Random.Shared.Next(1,10)),
		};
	}

	internal static MessageResponseHttp CreateMessageResponse()
	{
		return new MessageResponseHttp
		{
			Offset = (ulong)Random.Shared.Next(1, 10),
			Payload = Convert.ToBase64String("TROLOLO"u8.ToArray()),
			Timestamp = 12371237821L,
			Id = new UInt128(69,420),
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
	public required ulong Timestamp { get; init; }
	public UInt128 Id { get; init; }
	public required string Payload { get; init; }

	public Dictionary<HeaderKey, HeaderValue>? Headers { get; init; }
}
internal class DummyObject
{
	public required int Id { get; set; }
	public required string Text { get; set; }
}