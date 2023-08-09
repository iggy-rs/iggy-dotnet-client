using DotNet.Testcontainers.Builders;
using IContainer = DotNet.Testcontainers.Containers.IContainer;

namespace Iggy_SDK_Tests.IntegrationTests;


public sealed class IggyFixture : IAsyncLifetime
{
	private readonly IContainer _container = new ContainerBuilder().WithImage("iggyrs/iggy:latest").Build();
	
	public async Task InitializeAsync()
	{
		await _container.StartAsync();
	}

	public async Task DisposeAsync()
	{
		await _container.StopAsync();
	}
}