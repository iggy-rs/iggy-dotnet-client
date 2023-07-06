using System.Net.Sockets;
using System.Text;


//this is payload
var message = "";
var messageLength = message.Length + 1;
var messageBytes = new byte[4 + messageLength];

using var client = new TcpClient("127.0.0.1", 8090);
var stream = client.GetStream();

byte[] messageLengthBytes = BitConverter.GetBytes(messageLength);
Buffer.BlockCopy(messageLengthBytes, 0, messageBytes, 0, messageLengthBytes.Length);
byte commandBytes = Convert.ToByte(1);
byte[] payloadBytes = Encoding.ASCII.GetBytes(message);
messageBytes[4] = commandBytes;

Buffer.BlockCopy(payloadBytes, 0, messageBytes, 5, payloadBytes.Length);

await stream.WriteAsync(messageBytes, 0, messageBytes.Length);
Console.WriteLine($"client sent message: {message}");

var buffer = new byte[1_024];
var received = await stream.ReadAsync(buffer);
var response = Encoding.ASCII.GetString(buffer, 0, received);
Console.WriteLine(
	$"client received : {response}");
await stream.DisposeAsync();
client.Close();



// var bus = MessageStreamFactory.CreateMessageStream(options =>
// {
//     options.BaseAdress = "http://localhost:3000";
//     options.Protocol = Protocol.Http;
// });
//
// var groups = await bus.GetGroupsAsync(1, 1);

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






