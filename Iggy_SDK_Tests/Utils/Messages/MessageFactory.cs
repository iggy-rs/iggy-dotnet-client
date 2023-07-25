using System.Text;
using System.Text.Json;
using Iggy_SDK_Tests.Utils.DummyObj;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;
using Iggy_SDK.Messages;

namespace Iggy_SDK_Tests.Utils.Messages;

internal static class MessageFactory
{
	internal static (ulong offset, ulong timestamp, Guid guid, byte[] payload) CreateMessageResponseFields()
	{
		ulong offset = (ulong)Random.Shared.Next(6, 69);
		var timestamp = (ulong)Random.Shared.Next(420, 69420);
		var guid = Guid.NewGuid();
		var bytes = Encoding.UTF8.GetBytes(RandomString(Random.Shared.Next(6, 69)));
		return (offset, timestamp, guid, bytes);
	}

	internal static MessageSendRequest CreateMessageSendRequest()
	{
		return new MessageSendRequest
		{
			KeyKind = Keykind.PartitionId,
			KeyValue = Random.Shared.Next(1, 10),
			Messages = new List<Message>
			{
				new()
				{
					Id = Guid.NewGuid(),
					Payload = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(DummyObjFactory.CreateDummyObject()))
				},
				new() 
				{
					Id = Guid.NewGuid(),
					Payload =  Encoding.UTF8.GetBytes(JsonSerializer.Serialize(DummyObjFactory.CreateDummyObject()))
				}

			}
		};
	}
	internal static MessageFetchRequest CreateMessageFetchRequest()
	{
		return new MessageFetchRequest
		{
			Count = Random.Shared.Next(1, 10),
			Value = (ulong)Random.Shared.Next(1, 10),
			AutoCommit = true,
			ConsumerType = ConsumerType.Consumer,
			ConsumerId = Random.Shared.Next(1, 10),
			PartitionId = Random.Shared.Next(1, 10),
			PollingStrategy = MessagePolling.Offset,
			StreamId = Random.Shared.Next(1, 10),
			TopicId = Random.Shared.Next(1, 10)
		};
	}

	internal static MessageResponseHttp CreateMessageResponse()
	{
		return new MessageResponseHttp
		{
			Offset = (ulong)Random.Shared.Next(1, 10),
			Payload = Convert.ToBase64String("TROLOLO"u8.ToArray()),
			Timestamp = 12371237821L,
			Id = new UInt128(69,420)
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
}
internal class DummyObject
{
	public int Id { get; set; }
	public string Text { get; set; }
}