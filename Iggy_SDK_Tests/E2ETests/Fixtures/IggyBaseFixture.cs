using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Iggy_SDK.Configuration;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;
using Iggy_SDK.Factory;
using Iggy_SDK.IggyClient;
using Microsoft.Extensions.Logging.Abstractions;

namespace Iggy_SDK_Tests.E2ETests.Fixtures;

public abstract class IggyBaseFixture : IAsyncLifetime
{
    private readonly IIggyBootstrap _bootstraper;
    private readonly Action<MessagePollingSettings> _pollingSettings;
    private readonly Action<MessageBatchingSettings> _batchingSettings;

    private readonly IContainer _tcpContainer = new ContainerBuilder().WithImage("iggyrs/iggy:latest")
        .WithPortBinding(8090, true)
        .WithOutputConsumer(Consume.RedirectStdoutAndStderrToConsole())
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8090))
        .Build();
    private readonly IContainer _httpContainer = new ContainerBuilder().WithImage("iggyrs/iggy:latest")
        .WithPortBinding(3000, true)
        .WithOutputConsumer(Consume.RedirectStdoutAndStderrToConsole())
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(3000))
        .Build();
    

    public IIggyClient[] SubjectsUnderTest { get; } = new IIggyClient[2];
    protected IggyBaseFixture(IIggyBootstrap bootstraper, Action<MessagePollingSettings> pollingSettings,
        Action<MessageBatchingSettings> batchingSettings)
    {
        _bootstraper = bootstraper;
        _pollingSettings = pollingSettings;
        _batchingSettings = batchingSettings;
    }

    public async Task InitializeAsync()
    {
        await _tcpContainer.StartAsync();
        var firstSubject = MessageStreamFactory.CreateMessageStream(options =>
        {
            options.BaseAdress = $"127.0.0.1:{_tcpContainer.GetMappedPublicPort(8090)}";
            options.Protocol = Protocol.Tcp;
            options.MessageBatchingSettings = _batchingSettings;
            options.MessagePollingSettings = _pollingSettings;
        }, NullLoggerFactory.Instance);
        await firstSubject.LoginUser(new LoginUserRequest
        {
            Password = "iggy",
            Username = "iggy"
        });
        SubjectsUnderTest[0] = firstSubject;
        
        await _httpContainer.StartAsync();
        var secondSubject = (MessageStreamFactory.CreateMessageStream(options =>
        {
            options.BaseAdress = $"127.0.0.1:{_httpContainer.GetMappedPublicPort(8090)}";
            options.Protocol = Protocol.Http;
            options.MessageBatchingSettings = _batchingSettings;
            options.MessagePollingSettings = _pollingSettings;
        }, NullLoggerFactory.Instance));
        await secondSubject.LoginUser(new LoginUserRequest
        {
            Password = "iggy",
            Username = "iggy"
        });
        SubjectsUnderTest[1] = secondSubject;
        await _bootstraper.BootstrapResourcesAsync();
    }

    public async Task DisposeAsync()
    {
        await _tcpContainer.StopAsync();
        await _httpContainer.StopAsync();
    }
}