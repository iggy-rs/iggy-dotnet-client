using FluentAssertions;
using Iggy_SDK;
using Iggy_SDK_Tests.E2ETests.Fixtures.Tcp;
using Iggy_SDK_Tests.Utils;
using Iggy_SDK.Exceptions;

namespace Iggy_SDK_Tests.E2ETests.Topics;

[TestCaseOrderer("Iggy_SDK_Tests.Utils.PriorityOrderer", "Iggy_SDK_Tests")]
public sealed class TopicsE2ETcp : IClassFixture<IggyTcpTopicFixture>
{
	private readonly IggyTcpTopicFixture _fixture;
	public TopicsE2ETcp(IggyTcpTopicFixture fixture)
	{
		_fixture = fixture;
    }

	[Fact, TestPriority(1)]
	public async Task CreateTopic_HappyPath_Should_CreateTopic_Successfully()
	{
		await _fixture.sut.Invoking(async x => 
			 await x.CreateTopicAsync(Identifier.Numeric(_fixture.StreamRequest.StreamId), _fixture.TopicRequest))
			.Should()
			.NotThrowAsync();
	}
    
	[Fact, TestPriority(2)]
	public async Task CreateTopic_Duplicate_Should_Throw_InvalidResponse()
	{
		await _fixture.sut.Invoking(async x =>
				await x.CreateTopicAsync(Identifier.Numeric(_fixture.StreamRequest.StreamId), _fixture.TopicRequest))
			.Should()
			.ThrowExactlyAsync<InvalidResponseException>();
	}
    
	[Fact, TestPriority(3)]
	public async Task GetTopic_Should_ReturnValidResponse()
	{
		var response = await _fixture.sut.GetTopicByIdAsync(Identifier.Numeric(_fixture.StreamRequest.StreamId), Identifier.Numeric(_fixture.TopicRequest.TopicId));
		response.Should().NotBeNull();
		response!.Id.Should().Be(_fixture.TopicRequest.TopicId);
		response.Name.Should().Be(_fixture.TopicRequest.Name);
		response.Partitions.Should().HaveCount(_fixture.TopicRequest.PartitionsCount);
		response.MessageExpiry.Should().Be(_fixture.TopicRequest.MessageExpiry);
		response.SizeBytes.Should().Be(0);
		response.MessagesCount.Should().Be(0);
		response.MessageExpiry.Should().Be(_fixture.TopicRequest.MessageExpiry);
		response.CreatedAt.Year.Should().Be(DateTimeOffset.UtcNow.Year);
		response.CreatedAt.Month.Should().Be(DateTimeOffset.UtcNow.Month);
		response.CreatedAt.Day.Should().Be(DateTimeOffset.UtcNow.Day);
		response.CreatedAt.Minute.Should().Be(DateTimeOffset.UtcNow.Minute);
	}
    
    [Fact, TestPriority(4)]
    public async Task DeleteTopic_Should_DeleteTopic_Successfully()
    {
		await _fixture.sut.Invoking(async x =>
				await x.DeleteTopicAsync(Identifier.Numeric(_fixture.StreamRequest.StreamId),
                    Identifier.Numeric(_fixture.TopicRequest.TopicId)))
			.Should()
			.NotThrowAsync();
    }
    
    [Fact, TestPriority(5)]
    public async Task DeleteTopic_Should_Throw_InvalidResponse()
    {
        await _fixture.sut.Invoking(async x =>
                await x.DeleteTopicAsync(Identifier.Numeric(_fixture.StreamRequest.StreamId),
                    Identifier.Numeric(_fixture.TopicRequest.TopicId)))
            .Should()
            .ThrowExactlyAsync<InvalidResponseException>();
    }
    
	[Fact, TestPriority(6)]
	public async Task GetTopic_Should_Throw_InvalidResponse()
	{
        await _fixture.sut.Invoking(async x =>
                await x.GetTopicByIdAsync(Identifier.Numeric(_fixture.StreamRequest.StreamId),
                    Identifier.Numeric(_fixture.TopicRequest.TopicId)))
            .Should()
            .ThrowExactlyAsync<InvalidResponseException>();
	}
}