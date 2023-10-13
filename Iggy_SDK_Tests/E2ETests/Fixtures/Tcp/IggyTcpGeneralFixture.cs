using DotNet.Testcontainers.Builders;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;
using Iggy_SDK.Factory;
using Iggy_SDK.IggyClient;
using Microsoft.Extensions.Logging.Abstractions;
using IContainer = DotNet.Testcontainers.Containers.IContainer;

namespace Iggy_SDK_Tests.E2ETests.Fixtures.Tcp;

public sealed class IggyTcpGeneralFixture : IAsyncLifetime
{
    private readonly IContainer _container = new ContainerBuilder().WithImage("iggyrs/iggy:latest")
        //.WithPortBinding(3000, true)
        .WithPortBinding(8090, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8090))
        //.WithPortBinding(8080, true)
        .Build();
    public IIggyClient sut;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        sut = MessageStreamFactory.CreateMessageStream(options =>
        {
            options.BaseAdress = $"127.0.0.1:{_container.GetMappedPublicPort(8090)}";
            options.Protocol = Protocol.Tcp;
            options.IntervalBatchingConfig = x =>
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
    }

    public async Task DisposeAsync()
    {
        await _container.StopAsync();
    }
}