using DotNet.Testcontainers.Builders;
using Iggy_SDK;
using Iggy_SDK_Tests.Utils.Streams;
using Iggy_SDK_Tests.Utils.Topics;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;
using Iggy_SDK.Factory;
using Iggy_SDK.MessageStream;
using IContainer = DotNet.Testcontainers.Containers.IContainer;
using Iggy_SDK_Tests.Utils.Messages;
using Iggy_SDK.Messages;

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
	private static readonly StreamRequest NonExistingStreamRequest = StreamFactory.CreateStreamRequest();
	private static readonly TopicRequest NonExistingTopicRequest = TopicFactory.CreateTopicRequest();
	private static readonly TopicRequest TopicRequest = TopicFactory.CreateTopicRequest();

    public readonly int StreamId = StreamRequest.StreamId;
    public readonly int TopicId = TopicRequest.TopicId;
    
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
			options.SendMessagesOptions = x =>
			{
				x.MaxMessagesPerBatch = 1000;
				x.PollingInterval = TimeSpan.FromMilliseconds(100);
			};
		});
        
        await sut.CreateStreamAsync(StreamRequest);
        await sut.CreateTopicAsync(Identifier.Numeric(StreamRequest.StreamId), TopicRequest);


        var request = MessageFactory.CreateMessageSendRequest(
            StreamRequest.StreamId, TopicRequest.TopicId, PartitionId,
			MessageFactory.GenerateDummyMessages(20));

        var requestWithHeaders = MessageFactory.CreateMessageSendRequest(
            StreamRequest.StreamId, TopicRequest.TopicId, PartitionId,
			MessageFactory.GenerateDummyMessages(20, MessageFactory.GenerateMessageHeaders(6)));
        await sut.SendMessagesAsync(request);
		await sut.SendMessagesAsync(requestWithHeaders);

        await Task.Delay(200);
    }

	public async Task DisposeAsync()
	{
		await _container.StopAsync();
	}
}