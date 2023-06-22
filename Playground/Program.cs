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
 /*
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
*/
/*var rng = new Random();
var orderCreated = new OrderCreated
{
	Id = (ulong)rng.Next(),
	CurrencyPair = "BTC/USDT",
	Price = (double)rng.Next(),
	Quantity = (double)rng.Next(),
	Side = "Buy",
	Timestamp = (ulong)rng.Next()
};
var orderRejected = new OrderRejected()
{
	Id = (ulong)rng.Next(),
	Timestamp = (ulong)rng.Next(),
	Reason = "Order rejected"
};
var orderConfirmed = new OrderConfirmed()
{
	Id = (ulong)rng.Next(),
	Price = (double)rng.Next(),
	Timestamp = (ulong)rng.Next()
};

 var sendObjects = await bus.SendMessagesAsync(new MessageSendRequest
 {
	 StreamId = 1,
	 TopicId = 1,
	 KeyKind = Keykind.PartitionId,
	 KeyValue = 1,
	 Messages = new List<DummyMessage>
	 {
		new DummyMessage { Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(orderCreated))), Id = 1},
		new DummyMessage { Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(orderRejected))), Id = 1},
		new DummyMessage { Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(orderConfirmed))), Id = 1}
	 }
 });*/

 var result = await bus.GetMessagesAsync(new MessageFetchRequest
 {
     StreamId = 1,
     TopicId = 1,
     ConsumerId = 1,
     PartitionId = 1,
     PollingStrategy = MessagePolling.Offset,
     Value = 0,
     Count = 20,
     AutoCommit = true
});

Console.WriteLine();
Console.ReadKey();






