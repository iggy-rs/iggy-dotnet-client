using DotNet.Testcontainers.Builders;
using Iggy_SDK;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;
using Iggy_SDK.Factory;
using Iggy_SDK_Tests.Utils.Messages;
using Iggy_SDK_Tests.Utils.Streams;
using Iggy_SDK_Tests.Utils.Topics;
using Iggy_SDK.IggyClient;
using Microsoft.Extensions.Logging.Abstractions;
using IContainer = DotNet.Testcontainers.Containers.IContainer;

namespace Iggy_SDK_Tests.E2ETests.Fixtures.Tcp;

public sealed class IggyTcpFetchMessagesFixture : IAsyncLifetime
{
    private readonly IContainer _container = new ContainerBuilder().WithImage("iggyrs/iggy:latest")
        //.WithPortBinding(3000, true)
        .WithPortBinding(8090, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8090))
        //.WithPortBinding(8080, true)
        .Build();


    public required IIggyClient sut;

    private static readonly StreamRequest StreamRequest = StreamFactory.CreateStreamRequest();
    private static readonly StreamRequest NonExistingStreamRequest = StreamFactory.CreateStreamRequest();
    private static readonly TopicRequest NonExistingTopicRequest = TopicFactory.CreateTopicRequest(3000);
    private static readonly TopicRequest TopicRequest = TopicFactory.CreateTopicRequest();
    private static readonly TopicRequest HeadersTopicRequest = TopicFactory.CreateTopicRequest();

    public readonly int StreamId = StreamRequest.StreamId;
    public readonly int TopicId = TopicRequest.TopicId;
    public readonly int HeadersTopicId = HeadersTopicRequest.TopicId;

    public readonly int InvalidStreamId = NonExistingStreamRequest.StreamId;
    public readonly int InvalidTopicId = NonExistingTopicRequest.TopicId;
    public readonly int PartitionId = 1;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        sut = MessageStreamFactory.CreateMessageStream(options =>
        {
            options.BaseAdress = $"127.0.0.1:{_container.GetMappedPublicPort(8090)}";
            options.Protocol = Protocol.Tcp;
            options.IntervalBatchingConfig = x =>
            {
                x.Interval = TimeSpan.FromMilliseconds(100);
                x.MaxMessagesPerBatch = 1000;
                x.MaxRequests = 8912;
            };
        }, NullLoggerFactory.Instance);
        
        await sut.LoginUser(new LoginUserRequest
        {
            Password = "iggy",
            Username = "iggy"
        });
        await sut.CreateStreamAsync(StreamRequest);
        await sut.CreateTopicAsync(Identifier.Numeric(StreamRequest.StreamId), TopicRequest);
        await sut.CreateTopicAsync(Identifier.Numeric(StreamRequest.StreamId), HeadersTopicRequest);


        var request = MessageFactory.CreateMessageSendRequest(
            StreamRequest.StreamId, TopicRequest.TopicId, PartitionId,
            MessageFactory.GenerateMessages(20));

        var requestWithHeaders = MessageFactory.CreateMessageSendRequest(
            StreamRequest.StreamId, HeadersTopicRequest.TopicId, PartitionId,
            MessageFactory.GenerateMessages(20, MessageFactory.GenerateMessageHeaders(6)));
        await sut.SendMessagesAsync(request);
        await sut.SendMessagesAsync(requestWithHeaders);

        await Task.Delay(1000);
    }

    public async Task DisposeAsync()
    {
        await _container.StopAsync();
    }
}