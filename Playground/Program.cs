using System.Net;
using System.Net.Security;
using Iggy_SDK.Contracts;
using Iggy_SDK.Enums;
using Iggy_SDK.Factory;

Console.WriteLine();

// var bus = MessageStreamFactory.CreateMessageStream(options =>
// {
// 	options.BaseAdress = "127.0.0.1:8090";
// 	options.Protocol = Protocol.Tcp;
// });
//
// var stream = await bus.GetStreamByIdAsync(1);
// Console.WriteLine();


var bus = MessageStreamFactory.CreateMessageStream(options =>
{
     options.BaseAdress = "localhost:8090";
     options.Protocol = Protocol.Tcp;
});

var createStream = await bus.CreateStreamAsync(new StreamRequest
{
	Name = "Test Stream",
	StreamId = 1
});
Console.WriteLine();

// var groupResponse = await bus.GetGroupByIdAsync(1, 1, 1);

/*
var streamCreation = await bus.CreateStreamAsync(new CreateStreamRequest
{
	Name = "Test Stream",
	StreamId = 1
});
	

var stream = await bus.GetStreamByIdAsync(1);
var streams = await bus.GetStreamsAsync();

var topic = await bus.CreateTopicAsync(1, new TopicRequest
{
	Name = "Test Topic From C#",
	PartitionsCount = 3,
	TopicId = 1,
});

var topics = await bus.GetTopicsAsync(1);
var topicOne = await bus.GetTopicByIdAsync(1, 1);
Console.WriteLine();
*/

// var offset = await bus.UpdateOffsetAsync(1, 1, new OffsetContract
// {
// 	Offset = 0,
// 	ConsumerId = 1,
// 	PartitionId = 1,
// });
// var offset = await bus.GetOffsetAsync(new OffsetRequest
// {
// 	StreamId = 1,
// 	TopicId = 1,
// 	ConsumerId = 1,
// 	PartitionId = 1
// });


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

//  var result = await bus.GetMessagesAsync(new MessageFetchRequest
//  {
//      StreamId = 1,
//      TopicId = 1,
//      ConsumerId = 1,
//      PartitionId = 1,
//      PollingStrategy = MessagePolling.Offset,
//      Value = 0,
//      Count = 20,
//      AutoCommit = true
// });






