using ConsoleApp;
using Iggy_SDK.Contracts;
using Iggy_SDK.Enums;
using Iggy_SDK.Factory;

var bus = MessageStreamFactory.CreateMessageStream(options =>
{
    options.BaseAdress = "http://localhost:3000";
    options.Protocol = Protocol.Http;
});

// VGVzdA==
// TWVzc2FnZQ==
 var send = await bus.SendMessagesAsync(new MessageSendRequest
 {
	 StreamId = 1,
	 TopicId = 1,
	 KeyKind = Keykind.PartitionId,
	 KeyValue = 1,
	 Messages = new List<DummyMessage>
	 {
		new DummyMessage { Payload = "VgVzdA==", Id = 0},
		new DummyMessage { Payload = "TWVzc2FnZQ==", Id = 0}
	 }
 });

 var result = await bus.GetMessagesAsync(new MessageFetchRequest
 {
     StreamId = 1,
     TopicId = 1,
     ConsumerId = 1,
     PartitionId = 1,
     PollingStrategy = MessagePolling.Offset,
     Value = 0,
     Count = 10,
     AutoCommit = false
});

Console.WriteLine();
Console.ReadKey();






