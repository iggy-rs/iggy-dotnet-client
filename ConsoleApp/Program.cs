using ConsoleApp;
using Iggy_SDK.Factory;
using Iggy_SDK.Protocols;

var bus = MessageStreamFactory.CreateMessageStream(options =>
{
    options.BaseAdress = "http://localhost:3000";
    options.Protocol = Protocol.Http;
});

//VGVzdA==
//TWVzc2FnZQ==
var result = await bus.SendMessagesAsync(1,1,"partition_id",1, new []
{
    new DummyMessage { Id = 0, Payload = "VGVzdA==" },
    new DummyMessage { Id = 0, Payload = "TWVzc2FnZQ==" }
});

Console.WriteLine();

Console.ReadKey();






