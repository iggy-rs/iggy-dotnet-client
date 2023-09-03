using DotNet.Testcontainers.Builders;
using IContainer = DotNet.Testcontainers.Containers.IContainer;

namespace Iggy_SDK_Tests.E2ETests.Tcp;

public sealed class IggyTcpFixture : IAsyncLifetime
{
	public readonly IContainer Container = new ContainerBuilder().WithImage("iggyrs/iggy:latest")
		//.WithPortBinding(3000, true)
		.WithPortBinding(8090, true)
		//.WithPortBinding(8080, true)
		.Build();

	public async Task InitializeAsync()
	{
		await Container.StartAsync();
	}

	public async Task DisposeAsync()
	{
		await Container.StopAsync();
	}
}