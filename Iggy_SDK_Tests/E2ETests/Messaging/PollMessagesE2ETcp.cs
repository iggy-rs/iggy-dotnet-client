using FluentAssertions;
using Iggy_SDK;
using Iggy_SDK_Tests.E2ETests.Fixtures.Tcp;
using Iggy_SDK_Tests.Utils;
using Iggy_SDK_Tests.Utils.Messages;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;
using Iggy_SDK.Kinds;
namespace Iggy_SDK_Tests.E2ETests.Messaging;

public sealed class PollMessagesE2ETcp : IClassFixture<IggyTcpPollMessagesFixture>
{
    private readonly IggyTcpPollMessagesFixture _fixture;
    public PollMessagesE2ETcp(IggyTcpPollMessagesFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact, TestPriority(1)]
    public async Task PollMessagesTMessage_WithNoHeaders_Should_PollMessages_Successfully()
    {
        _ = Task.Run(async () =>
        {
            int i = 0;
            await foreach (var msgResponse in _fixture.sut.PollMessagesAsync(new PollMessagesRequest
                           {
                               Consumer = Consumer.New(1),
                               Count = 10,
                               Interval = TimeSpan.FromMilliseconds(100),
                               PartitionId = _fixture.PartitionId,
                               PollingStrategy = PollingStrategy.Next(),
                               StreamId = Identifier.Numeric(_fixture.StreamId),
                               TopicId = Identifier.Numeric(_fixture.TopicId),
                               StoreOffsetStragety = StoreOffset.WhenMessagesAreReceived
                           }, MessageFactory.DeserializeDummyMessage))
            {
                msgResponse.Headers.Should().BeNull();
                msgResponse.State.Should().Be(MessageState.Available);
                i++;
            }
            i.Should().Be(IggyTcpPollMessagesFixture.MessageCount);
        });
    }
    
    [Fact, TestPriority(2)]
    public async Task PollMessagesTMessage_NoHeaders_Should_PollMessages_Successfully()
    {
        _ = Task.Run(async () =>
        {
            int i = 0;
            await foreach (var msgResponse in _fixture.sut.PollMessagesAsync(new PollMessagesRequest
                               {
                               Consumer = Consumer.New(1),
                               Count = 10,
                               Interval = TimeSpan.FromMilliseconds(100),
                               PartitionId = _fixture.PartitionId,
                               PollingStrategy = PollingStrategy.Next(),
                               StreamId = Identifier.Numeric(_fixture.StreamId),
                               TopicId = Identifier.Numeric(_fixture.HeadersTopicId),
                               StoreOffsetStragety = StoreOffset.WhenMessagesAreReceived
                           }, MessageFactory.DeserializeDummyMessage))
            {
                msgResponse.Headers.Should().NotBeNull();
                msgResponse.Headers.Should().HaveCount(_fixture.HeadersCount);
                msgResponse.State.Should().Be(MessageState.Available);
                i++;
            }
            i.Should().Be(IggyTcpPollMessagesFixture.MessageCount);
        });
    }
}