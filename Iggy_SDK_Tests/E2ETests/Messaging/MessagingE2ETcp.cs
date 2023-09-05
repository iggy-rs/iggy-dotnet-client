using FluentAssertions;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Exceptions;
using Iggy_SDK.Messages;
using Iggy_SDK_Tests.E2ETests.Fixtures.Tcp;
using Iggy_SDK_Tests.Utils;
using Iggy_SDK_Tests.Utils.Messages;

[TestCaseOrderer("Iggy_SDK_Tests.Utils.PriorityOrderer", "Iggy_SDK_Tests")]
public sealed class MessagingE2ETcp : IClassFixture<IggyTcpMessagingFixture>
{
    private readonly IggyTcpMessagingFixture _fixture;
	public readonly MessageSendRequest MessageNoHeadersSendRequest;
	public readonly MessageSendRequest MessageWithHeadersSendRequest;
	public readonly MessageSendRequest InvalidMessageNoHeadersSendRequest;
	public readonly MessageSendRequest InvalidMessageWithHeadersSendRequest;
    private readonly IList<Message> MessagesWithHeaders = MessageFactory.GenerateDummyMessages(
        Random.Shared.Next(20 ,50),
        Random.Shared.Next(69 ,420),
        MessageFactory.GenerateMessageHeaders(Random.Shared.Next(1,20))
    );
    public MessagingE2ETcp(IggyTcpMessagingFixture fixture)
    {
        _fixture =  fixture;
        MessageNoHeadersSendRequest = MessageFactory.CreateMessageSendRequest(_fixture.StreamRequest.StreamId, 
            _fixture.TopicRequest.TopicId, _fixture.PartitionId);
        InvalidMessageNoHeadersSendRequest = MessageFactory.CreateMessageSendRequest(_fixture.NonExistingStreamRequest.StreamId, 
            _fixture.NonExistingTopicRequest.TopicId, _fixture.PartitionId);

        MessageWithHeadersSendRequest = MessageFactory.CreateMessageSendRequest(_fixture.NonExistingStreamRequest.StreamId, 
            _fixture.TopicRequest.TopicId, _fixture.PartitionId, MessagesWithHeaders);
        InvalidMessageWithHeadersSendRequest = MessageFactory.CreateMessageSendRequest(_fixture.NonExistingStreamRequest.StreamId, 
            _fixture.TopicRequest.TopicId, _fixture.PartitionId, MessagesWithHeaders);
    }

    [Fact, TestPriority(1)]
    public async Task SendMessages_NoHeaders_NonGeneric_Should_SendMessages_Successfully()
    {
        await _fixture.sut.Invoking(x => x.SendMessagesAsync(MessageNoHeadersSendRequest))
            .Should()
            .NotThrowAsync();
    }

    [Fact, TestPriority(2)]
    public async Task SendMessages_NoHeaders_NonGeneric_Should_Throw_InvalidResponse()
    {
        await _fixture.sut.Invoking(x => x.SendMessagesAsync(InvalidMessageNoHeadersSendRequest))
            .Should()
            .ThrowAsync<InvalidResponseException>();
    }
    [Fact, TestPriority(3)]
    public async Task SendMessages_WithHeaders_NonGeneric_Should_SendMessages_Successfully()
    {
        await _fixture.sut.Invoking(x => x.SendMessagesAsync(MessageWithHeadersSendRequest))
            .Should()
            .NotThrowAsync();
    }
    [Fact, TestPriority(4)]
    public async Task SendMessages_WithHeaders_NonGeneric_Should_Throw_InvalidResponse()
    {
    }
}