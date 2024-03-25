using FluentAssertions;
using Iggy_SDK;
using Iggy_SDK_Tests.E2ETests.Fixtures;
using Iggy_SDK_Tests.E2ETests.Fixtures.Bootstraps;
using Iggy_SDK_Tests.Utils;
using Iggy_SDK.Exceptions;

namespace Iggy_SDK_Tests.E2ETests;

[TestCaseOrderer("Iggy_SDK_Tests.Utils.PriorityOrderer", "Iggy_SDK_Tests")]
public sealed class StreamsE2E : IClassFixture<IggyStreamFixture>
{
    private readonly IggyStreamFixture _fixture;

    public StreamsE2E(IggyStreamFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact, TestPriority(1)]
    public async Task CreateStream_HappyPath_Should_CreateStream_Successfully()
    {
        var tasks = _fixture.SubjectsUnderTest.Select(sut => Task.Run(async () =>
        {
            await sut.Invoking(async x => await x.CreateStreamAsync(StreamsFixtureBootstrap.StreamRequest))
                .Should()
                .NotThrowAsync();
        })).ToArray();
        await Task.WhenAll(tasks);
    }

    [Fact, TestPriority(2)]
    public async Task CreateStream_Duplicate_Should_Throw_InvalidResponse()
    {
        var tasks = _fixture.SubjectsUnderTest.Select(sut => Task.Run(async () =>
        {
            await sut.Invoking(async x => await x.CreateStreamAsync(StreamsFixtureBootstrap.StreamRequest))
                .Should()
                .ThrowExactlyAsync<InvalidResponseException>();
        })).ToArray();
        await Task.WhenAll(tasks);
    }

    [Fact, TestPriority(3)]
    public async Task GetStreamById_Should_ReturnValidResponse()
    {
        var tasks = _fixture.SubjectsUnderTest.Select(sut => Task.Run(async () =>
        {
            var response = await sut.GetStreamByIdAsync(Identifier.Numeric((int)StreamsFixtureBootstrap.StreamRequest.StreamId!));
            response.Should().NotBeNull();
            response!.Id.Should().Be(StreamsFixtureBootstrap.StreamRequest.StreamId);
            response.Name.Should().Be(StreamsFixtureBootstrap.StreamRequest.Name);
        })).ToArray();
        await Task.WhenAll(tasks);
    }

    [Fact, TestPriority(4)]
    public async Task UpdateStream_Should_UpdateStream_Successfully()
    {
        var tasks = _fixture.SubjectsUnderTest.Select(sut => Task.Run(async () =>
        {
            await sut.Invoking(async x => await x.UpdateStreamAsync(Identifier.Numeric((int)StreamsFixtureBootstrap.StreamRequest.StreamId!), StreamsFixtureBootstrap.UpdateStreamRequest))
                .Should()
                .NotThrowAsync();
            var result = await sut.GetStreamByIdAsync(Identifier.Numeric((int)StreamsFixtureBootstrap.StreamRequest.StreamId!));
            result.Should().NotBeNull();
            result!.Name.Should().Be(StreamsFixtureBootstrap.UpdateStreamRequest.Name);
        })).ToArray();
        await Task.WhenAll(tasks);
    }

    [Fact, TestPriority(5)]
    public async Task DeleteStream_Should_DeleteStream_Successfully()
    {
        var tasks = _fixture.SubjectsUnderTest.Select(sut => Task.Run(async () =>
        {
            await sut.Invoking(async x => await x.DeleteStreamAsync(Identifier.Numeric((int)StreamsFixtureBootstrap.StreamRequest.StreamId!)))
                .Should()
                .NotThrowAsync();
        })).ToArray();
        await Task.WhenAll(tasks);
    }

    [Fact, TestPriority(6)]
    public async Task DeleteStream_Should_Throw_InvalidResponse()
    {
        var tasks = _fixture.SubjectsUnderTest.Select(sut => Task.Run(async () =>
        {
            await sut.Invoking(async x => await x.DeleteStreamAsync(Identifier.Numeric((int)StreamsFixtureBootstrap.StreamRequest.StreamId!)))
                .Should()
                .ThrowExactlyAsync<InvalidResponseException>();
        })).ToArray();
        await Task.WhenAll(tasks);
    }

    [Fact, TestPriority(7)]
    public async Task GetStreamById_Should_Throw_InvalidResponse()
    {
        var tasks = _fixture.SubjectsUnderTest.Select(sut => Task.Run(async () =>
        {
            await sut.Invoking(async x =>
                await x.GetStreamByIdAsync(Identifier.Numeric((int)StreamsFixtureBootstrap.StreamRequest.StreamId!)))
            .Should()
            .ThrowExactlyAsync<InvalidResponseException>();
        })).ToArray();
        await Task.WhenAll(tasks);
    }
}