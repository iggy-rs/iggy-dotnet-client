using FluentAssertions;
using Iggy_SDK;
using Iggy_SDK_Tests.Utils;
using Iggy_SDK_Tests.Utils.Streams;
using Iggy_SDK_Tests.Utils.Topics;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;
using Iggy_SDK.Exceptions;
using Iggy_SDK.Factory;
using Iggy_SDK.MessageStream;

namespace Iggy_SDK_Tests.E2ETests.Tcp.Streams;

[TestCaseOrderer("Iggy_SDK_Tests.Utils.PriorityOrderer", "Iggy_SDK_Tests")]
public sealed class TopicsE2E : IClassFixture<IggyTcpFixture>
{
	private IggyTcpFixture _fixture;
	private readonly IMessageStream _sut;
	private static readonly StreamRequest StreamRequest = StreamFactory.CreateStreamRequest();
	private static readonly TopicRequest TopicRequest = TopicFactory.CreateTopicRequest();

	public TopicsE2E(IggyTcpFixture fixture)
	{
		_fixture = fixture;
		_sut = MessageStreamFactory.CreateMessageStream(options =>
		{
			options.BaseAdress = $"127.0.0.1:{_fixture.Container.GetMappedPublicPort(8090)}";
			options.Protocol = Protocol.Tcp;
			options.SendMessagesOptions = x =>
			{
				x.MaxMessagesPerBatch = 1000;
				x.PollingInterval = TimeSpan.FromMilliseconds(100);
			};
		});
	}

	private async Task InitializeStream()
	{
		await _sut.CreateStreamAsync(StreamRequest);
	}

	[Fact, TestPriority(1)]
	public async Task CreateTopic_HappyPath_Should_CreateStream_Successfully()
	{
		//this is hack, I should've created separate fixture for this initialization.
		await InitializeStream();
		await _sut.Invoking(async x => 
			 await x.CreateTopicAsync(Identifier.Numeric(StreamRequest.StreamId), TopicRequest))
			.Should()
			.NotThrowAsync();
	}
	[Fact, TestPriority(2)]
	public async Task CreateTopic_Duplicate_Should_Throw_InvalidResponse()
	{
		await _sut.Invoking(async x =>
				await x.CreateTopicAsync(Identifier.Numeric(StreamRequest.StreamId), TopicRequest))
			.Should()
			.ThrowExactlyAsync<InvalidResponseException>();
	}
	[Fact, TestPriority(3)]
	public async Task GetTopic_Should_ReturnValidResponse()
	{
		var response = await _sut.GetTopicByIdAsync(Identifier.Numeric(StreamRequest.StreamId), Identifier.Numeric(TopicRequest.TopicId));
		response.Should().NotBeNull();
		response!.Id.Should().Be(TopicRequest.TopicId);
		response.Name.Should().Be(TopicRequest.Name);
		response.Partitions.Should().HaveCount(TopicRequest.PartitionsCount);
		response.MessageExpiry.Should().Be(TopicRequest.MessageExpiry);
		response.SizeBytes.Should().Be(0);
		response.MessagesCount.Should().Be(0);
		response.MessageExpiry.Should().Be(TopicRequest.MessageExpiry);
		response.CreatedAt.Year.Should().Be(DateTimeOffset.UtcNow.Year);
		response.CreatedAt.Month.Should().Be(DateTimeOffset.UtcNow.Month);
		response.CreatedAt.Day.Should().Be(DateTimeOffset.UtcNow.Day);
		response.CreatedAt.Minute.Should().Be(DateTimeOffset.UtcNow.Minute);
	}
	
}