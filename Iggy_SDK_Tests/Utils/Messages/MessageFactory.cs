using System.Text;
using System.Text.Json;
using Iggy_SDK_Tests.Utils.DummyObj;
using Iggy_SDK.Contracts;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;
using Iggy_SDK.Messages;

namespace Iggy_SDK_Tests.Utils.Messages;

internal static class MessageFactory
{
	internal static MessageSendRequest CreateMessageSendRequest()
	{
		return new MessageSendRequest
		{
			KeyKind = Keykind.PartitionId,
			KeyValue = Random.Shared.Next(1, 10),
			Messages = new List<Message>
			{
				new Message
				{
					Id = Guid.NewGuid(),
					Payload = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(DummyObjFactory.CreateDummyObject()))
				},
				new Message
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
			Value = Random.Shared.Next(1, 10),
			AutoCommit = true,
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
			Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes("TROLOLO")),
			Timestamp = 12371237821L,
			Id = 123123123123
		};
	}
}

internal class DummyObject
{
	public int Id { get; set; }
	public string Text { get; set; }
}