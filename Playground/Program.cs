// See https://aka.ms/new-console-template for more information

using System.Buffers.Binary;
using System.Text;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;
using Iggy_SDK.Factory;
using Iggy_SDK.Identifiers;
using Playground;

var bus = MessageStreamFactory.CreateMessageStream(options =>
{
	options.BaseAdress = "127.0.0.1:8090";
	options.Protocol = Protocol.Tcp;
	options.ReceiveBufferSize = Int32.MaxValue;
	options.SendBufferSize = Int32.MaxValue;
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
		Span<byte> bytes = stackalloc byte[4 + product.Description.Length + product.Name.Length];
		
		Encoding.UTF8.GetBytes(product.Name).CopyTo(bytes[..product.Name.Length]);
		Encoding.UTF8.GetBytes(product.Description).CopyTo(bytes[product.Name.Length..]);
		BinaryPrimitives.WriteInt32LittleEndian(bytes[(product.Name.Length + product.Description.Length)..], product.Quantity);
		
		return bytes.ToArray();
	});