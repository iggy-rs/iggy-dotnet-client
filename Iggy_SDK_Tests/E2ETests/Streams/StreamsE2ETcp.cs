using FluentAssertions;
using Iggy_SDK;
using Iggy_SDK.Exceptions;
using Iggy_SDK_Tests.E2ETests.Fixtures.Tcp;
using Iggy_SDK_Tests.Utils;

namespace Iggy_SDK_Tests.E2ETests.Streams;

[TestCaseOrderer("Iggy_SDK_Tests.Utils.PriorityOrderer", "Iggy_SDK_Tests")]
public sealed class StreamsE2ETcp : IClassFixture<IggyTcpStreamFixture>
{
    private readonly IggyTcpStreamFixture _fixture;

    public StreamsE2ETcp(IggyTcpStreamFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact, TestPriority(1)]
    public async Task CreateStream_HappyPath_Should_CreateStream_Successfully()
    {
        await _fixture.sut.Invoking(async x => await x.CreateStreamAsync(_fixture.StreamRequest))
            .Should()
            .NotThrowAsync();
    }

    [Fact, TestPriority(2)]
    public async Task CreateStream_Duplicate_Should_Throw_InvalidResponse()
    {
        await _fixture.sut.Invoking(async x => await x.CreateStreamAsync(_fixture.StreamRequest))
            .Should()
            .ThrowExactlyAsync<InvalidResponseException>()
            .WithMessage("Invalid response status code: 1012");
    }

    [Fact, TestPriority(3)]
    public async Task GetStreamById_Should_ReturnValidResponse()
    {
        var response = await _fixture.sut.GetStreamByIdAsync(Identifier.Numeric((int)_fixture.StreamRequest.StreamId!));
        response.Should().NotBeNull();
        response!.Id.Should().Be(_fixture.StreamRequest.StreamId);
        response.Name.Should().Be(_fixture.StreamRequest.Name);
    }

    [Fact, TestPriority(4)]
    public async Task UpdateStream_Should_UpdateStream_Successfully()
    {
        await _fixture.sut.Invoking(async x => await x.UpdateStreamAsync(Identifier.Numeric((int)_fixture.StreamRequest.StreamId!), _fixture.UpdateStreamRequest))
            .Should()
            .NotThrowAsync();
        var result = await _fixture.sut.GetStreamByIdAsync(Identifier.Numeric((int)_fixture.StreamRequest.StreamId!));
        result.Should().NotBeNull();
        result.Name.Should().Be(_fixture.UpdateStreamRequest.Name);
    }

    [Fact, TestPriority(5)]
    public async Task DeleteStream_Should_DeleteStream_Successfully()
    {
        await _fixture.sut.Invoking(async x =>
                await x.DeleteStreamAsync(Identifier.Numeric((int)_fixture.StreamRequest.StreamId!)))
            .Should()
            .NotThrowAsync();
    }

    [Fact, TestPriority(6)]
    public async Task DeleteStream_Should_Throw_InvalidResponse()
    {
        await _fixture.sut.Invoking(async x => await x.DeleteStreamAsync(Identifier.Numeric((int)_fixture.StreamRequest.StreamId!)))
            .Should()
            .ThrowExactlyAsync<InvalidResponseException>()
            .WithMessage("Invalid response status code: 1009");
    }

    [Fact, TestPriority(7)]
    public async Task GetStreamById_Should_Throw_InvalidResponse()
    {
        await _fixture.sut.Invoking(async x =>
                await x.GetStreamByIdAsync(Identifier.Numeric((int)_fixture.StreamRequest.StreamId!)))
            .Should()
            .ThrowExactlyAsync<InvalidResponseException>()
            .WithMessage("Invalid response status code: 1009");
    }
}