﻿using Benchmarks;
using Iggy_SDK;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;
using Iggy_SDK.Factory;
using Iggy_SDK.IggyClient;
using Microsoft.Extensions.Logging;

const int messagesCount = 1000;
const int messagesBatch = 1000;
const int messageSize = 1000;
const int producerCount = 3;
const int startingStreamId = 100;
const int topicId = 1;
Dictionary<int, IIggyClient> clients = new();
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddFilter("Iggy_SDK.MessageStream.Implementations;", LogLevel.Trace)
        .AddConsole();
});

for (int i = 0; i < producerCount; i++)
{
    var bus = MessageStreamFactory.CreateMessageStream(options =>
    {
        options.BaseAdress = "127.0.0.1:8090";
        options.Protocol = Protocol.Tcp;
        options.MessageBatchingSettings = x =>
        {
            x.Enabled = false;
            x.MaxMessagesPerBatch = 1000;
            x.Interval = TimeSpan.Zero;
        };
#if OS_LINUX
		options.ReceiveBufferSize = Int32.MaxValue;
		options.SendBufferSize = Int32.MaxValue;
#elif OS_WINDOWS
        options.ReceiveBufferSize = Int32.MaxValue;
        options.SendBufferSize = Int32.MaxValue;
#elif OS_MAC
		options.ReceiveBufferSize = 7280*1024;
		options.SendBufferSize = 7280*1024;
#endif
    }, loggerFactory);
    clients[i] = bus;
}

try
{
    for (int i = 0; i < producerCount; i++)
    {
        await clients[0].CreateStreamAsync(new StreamRequest
        {
            Name = $"Test bench stream_{i}",
            StreamId = startingStreamId + i
        });
        await clients[0].CreateTopicAsync(Identifier.Numeric(startingStreamId + i), new TopicRequest
        {
            Name = $"Test bench topic_{i}",
            PartitionsCount = 1,
            TopicId = topicId,
            ReplicationFactor = 3
        });
    }
}
catch
{
    Console.WriteLine("Failed to create streams, they already exist.");
}

List<Task> tasks = new();

for (int i = 0; i < producerCount; i++)
{
    tasks.Add(SendMessage.Create(clients[i], i, producerCount, messagesBatch, messagesCount, messageSize,
        Identifier.Numeric(startingStreamId + i),
        Identifier.Numeric(topicId)));
}

await Task.WhenAll(tasks);

try
{
    for (int i = 0; i < producerCount; i++)
    {
        await clients[0].DeleteStreamAsync(Identifier.Numeric(startingStreamId + i));
    }
}
catch
{
    Console.WriteLine("Failed to delete streams");
}