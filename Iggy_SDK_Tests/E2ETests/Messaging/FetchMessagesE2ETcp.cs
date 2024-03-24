using FluentAssertions;
using Iggy_SDK_Tests.E2ETests.Fixtures.Bootstraps;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;
using Iggy_SDK.Exceptions;
using Iggy_SDK_Tests.E2ETests.Fixtures.Tcp;
using Iggy_SDK_Tests.Utils;
using Iggy_SDK_Tests.Utils.Messages;

namespace Iggy_SDK_Tests.E2ETests.Messaging;

[TestCaseOrderer("Iggy_SDK_Tests.Utils.PriorityOrderer", "Iggy_SDK_Tests")]
public sealed class FetchMessagesE2ETcp : IClassFixture<IggyTcpFetchMessagesFixture>
{
    private readonly IggyTcpFetchMessagesFixture _fixture;

    private readonly MessageFetchRequest _messageFetchRequest;
    private readonly MessageFetchRequest _headersMessageFetchRequest;
    private readonly MessageFetchRequest _invalidFetchRequest;
    public FetchMessagesE2ETcp(IggyTcpFetchMessagesFixture fixture)
    {
        _fixture = fixture;
        _messageFetchRequest =
            MessageFactory.CreateMessageFetchRequestConsumer(10, FetchMessagesFixtureBootstrap.StreamId,
                FetchMessagesFixtureBootstrap.TopicId, FetchMessagesFixtureBootstrap.PartitionId);
        _headersMessageFetchRequest =
            MessageFactory.CreateMessageFetchRequestConsumer(10, FetchMessagesFixtureBootstrap.StreamId,
                FetchMessagesFixtureBootstrap.HeadersTopicId, FetchMessagesFixtureBootstrap.PartitionId);
        _invalidFetchRequest =
            MessageFactory.CreateMessageFetchRequestConsumer(10, FetchMessagesFixtureBootstrap.InvalidStreamId,
                FetchMessagesFixtureBootstrap.InvalidTopicId, FetchMessagesFixtureBootstrap.PartitionId);
    }

    [Fact, TestPriority(1)]
    public async Task PollMessagesTMessage_WithNoHeaders_Should_PollMessages_Successfully()
    {
        var tasks = _fixture.SubjectsUnderTest.Select(sut => Task.Run(async () =>
        {
            var response = await sut.FetchMessagesAsync(_messageFetchRequest, MessageFactory.DeserializeDummyMessage);
            response.Messages.Count.Should().Be(10);
            response.PartitionId.Should().Be(FetchMessagesFixtureBootstrap.PartitionId);
            response.CurrentOffset.Should().Be(19);
            foreach (var responseMessage in response.Messages)
            {
                responseMessage.Headers.Should().BeNull();
                responseMessage.State.Should().Be(MessageState.Available);
            }
        })).ToArray();
        await Task.WhenAll(tasks);
    }

    [Fact, TestPriority(2)]
    public async Task PollMessagesTMessage_Should_Throw_InvalidResponse()
    {
        var tasks = _fixture.SubjectsUnderTest.Select(sut => Task.Run(async () =>
        {
            await sut.Invoking(x => x.FetchMessagesAsync(_invalidFetchRequest, MessageFactory.DeserializeDummyMessage))
                .Should()
                .ThrowExactlyAsync<InvalidResponseException>();
        })).ToArray();
        await Task.WhenAll(tasks);
    }

    [Fact, TestPriority(3)]
    public async Task PollMessages_WithNoHeaders_Should_PollMessages_Successfully()
    {
        var tasks = _fixture.SubjectsUnderTest.Select(sut => Task.Run(async () =>
        {
            var response = await sut.FetchMessagesAsync(_messageFetchRequest);
            response.Messages.Count.Should().Be(10);
            response.PartitionId.Should().Be(FetchMessagesFixtureBootstrap.PartitionId);
            response.CurrentOffset.Should().Be(19);
            foreach (var responseMessage in response.Messages)
            {
                responseMessage.Headers.Should().BeNull();
                responseMessage.State.Should().Be(MessageState.Available);
            }
        })).ToArray();
        await Task.WhenAll(tasks);
    }

    [Fact, TestPriority(4)]
    public async Task PollMessages_Should_Throw_InvalidResponse()
    {
        var tasks = _fixture.SubjectsUnderTest.Select(sut => Task.Run(async () =>
        {
            await sut.Invoking(x => x.FetchMessagesAsync(_invalidFetchRequest))
                .Should()
                .ThrowExactlyAsync<InvalidResponseException>();
        })).ToArray();
        await Task.WhenAll(tasks);
    }

    [Fact, TestPriority(5)]
    public async Task PollMessages_WithHeaders_Should_PollMessages_Successfully()
    {
        var tasks = _fixture.SubjectsUnderTest.Select(sut => Task.Run(async () =>
        {
            var response = await sut.FetchMessagesAsync(_headersMessageFetchRequest);
            response.Messages.Count.Should().Be(10);
            response.PartitionId.Should().Be(FetchMessagesFixtureBootstrap.PartitionId);
            response.CurrentOffset.Should().Be(19);
            foreach (var responseMessage in response.Messages)
            {
                responseMessage.Headers.Should().NotBeNull();
                responseMessage.State.Should().Be(MessageState.Available);
                responseMessage.Headers!.Count.Should().Be(6);
            }
        })).ToArray();
        await Task.WhenAll(tasks);
    }

    [Fact, TestPriority(6)]
    public async Task PollMessagesTMessage_WithHeaders_Should_PollMessages_Successfully()
    {
        var tasks = _fixture.SubjectsUnderTest.Select(sut => Task.Run(async () =>
        {
            var response = await sut.FetchMessagesAsync(_headersMessageFetchRequest, MessageFactory.DeserializeDummyMessage);
            response.Messages.Count.Should().Be(10);
            response.PartitionId.Should().Be(FetchMessagesFixtureBootstrap.PartitionId);
            response.CurrentOffset.Should().Be(19);
            foreach (var responseMessage in response.Messages)
            {
                responseMessage.Headers.Should().NotBeNull();
                responseMessage.State.Should().Be(MessageState.Available);
                responseMessage.Headers!.Count.Should().Be(6);
            }
        })).ToArray();
        await Task.WhenAll(tasks);
    }
}