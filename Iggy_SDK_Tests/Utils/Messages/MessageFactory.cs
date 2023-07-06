using System.Text;
using System.Text.Json;
using Iggy_SDK_Tests.Utils.DummyObj;
using Iggy_SDK.Contracts;
using Iggy_SDK.Enums;
using Iggy_SDK.Messages;

namespace Iggy_SDK_Tests.Utils.Messages;

public static class MessageFactory
{
	public static MessageSendRequest CreateMessageSendRequest()
	{
		return new MessageSendRequest
		{
			KeyKind = Keykind.PartitionId,
			KeyValue = Random.Shared.Next(1, 10),
			StreamId = Random.Shared.Next(1, 10),
			TopicId = Random.Shared.Next(1, 10),
			Messages = new List<IMessage>
			{
				new DummyMessage
				{
					Id = Random.Shared.Next(1, 10),
					Payload = JsonSerializer.Serialize(DummyObjFactory.CreateDummyObject())
				},
				new DummyMessage
				{
					Id = Random.Shared.Next(1, 10),
					Payload =  JsonSerializer.Serialize(DummyObjFactory.CreateDummyObject())
				}

			}
		};
	}

	public static MessageFetchRequest CreateMessageFetchRequest()
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

	public static MessageResponse CreateMessageResponse()
	{
		return new MessageResponse
		{
			Offset = Random.Shared.Next(1, 10),
			Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes("TROLOLO")),
			Timestamp = 12371237821L,
			Id = UInt128.MaxValue,
		};
	}
}

public class DummyMessage  : IMessage
{
	public int Id { get; set; }
	public string Payload { get; set; }
}

public class DummyObject
{
	public int Id { get; set; }
	public string Text { get; set; }
}