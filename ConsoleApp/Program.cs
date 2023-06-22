using ConsoleApp;
using Iggy_SDK.Contracts;
using Iggy_SDK.Enums;
using Iggy_SDK.Factory;

var bus = MessageStreamFactory.CreateMessageStream(options =>
{
    options.BaseAdress = "http://localhost:3000";
    options.Protocol = Protocol.Http;
});

//VGVzdA==
//TWVzc2FnZQ==
// var result = await bus.SendMessagesAsync(1,1,Keykind.PartitionId,1, new []
// {
//     new DummyMessage { Id = 0, Payload = "VGVzdA==" },
//     new DummyMessage { Id = 0, Payload = "TWVzc2FnZQ==" }
// });

var result = await bus.GetMessagesAsync(new MessageRequest
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






