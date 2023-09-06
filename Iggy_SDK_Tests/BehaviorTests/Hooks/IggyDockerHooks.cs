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

namespace Iggy_SDK_Tests.BehaviorTests.Hooks;

[Binding]
public sealed class IggyDockerHooks
{
    private readonly IContainer _container = new ContainerBuilder().WithImage("iggyrs/iggy:latest")
		//.WithPortBinding(3000, true)
		.WithPortBinding(8090, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8090))
		//.WithPortBinding(8080, true)
		.Build();

    private IMessageStream _sut;
    
    private static readonly StreamRequest _streamRequest = StreamFactory.CreateStreamRequest();
    private static readonly TopicRequest _topicConsumersRequest = TopicFactory.CreateTopicRequest(); 
    private static readonly TopicRequest _topicConsumerGroupRequest = TopicFactory.CreateTopicRequest();

    private static readonly int _partitionId = 1;

    private static readonly IList<Message> _messages = MessageFactory.GenerateDummyMessages(10);

    private readonly MessageSendRequest _messageConsumersSendRequest =
        MessageFactory.CreateMessageSendRequest(_streamRequest.StreamId, _topicConsumersRequest.TopicId, 
            _partitionId, _messages);

    private readonly MessageSendRequest _messageConsumerGroupSendRequest =
        MessageFactory.CreateMessageSendRequest(_streamRequest.StreamId, _topicConsumerGroupRequest.TopicId,
            _partitionId, _messages);
    
    public static int StreamId = _streamRequest.StreamId;
    public static int TopicId = _topicConsumersRequest.TopicId;
    public static int HeadersTopicId = _topicConsumerGroupRequest.TopicId;

    [BeforeTestRun]
    public async Task IntialSetup()
    {
        await _container.StartAsync();
		_sut = MessageStreamFactory.CreateMessageStream(options =>
		{
			options.BaseAdress = $"127.0.0.1:{_container.GetMappedPublicPort(8090)}";
			options.Protocol = Protocol.Tcp;
			options.SendMessagesOptions = x =>
			{
				x.MaxMessagesPerBatch = 1000;
				x.PollingInterval = TimeSpan.FromMilliseconds(100);
			};
		});
        await _sut.CreateStreamAsync(_streamRequest);
        await _sut.CreateTopicAsync(Identifier.Numeric(_streamRequest.StreamId), _topicConsumersRequest);
        await _sut.CreateTopicAsync(Identifier.Numeric(_streamRequest.StreamId), _topicConsumerGroupRequest);
        
        await _sut.SendMessagesAsync(_messageConsumersSendRequest);
        await _sut.SendMessagesAsync(_messageConsumerGroupSendRequest);
    }

    [AfterTestRun]
    public async Task Teardown()
    {
        await _container.StopAsync();
    }
}