using FluentAssertions;
using Iggy_SDK;
using Iggy_SDK_Tests.Utils;
using Iggy_SDK_Tests.Utils.Streams;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;
using Iggy_SDK.Exceptions;
using Iggy_SDK.Factory;
using Iggy_SDK.MessageStream;

namespace Iggy_SDK_Tests.E2ETests.Tcp.Streams;

[TestCaseOrderer("Iggy_SDK_Tests.Utils.PriorityOrderer", "Iggy_SDK_Tests")]
public sealed class StreamsE2E : IClassFixture<IggyTcpFixture>
{
	private IggyTcpFixture _fixture;
	private readonly IMessageStream _sut;
	private static readonly StreamRequest StreamRequest = StreamFactory.CreateStreamRequest();

	public StreamsE2E(IggyTcpFixture fixture)
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

	[Fact, TestPriority(1)]
	public async Task CreateStream_HappyPath_Should_CreateStream_Successfully()
	{
		await _sut.Invoking(async x => await x.CreateStreamAsync(StreamRequest))
			.Should()
			.NotThrowAsync();
	}
	[Fact, TestPriority(2)]
	public async Task CreateStream_Duplicate_Should_Throw_InvalidResponse()
	{
		await _sut.Invoking(async x => await x.CreateStreamAsync(StreamRequest))
			.Should()
			.ThrowExactlyAsync<InvalidResponseException>()
			.WithMessage("Invalid response status code: 1011");
	}

	[Fact, TestPriority(3)]
	public async Task GetStreamById_Should_ReturnValidResponse()
	{
		var response = await _sut.GetStreamByIdAsync(Identifier.Numeric(StreamRequest.StreamId));
		response.Should().NotBeNull();
		response!.Id.Should().Be(StreamRequest.StreamId);
		response.Name.Should().Be(StreamRequest.Name);
	}
	[Fact, TestPriority(4)]
	public async Task DeleteStream_Should_DeleteStream_Successfully()
	{
		await _sut.Invoking(async x =>
				await x.DeleteStreamAsync(Identifier.Numeric(StreamRequest.StreamId)))
			.Should()
			.NotThrowAsync();
	}
	[Fact, TestPriority(5)]
	public async Task DeleteStream_Should_Throw_InvalidResponse()
	{
		await _sut.Invoking(async x => await x.DeleteStreamAsync(Identifier.Numeric(StreamRequest.StreamId)))
			.Should()
			.ThrowExactlyAsync<InvalidResponseException>()
			.WithMessage("Invalid response status code: 1009");
	}
	[Fact, TestPriority(6)]
	public async Task GetStreamById_Should_Throw_InvalidResponse()
	{
		await _sut.Invoking(async x => 
				await x.GetStreamByIdAsync(Identifier.Numeric(StreamRequest.StreamId)))
			.Should()
			.ThrowExactlyAsync<InvalidResponseException>()
			.WithMessage("Invalid response status code: 1009");
	}
}