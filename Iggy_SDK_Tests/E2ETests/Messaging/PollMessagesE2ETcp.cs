using FluentAssertions;
using Iggy_SDK_Tests.E2ETests.Fixtures.Tcp;
using Iggy_SDK_Tests.Utils;
using Iggy_SDK_Tests.Utils.Messages;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;

namespace Iggy_SDK_Tests.E2ETests.Messaging;

public sealed class PollMessagesE2ETcp : IClassFixture<IggyTcpPollMessagesFixture>
{
    private readonly IggyTcpPollMessagesFixture _fixture;

    private readonly MessageFetchRequest _messageFetchRequest; 
    public PollMessagesE2ETcp(IggyTcpPollMessagesFixture fixture)
    {
        _fixture = fixture;
        _messageFetchRequest =
            MessageFactory.CreateMessageFetchRequest(10, _fixture.StreamId, _fixture.TopicId, _fixture.PartitionId);
    }
    
    [Fact, TestPriority(1)]
    public async Task PollMessages_Should_PollMessages_Successfully()
    {
        var response = await _fixture.sut.PollMessagesAsync(_messageFetchRequest);
        response.Messages.Count.Should().Be(10);
        response.PartitionId.Should().Be(_fixture.PartitionId);
        response.CurrentOffset.Should().Be(49);
        foreach (var responseMessage in response.Messages)
        {
            responseMessage.State.Should().Be(MessageState.Available);
        }
    }
}