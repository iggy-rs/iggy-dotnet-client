using DotNet.Testcontainers.Builders;
using Iggy_SDK;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;
using Iggy_SDK.Factory;
using Iggy_SDK.Messages;
using Iggy_SDK.MessageStream;
using Iggy_SDK_Tests.Utils.Messages;
using Iggy_SDK_Tests.Utils.Streams;
using Iggy_SDK_Tests.Utils.Topics;
using Iggy_SDK.Configuration;
using Iggy_SDK.Kinds;
using Microsoft.Extensions.Logging.Abstractions;
using IContainer = DotNet.Testcontainers.Containers.IContainer;

namespace Iggy_SDK_Tests.E2ETests.Fixtures.Tcp;

public sealed class IggyTcpPollMessagesFixture : IAsyncLifetime
{
    private readonly IContainer _container = new ContainerBuilder().WithImage("iggyrs/iggy:latest")
        //.WithPortBinding(3000, true)
        .WithPortBinding(8090, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8090))
        //.WithPortBinding(8080, true)
        .Build();


    public required IMessageStream sut;

    private static readonly StreamRequest StreamRequest = StreamFactory.CreateStreamRequest();
    private static readonly TopicRequest TopicRequest = TopicFactory.CreateTopicRequest();
    private static readonly TopicRequest HeadersTopicRequest = TopicFactory.CreateTopicRequest();
    public const int MessageCount = 1000000;

    public readonly int StreamId = StreamRequest.StreamId;
    public readonly int TopicId = TopicRequest.TopicId;
    public readonly int HeadersTopicId = HeadersTopicRequest.TopicId;

    public readonly int PartitionId = 1;
    public readonly int HeadersCount = 6;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        sut = MessageStreamFactory.CreateMessageStream(options =>
        {
            options.BaseAdress = $"127.0.0.1:{_container.GetMappedPublicPort(8090)}";
            options.Protocol = Protocol.Tcp;
            options.SendMessagesOptions = x =>
            {
                x.PollingInterval = TimeSpan.FromMilliseconds(100);
                x.MaxMessagesPerBatch = 1000;
                x.MaxRequestsInPoll = 8912;
            };
            options.LoggerFactory = NullLoggerFactory.Instance;
        });
        await sut.CreateStreamAsync(StreamRequest);
        await sut.CreateTopicAsync(Identifier.Numeric(StreamRequest.StreamId), TopicRequest);
        await sut.CreateTopicAsync(Identifier.Numeric(StreamRequest.StreamId), HeadersTopicRequest);

        await sut.SendMessagesAsync(Identifier.Numeric(StreamId), Identifier.Numeric(TopicId),
            Partitioning.PartitionId(PartitionId), MessageFactory.GenerateDummyMessages(MessageCount), MessageFactory.Serializer);
        
        await sut.SendMessagesAsync(Identifier.Numeric(StreamId), Identifier.Numeric(HeadersTopicId),
            Partitioning.PartitionId(PartitionId), MessageFactory.GenerateDummyMessages(MessageCount), MessageFactory.Serializer, 
            headers: MessageFactory.GenerateMessageHeaders(HeadersCount));

        await Task.Delay(2500);
    }

    public async Task DisposeAsync()
    {
        await _container.StopAsync();
    }
}