using DotNet.Testcontainers.Builders;
using Iggy_SDK;
using Iggy_SDK_Tests.Utils.Streams;
using Iggy_SDK_Tests.Utils.Topics;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;
using Iggy_SDK.Factory;
using Iggy_SDK.MessageStream;
using IContainer = DotNet.Testcontainers.Containers.IContainer;
using Iggy_SDK.Kinds;
using Iggy_SDK_Tests.Utils.Messages;

namespace Iggy_SDK_Tests.E2ETests.Fixtures.Tcp;

public sealed class IggyTcpMessagingFixture : IAsyncLifetime
{
	public readonly IContainer Container = new ContainerBuilder().WithImage("iggyrs/iggy:latest")
		//.WithPortBinding(3000, true)
		.WithPortBinding(8090, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8090))
		//.WithPortBinding(8080, true)
		.Build();
	public IMessageStream sut;

	public readonly StreamRequest StreamRequest = StreamFactory.CreateStreamRequest();
	public readonly StreamRequest NonExistingStreamRequest = StreamFactory.CreateStreamRequest();
	public readonly TopicRequest NonExistingTopicRequest = TopicFactory.CreateTopicRequest();
	public readonly TopicRequest TopicRequest = TopicFactory.CreateTopicRequest();
	public readonly int PartitionId = 1;
	public async Task InitializeAsync()
	{
		await Container.StartAsync();
		sut = MessageStreamFactory.CreateMessageStream(options =>
		{
			options.BaseAdress = $"127.0.0.1:{Container.GetMappedPublicPort(8090)}";
			options.Protocol = Protocol.Tcp;
			options.SendMessagesOptions = x =>
			{
				x.MaxMessagesPerBatch = 1000;
				x.PollingInterval = TimeSpan.FromMilliseconds(100);
			};
		});
        await sut.CreateStreamAsync(StreamRequest);
        await sut.CreateTopicAsync(Identifier.Numeric(StreamRequest.StreamId), TopicRequest);
    }

	public async Task DisposeAsync()
	{
		await Container.StopAsync();
	}
}