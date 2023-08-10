using System.Buffers.Binary;
using System.Text;
using Iggy_SDK;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;
using Iggy_SDK.Factory;
using Iggy_SDK.Kinds;
using Playground;

var bus = MessageStreamFactory.CreateMessageStream(options =>
{
	options.BaseAdress = "http://127.0.0.1:3000";
	options.Protocol = Protocol.Http;
	options.ReceiveBufferSize = int.MaxValue;
	options.SendBufferSize = int.MaxValue;
});

List<Product> products = new();
products.Add(new Product
{
	Quantity = 12,
	Name = "Product_1",
	Description = "Description_1"
});
products.Add(new Product
{
	Quantity = 12,
	Name = "Product_2",
	Description = "Description_2"
});
products.Add(new Product
{
	Quantity = 12,
	Name = "Product_3",
	Description = "Description_3"
});
try
{
	await bus.CreateStreamAsync(new StreamRequest
	{
		Name = "test-stream",
		StreamId = 1,
	});
	await bus.CreateTopicAsync(Identifier.Numeric(1), new TopicRequest
	{
		Name = "test-topic",
		PartitionsCount = 3,
		TopicId = 1,
	});
}
catch
{
	Console.WriteLine("cannot create stream and topic");
}

await bus.SendMessagesAsync<Product>(Identifier.Numeric(1), Identifier.Numeric(1),
	Partitioning.PartitionId(1), products, static product =>
	{
		var descLength = product.Description.Length;
		var nameLength = product.Name.Length;
		Span<byte> bytes = stackalloc byte[4 + descLength + nameLength + 4];

		BinaryPrimitives.WriteInt32LittleEndian(bytes[..4], product.Quantity);
		BinaryPrimitives.WriteInt32LittleEndian(bytes[4..8], nameLength);
		Encoding.UTF8.GetBytes(product.Name).CopyTo(bytes[8..(nameLength + 8)]);
		Encoding.UTF8.GetBytes(product.Description).CopyTo(bytes[(nameLength + 8)..]);

		return bytes.ToArray();
	});

var messages = (await bus.PollMessagesAsync<Product>(new MessageFetchRequest
{
	Consumer = Consumer.New(1),
	Count = 3,
	TopicId = Identifier.Numeric(1),
	StreamId = Identifier.Numeric(1),
	PartitionId = 1,
	PollingStrategy = MessagePolling.Next,
	Value = 0,
	AutoCommit = true,
}, bytes =>
{
	var quantity = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan()[..4]);
	var nameLength = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan()[4..8]);
	var name = Encoding.UTF8.GetString(bytes.AsSpan()[8..(nameLength + 8)]);
	var description = Encoding.UTF8.GetString(bytes.AsSpan()[(nameLength + 8)..]);
	return new Product
	{
		Quantity = quantity,
		Name = name,
		Description = description
	};
})).ToList();

foreach (var obj in messages)
{
	Console.WriteLine(obj.Message.Description);
}