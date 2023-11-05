using DotNet.Testcontainers.Builders;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;
using Iggy_SDK.Factory;
using Iggy_SDK_Tests.Utils.Streams;
using Iggy_SDK_Tests.Utils.Topics;
using Iggy_SDK.IggyClient;
using Microsoft.Extensions.Logging.Abstractions;
using IContainer = DotNet.Testcontainers.Containers.IContainer;

namespace Iggy_SDK_Tests.E2ETests.Fixtures.Tcp;

public sealed class IggyTcpClientsFixture : IAsyncLifetime
{
    private readonly IContainer _container = new ContainerBuilder().WithImage("iggyrs/iggy:latest")
        //.WithPortBinding(3000, true)
        .WithPortBinding(8090, true)
        .WithOutputConsumer(Consume.RedirectStdoutAndStderrToConsole())
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8090))
        //.WithPortBinding(8080, true)
        .Build();
    public IIggyClient sut;
    private const int freshClientsCount = 6;
    
    public readonly int TotalClientsCount = freshClientsCount + 1;

    public readonly StreamRequest StreamRequest = StreamFactory.CreateStreamRequest();
    public readonly TopicRequest TopicRequest = TopicFactory.CreateTopicRequest();
    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        sut = MessageStreamFactory.CreateMessageStream(options =>
        {
            options.BaseAdress = $"127.0.0.1:{_container.GetMappedPublicPort(8090)}";
            options.Protocol = Protocol.Tcp;
            options.MessageBatchingSettings = x =>
            {
                x.MaxMessagesPerBatch = 1000;
                x.Interval = TimeSpan.FromMilliseconds(100);
            };
        }, NullLoggerFactory.Instance);
        await sut.LoginUser(new LoginUserRequest
        {
            Password = "iggy",
            Username = "iggy"
        });
        for (int i = 0; i < freshClientsCount; i++)
        {
            MessageStreamFactory.CreateMessageStream(options =>
            {
                options.BaseAdress = $"127.0.0.1:{_container.GetMappedPublicPort(8090)}";
                options.Protocol = Protocol.Tcp;
                options.MessageBatchingSettings = x =>
                {
                    x.MaxMessagesPerBatch = 1000;
                    x.Interval = TimeSpan.FromMilliseconds(100);
                };
            }, NullLoggerFactory.Instance);
        }
    }

    public async Task DisposeAsync()
    {
        await _container.StopAsync();
    }
}