using FluentAssertions;
using Iggy_SDK_Tests.E2ETests.Fixtures;
using Iggy_SDK_Tests.E2ETests.Fixtures.Bootstraps;
using Iggy_SDK_Tests.Utils;
using Iggy_SDK_Tests.Utils.Messages;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Exceptions;

namespace Iggy_SDK_Tests.E2ETests;

[TestCaseOrderer("Iggy_SDK_Tests.Utils.PriorityOrderer", "Iggy_SDK_Tests")]
public sealed class SendMessagesE2E : IClassFixture<IggySendMessagesFixture>
{
    private const string SkipMessage = "TCP implementation needs to be aligned with Iggyrs core changes";
    private readonly IggySendMessagesFixture _fixture;

    private readonly MessageSendRequest _messageNoHeadersSendRequest;
    private readonly MessageSendRequest _messageWithHeadersSendRequest;
    private readonly MessageSendRequest _invalidMessageNoHeadersSendRequest;
    private readonly MessageSendRequest _invalidMessageWithHeadersSendRequest;

    public SendMessagesE2E(IggySendMessagesFixture fixture)
    {
        _fixture = fixture;

        var messageWithHeaders = MessageFactory.GenerateDummyMessages(
            Random.Shared.Next(20, 50),
            Random.Shared.Next(69, 420),
            MessageFactory.GenerateMessageHeaders(Random.Shared.Next(1, 20)));

        _messageNoHeadersSendRequest = MessageFactory.CreateMessageSendRequest(SendMessagesFixtureBootstrap.StreamId,
            SendMessagesFixtureBootstrap.TopicId, SendMessagesFixtureBootstrap.PartitionId);
        _invalidMessageNoHeadersSendRequest = MessageFactory.CreateMessageSendRequest(SendMessagesFixtureBootstrap.InvalidStreamId,
            SendMessagesFixtureBootstrap.InvalidTopicId, SendMessagesFixtureBootstrap.PartitionId);

        _messageWithHeadersSendRequest = MessageFactory.CreateMessageSendRequest(SendMessagesFixtureBootstrap.StreamId,
            SendMessagesFixtureBootstrap.TopicId, SendMessagesFixtureBootstrap.PartitionId, messageWithHeaders);
        _invalidMessageWithHeadersSendRequest = MessageFactory.CreateMessageSendRequest(SendMessagesFixtureBootstrap.InvalidStreamId,
            SendMessagesFixtureBootstrap.InvalidTopicId, SendMessagesFixtureBootstrap.PartitionId, messageWithHeaders);
    }

    [Fact(Skip = SkipMessage), TestPriority(1)]
    public async Task SendMessages_NoHeaders_Should_SendMessages_Successfully()
    {
        var tasks = _fixture.SubjectsUnderTest.Select(sut => Task.Run(async () =>
        {
            await sut.Invoking(x => x.SendMessagesAsync(_messageNoHeadersSendRequest))
                .Should()
                .NotThrowAsync();
        })).ToArray();
        await Task.WhenAll(tasks);
    }

    [Fact(Skip = SkipMessage), TestPriority(2)]
    public async Task SendMessages_NoHeaders_Should_Throw_InvalidResponse()
    {
        var tasks = _fixture.SubjectsUnderTest.Select(sut => Task.Run(async () =>
        {
            await sut.Invoking(x => x.SendMessagesAsync(_invalidMessageNoHeadersSendRequest))
                .Should()
                .ThrowAsync<InvalidResponseException>();
        })).ToArray();
        await Task.WhenAll(tasks);
    }

    [Fact(Skip = SkipMessage), TestPriority(3)]
    public async Task SendMessages_WithHeaders_Should_SendMessages_Successfully()
    {
        var tasks = _fixture.SubjectsUnderTest.Select(sut => Task.Run(async () =>
        {
            await sut.Invoking(x => x.SendMessagesAsync(_messageWithHeadersSendRequest))
                .Should()
                .NotThrowAsync();
        })).ToArray();
        await Task.WhenAll(tasks);
    }

    [Fact(Skip = SkipMessage), TestPriority(4)]
    public async Task SendMessages_WithHeaders_Should_Throw_InvalidResponse()
    {
        var tasks = _fixture.SubjectsUnderTest.Select(sut => Task.Run(async () =>
        {
            await sut.Invoking(x => x.SendMessagesAsync(_invalidMessageWithHeadersSendRequest))
                .Should()
                .ThrowAsync<InvalidResponseException>();
        })).ToArray();
        await Task.WhenAll(tasks);
    }
}