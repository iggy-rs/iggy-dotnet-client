using ConsoleApp;
using Iggy_SDK.Contracts;
using Iggy_SDK.Enums;
using Iggy_SDK.Factory;


var bus = MessageStreamFactory.CreateMessageStream(options =>
{
	options.BaseAdress = "127.0.0.1:8090";
	options.Protocol = Protocol.Tcp;
});

var order = new Shared.OrderCreated()
{
	Id = 69,
	Timestamp = 123321312,
	Price = 12.23,
	Quantity = 12,
	Side = "Buy",
	CurrencyPair = "PLN/USD"
};
var env = order.ToJson();
Console.WriteLine(env);

var createMessage = await bus.SendMessagesAsync(new MessageSendRequest
{
	StreamId = 1,
	TopicId = 1,
	KeyKind = Keykind.PartitionId,
	KeyValue = 3,
	Messages = new List<DummyMessage>()
	{
		new DummyMessage
		{
			Id = 69,
			Payload = env
		}
	}
});

var resp = await bus.GetMessagesAsync(new MessageFetchRequest
{
	Count = 1,
	AutoCommit = true,
	ConsumerId = 1,
	PollingStrategy = MessagePolling.Next,
	Value = 0,
	PartitionId = 3,
	StreamId = 1,
	TopicId = 1,
});
Console.WriteLine();

//const int initialBytesLength = 4;

// using var client = new TcpClient("127.0.0.1", 8090);
// var stream = client.GetStream();
// var newStream = new StreamRequest
// {
// 	Name = "stream from tcp",
// 	StreamId = 3,
// };
	

//var message = BitConverter.GetBytes(1);
/*var message = new byte[0];
var messageLength = message.Length + 1;

byte commandByte = CommandCodes.CREATE_STREAM_CODE;
byte[] messageLengthBytes = BitConverter.GetBytes(messageLength);

byte[] CreatePayload()
{
	Span<byte> messageBytes = stackalloc byte[initialBytesLength + messageLength];
	messageLengthBytes.CopyTo(messageBytes);
	messageBytes[initialBytesLength] = commandByte;
	message.CopyTo(messageBytes[(initialBytesLength + 1)..]);
	return messageBytes.ToArray();
}

var messageBytes = CreatePayload();

await stream.WriteAsync(messageBytes, 0, messageBytes.Length);
Console.WriteLine($"client sent message: {message}");

var buffer = new byte[5];
await stream.ReadExactlyAsync(buffer);
if (buffer.Length != 5)
{
	Console.WriteLine("received wrong response");
}

var status = buffer[0];
var length = buffer[1];
Console.WriteLine($"Status {status}");
Console.WriteLine($"Length {length}");
//var response = Encoding.UTF8.GetString(buffer, 0, readBytes);
if (status != 0)
{
	Console.WriteLine("Received an invalid response status");
}
if (length <= 1)
{
	Console.WriteLine("EMPTY RESPONSE");
}
var responseBuffer = new byte[length];
await stream.ReadExactlyAsync(responseBuffer);
var streams = BinaryMapper.MapTopics(responseBuffer); */ 


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






