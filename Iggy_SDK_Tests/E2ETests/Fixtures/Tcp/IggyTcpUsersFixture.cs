using DotNet.Testcontainers.Builders;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK_Tests.Utils.Users;
using Iggy_SDK.Enums;
using Iggy_SDK.Factory;
using Iggy_SDK.IggyClient;
using Microsoft.Extensions.Logging.Abstractions;
using IContainer = DotNet.Testcontainers.Containers.IContainer;

namespace Iggy_SDK_Tests.E2ETests.Fixtures.Tcp;

public sealed class IggyTcpUsersFixture : IAsyncLifetime
{
    public readonly IContainer Container = new ContainerBuilder().WithImage("iggyrs/iggy:latest")
        //.WithPortBinding(3000, true)
        .WithPortBinding(8090, true)
        .WithOutputConsumer(Consume.RedirectStdoutAndStderrToConsole())
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8090))
        //.WithPortBinding(8080, true)
        .Build();
    public IIggyClient sut;
    public CreateUserRequest UserRequest = UsersFactory.CreateUserRequest("user1", "user1");
    public Permissions UpdatePermissionsRequest = UsersFactory.CreatePermissions();
    public string NewUsername = "new_username";
    public LoginUserRequest LoginRequest = new LoginUserRequest{ Username = "user1", Password = "user1" };
    
    public async Task InitializeAsync()
    {
        await Container.StartAsync();
        sut = MessageStreamFactory.CreateMessageStream(options =>
        {
            options.BaseAdress = $"127.0.0.1:{Container.GetMappedPublicPort(8090)}";
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
    }

    public async Task DisposeAsync()
    {
        await Container.StopAsync();
    }
}