using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Iggy_SDK;
using Iggy_SDK_Tests.Utils.Messages;
using Iggy_SDK_Tests.Utils.Streams;
using Iggy_SDK_Tests.Utils.Topics;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;
using Iggy_SDK.Factory;
using Iggy_SDK.Messages;
using Iggy_SDK.MessageStream;
using TechTalk.SpecFlow;
using BoDi;

namespace Iggy_SDK_Tests.BehaviorTests.Hooks;

[Binding]
public sealed class IggyDockerHooks
{
    private static readonly IContainer _container = new ContainerBuilder().WithImage("iggyrs/iggy:latest")
		//.WithPortBinding(3000, true)
		.WithPortBinding(8090, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8090))
		//.WithPortBinding(8080, true)
		.Build();

    private static readonly StreamRequest _streamRequest = StreamFactory.CreateStreamRequest();
    private static readonly TopicRequest _topicConsumersRequest = TopicFactory.CreateTopicRequest(); 
    private static readonly TopicRequest _topicConsumerGroupRequest = TopicFactory.CreateTopicRequest();

    private static readonly int _partitionId = 1;

    private static readonly IList<Message> _messages = MessageFactory.GenerateDummyMessages(10);

    private static readonly MessageSendRequest _messageConsumersSendRequest =
        MessageFactory.CreateMessageSendRequest(_streamRequest.StreamId, _topicConsumersRequest.TopicId, 
            _partitionId, _messages);

    private static readonly MessageSendRequest _messageConsumerGroupSendRequest =
        MessageFactory.CreateMessageSendRequest(_streamRequest.StreamId, _topicConsumerGroupRequest.TopicId,
            _partitionId, _messages);
    

    private readonly IObjectContainer _dependencyContainer; 


    public IggyDockerHooks(IObjectContainer dependencyContainer)
    {
        _dependencyContainer = dependencyContainer;
    }

    [BeforeScenario]
    public void RegisterDependencies()
    {
		var messageBus = MessageStreamFactory.CreateMessageStream(options =>
		{
			options.BaseAdress = $"127.0.0.1:{_container.GetMappedPublicPort(8090)}";
			options.Protocol = Protocol.Tcp;
            options.SendBufferSize = 10000;
            options.ReceiveBufferSize = 10000;
			options.SendMessagesOptions = x =>
			{
				x.MaxMessagesPerBatch = 1000;
				x.PollingInterval = TimeSpan.FromMilliseconds(100);
			}; });
       _dependencyContainer.RegisterInstanceAs<IMessageStream>(messageBus);
        var listOfIds = new int[3]{_streamRequest.StreamId, _topicConsumersRequest.TopicId, _topicConsumerGroupRequest.TopicId};
       _dependencyContainer.RegisterInstanceAs<int[]>(listOfIds);

    }

    [BeforeTestRun]
    public static async Task IntialSetup()
    {
        await _container.StartAsync();
		var messageBus = MessageStreamFactory.CreateMessageStream(options =>
		{
			options.BaseAdress = $"127.0.0.1:{_container.GetMappedPublicPort(8090)}";
			options.Protocol = Protocol.Tcp;
            options.SendBufferSize = 10000;
            options.ReceiveBufferSize = 10000;
			options.SendMessagesOptions = x =>
			{
				x.MaxMessagesPerBatch = 1000;
				x.PollingInterval = TimeSpan.FromMilliseconds(100);
			};
		});

        await messageBus.CreateStreamAsync(_streamRequest);
        await messageBus.CreateTopicAsync(Identifier.Numeric(_streamRequest.StreamId), _topicConsumersRequest);
        await messageBus.CreateTopicAsync(Identifier.Numeric(_streamRequest.StreamId), _topicConsumerGroupRequest);
        
        await messageBus.SendMessagesAsync(_messageConsumersSendRequest);
        await messageBus.SendMessagesAsync(_messageConsumerGroupSendRequest);
    }

    [AfterTestRun]
    public static async Task Teardown()
    {
        await _container.StopAsync();
    }
}