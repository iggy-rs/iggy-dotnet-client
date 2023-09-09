using FluentAssertions;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Exceptions;
using Iggy_SDK_Tests.E2ETests.Fixtures.Tcp;
using Iggy_SDK_Tests.Utils;
using Iggy_SDK_Tests.Utils.Messages;

namespace Iggy_SDK_Tests.E2ETests.Messaging;

[TestCaseOrderer("Iggy_SDK_Tests.Utils.PriorityOrderer", "Iggy_SDK_Tests")]
public sealed class SendMessagesE2ETcp : IClassFixture<IggyTcpSendMessagesFixture>
{
    private readonly IggyTcpSendMessagesFixture _fixture;

    private readonly MessageSendRequest _messageNoHeadersSendRequest;
    private readonly MessageSendRequest _messageWithHeadersSendRequest;
    private readonly MessageSendRequest _invalidMessageNoHeadersSendRequest;
    private readonly MessageSendRequest _invalidMessageWithHeadersSendRequest;

    public SendMessagesE2ETcp(IggyTcpSendMessagesFixture fixture)
    {
        _fixture = fixture;

        var messageWithHeaders = MessageFactory.GenerateDummyMessages(
            Random.Shared.Next(20, 50),
            Random.Shared.Next(69, 420),
            MessageFactory.GenerateMessageHeaders(Random.Shared.Next(1, 20)));

        _messageNoHeadersSendRequest = MessageFactory.CreateMessageSendRequest(_fixture.StreamId,
            _fixture.TopicId, _fixture.PartitionId);
        _invalidMessageNoHeadersSendRequest = MessageFactory.CreateMessageSendRequest(_fixture.InvalidStreamId,
            _fixture.InvalidTopicId, _fixture.PartitionId);

        _messageWithHeadersSendRequest = MessageFactory.CreateMessageSendRequest(_fixture.StreamId,
            _fixture.TopicId, _fixture.PartitionId, messageWithHeaders);
        _invalidMessageWithHeadersSendRequest = MessageFactory.CreateMessageSendRequest(_fixture.InvalidStreamId,
            _fixture.InvalidTopicId, _fixture.PartitionId, messageWithHeaders);

    }

    [Fact, TestPriority(1)]
    public async Task SendMessages_NoHeaders_Should_SendMessages_Successfully()
    {
        await _fixture.sut.Invoking(x => x.SendMessagesAsync(_messageNoHeadersSendRequest))
            .Should()
            .NotThrowAsync();
    }

    [Fact, TestPriority(2)]
    public async Task SendMessages_NoHeaders_Should_Throw_InvalidResponse()
    {
        await _fixture.sut.Invoking(x => x.SendMessagesAsync(_invalidMessageNoHeadersSendRequest))
            .Should()
            .ThrowAsync<InvalidResponseException>();
    }

    [Fact, TestPriority(3)]
    public async Task SendMessages_WithHeaders_Should_SendMessages_Successfully()
    {
        await _fixture.sut.Invoking(x => x.SendMessagesAsync(_messageWithHeadersSendRequest))
            .Should()
            .NotThrowAsync();
    }

    [Fact, TestPriority(4)]
    public async Task SendMessages_WithHeaders_Should_Throw_InvalidResponse()
    {
        await _fixture.sut.Invoking(x => x.SendMessagesAsync(_invalidMessageWithHeadersSendRequest))
            .Should()
            .ThrowAsync<InvalidResponseException>();
    }
}