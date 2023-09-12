using FluentAssertions;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;
using Iggy_SDK.Exceptions;
using Iggy_SDK_Tests.E2ETests.Fixtures.Tcp;
using Iggy_SDK_Tests.Utils;
using Iggy_SDK_Tests.Utils.DummyObj;
using Iggy_SDK_Tests.Utils.Messages;

namespace Iggy_SDK_Tests.E2ETests.Messaging;

public sealed class PollMessagesE2ETcp : IClassFixture<IggyTcpPollMessagesFixture>
{
    private readonly IggyTcpPollMessagesFixture _fixture;

    private readonly MessageFetchRequest _messageFetchRequest;
    private readonly MessageFetchRequest _headersMessageFetchRequest;
    private readonly MessageFetchRequest _invalidFetchRequest;
    public PollMessagesE2ETcp(IggyTcpPollMessagesFixture fixture)
    {
        _fixture = fixture;
        _messageFetchRequest =
            MessageFactory.CreateMessageFetchRequestConsumer(10, _fixture.StreamId, _fixture.TopicId, _fixture.PartitionId);
        _headersMessageFetchRequest =
            MessageFactory.CreateMessageFetchRequestConsumer(10, _fixture.StreamId, _fixture.HeadersTopicId, _fixture.PartitionId);
        _invalidFetchRequest =
            MessageFactory.CreateMessageFetchRequestConsumer(10, _fixture.InvalidStreamId, _fixture.InvalidTopicId, _fixture.PartitionId);
    }

    [Fact, TestPriority(1)]
    public async Task PollMessagesTMessage_WithNoHeaders_Should_PollMessages_Successfully()
    {
        var response = await _fixture.sut.FetchMessagesAsync<DummyMessage>(_messageFetchRequest, MessageFactory.DeserializeDummyMessage);
        response.Messages.Count.Should().Be(10);
        response.PartitionId.Should().Be(_fixture.PartitionId);
        response.CurrentOffset.Should().Be(19);
        foreach (var responseMessage in response.Messages)
        {
            responseMessage.Headers.Should().BeNull();
            responseMessage.State.Should().Be(MessageState.Available);
        }
    }

    [Fact, TestPriority(2)]
    public async Task PollMessagesTMessage_Should_Throw_InvalidResponse()
    {
        await _fixture.sut.Invoking(x => x.FetchMessagesAsync(_invalidFetchRequest, MessageFactory.DeserializeDummyMessage))
            .Should()
            .ThrowExactlyAsync<InvalidResponseException>();
    }

    [Fact, TestPriority(3)]
    public async Task PollMessages_WithNoHeaders_Should_PollMessages_Successfully()
    {
        var response = await _fixture.sut.FetchMessagesAsync(_messageFetchRequest);
        response.Messages.Count.Should().Be(10);
        response.PartitionId.Should().Be(_fixture.PartitionId);
        response.CurrentOffset.Should().Be(19);
        foreach (var responseMessage in response.Messages)
        {
            responseMessage.Headers.Should().BeNull();
            responseMessage.State.Should().Be(MessageState.Available);
        }
    }

    [Fact, TestPriority(4)]
    public async Task PollMessages_Should_Throw_InvalidResponse()
    {
        await _fixture.sut.Invoking(x => x.FetchMessagesAsync(_invalidFetchRequest))
            .Should()
            .ThrowExactlyAsync<InvalidResponseException>();
    }

    [Fact, TestPriority(5)]
    public async Task PollMessages_WithHeaders_Should_PollMessages_Successfully()
    {
        var response = await _fixture.sut.FetchMessagesAsync(_headersMessageFetchRequest);
        response.Messages.Count.Should().Be(10);
        response.PartitionId.Should().Be(_fixture.PartitionId);
        response.CurrentOffset.Should().Be(19);
        foreach (var responseMessage in response.Messages)
        {
            responseMessage.Headers.Should().NotBeNull();
            responseMessage.State.Should().Be(MessageState.Available);
            responseMessage.Headers!.Count.Should().Be(6);
        }
    }

    [Fact, TestPriority(6)]
    public async Task PollMessagesTMessage_WithHeaders_Should_PollMessages_Successfully()
    {
        var response = await _fixture.sut.FetchMessagesAsync(_headersMessageFetchRequest, MessageFactory.DeserializeDummyMessage);
        response.Messages.Count.Should().Be(10);
        response.PartitionId.Should().Be(_fixture.PartitionId);
        response.CurrentOffset.Should().Be(19);
        foreach (var responseMessage in response.Messages)
        {
            responseMessage.Headers.Should().NotBeNull();
            responseMessage.State.Should().Be(MessageState.Available);
            responseMessage.Headers!.Count.Should().Be(6);
        }
    }
}