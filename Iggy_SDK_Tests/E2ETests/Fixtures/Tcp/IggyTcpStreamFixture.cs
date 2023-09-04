using DotNet.Testcontainers.Builders;
using Iggy_SDK_Tests.Utils.Streams;
using Iggy_SDK.Contracts.Http;
using IContainer = DotNet.Testcontainers.Containers.IContainer;

namespace Iggy_SDK_Tests.E2ETests.Fixtures.Tcp;

public sealed class IggyTcpStreamFixture : IAsyncLifetime
{
	public readonly IContainer Container = new ContainerBuilder().WithImage("iggyrs/iggy:latest")
		//.WithPortBinding(3000, true)
		.WithPortBinding(8090, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8090))
		//.WithPortBinding(8080, true)
		.Build();

	public readonly StreamRequest StreamRequest = StreamFactory.CreateStreamRequest();
	public async Task InitializeAsync()
	{
		await Container.StartAsync();
	}

	public async Task DisposeAsync()
	{
		await Container.StopAsync();
	}
}