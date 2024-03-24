using FluentAssertions;
using Iggy_SDK;
using Iggy_SDK_Tests.E2ETests.Fixtures.Bootstraps;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Exceptions;
using Iggy_SDK_Tests.E2ETests.Fixtures.Tcp;
using Iggy_SDK_Tests.Utils;
using Iggy_SDK_Tests.Utils.Groups;

namespace Iggy_SDK_Tests.E2ETests.ConsumerGroup;

[TestCaseOrderer("Iggy_SDK_Tests.Utils.PriorityOrderer", "Iggy_SDK_Tests")]
public sealed class ConsumerGroupE2ETcp : IClassFixture<IggyTcpConsumerGroupFixture>
{
    private readonly IggyTcpConsumerGroupFixture _fixture;
    private readonly CreateConsumerGroupRequest _createConsumerGroupRequest;
    private readonly JoinConsumerGroupRequest _joinConsumerGroupRequest;
    private readonly LeaveConsumerGroupRequest _leaveConsumerGroupRequest;
    private readonly DeleteConsumerGroupRequest _deleteConsumerGroupRequest;

    private const int GROUP_ID = 1;
    private Identifier ConsumerGroupId = Identifier.Numeric(GROUP_ID);

    public ConsumerGroupE2ETcp(IggyTcpConsumerGroupFixture fixture)
    {
        _fixture = fixture;
        _createConsumerGroupRequest = ConsumerGroupFactory.CreateRequest((int)ConsumerGroupFixtureBootstrap.StreamRequest.StreamId! ,
            (int)ConsumerGroupFixtureBootstrap.TopicRequest.TopicId!, GROUP_ID);
        _joinConsumerGroupRequest = ConsumerGroupFactory.CreateJoinGroupRequest((int)ConsumerGroupFixtureBootstrap.StreamRequest.StreamId,
            (int)ConsumerGroupFixtureBootstrap.TopicRequest.TopicId, GROUP_ID);
        _leaveConsumerGroupRequest = ConsumerGroupFactory.CreateLeaveGroupRequest((int)ConsumerGroupFixtureBootstrap.StreamRequest.StreamId,
            (int)ConsumerGroupFixtureBootstrap.TopicRequest.TopicId, GROUP_ID);
        _deleteConsumerGroupRequest = ConsumerGroupFactory.CreateDeleteGroupRequest((int)ConsumerGroupFixtureBootstrap.StreamRequest.StreamId,
            (int)ConsumerGroupFixtureBootstrap.TopicRequest.TopicId, GROUP_ID);
    }

    [Fact, TestPriority(1)]
    public async Task CreateConsumerGroup_HappyPath_Should_CreateConsumerGroup_Successfully()
    {
        var tasks = _fixture.SubjectsUnderTest.Select(sut => Task.Run(async () =>
        {
            await sut.CreateConsumerGroupAsync(_createConsumerGroupRequest);
        })).ToArray();
        await Task.WhenAll(tasks);
    }

    [Fact, TestPriority(2)]
    public async Task CreateConsumerGroup_Should_Throw_InvalidResponse()
    {
        var tasks = _fixture.SubjectsUnderTest.Select(sut => Task.Run(async () =>
        {
            await sut.Invoking(x => x.CreateConsumerGroupAsync(_createConsumerGroupRequest))
                .Should()
                .ThrowExactlyAsync<InvalidResponseException>();
        })).ToArray();
        await Task.WhenAll(tasks);
    }

    [Fact, TestPriority(3)]
    public async Task GetConsumerGroupById_Should_Return_ValidResponse()
    {
        var tasks = _fixture.SubjectsUnderTest.Select(sut => Task.Run(async () =>
        {
            var response = await sut.GetConsumerGroupByIdAsync(
                Identifier.Numeric((int)ConsumerGroupFixtureBootstrap.StreamRequest.StreamId!), Identifier.Numeric((int)ConsumerGroupFixtureBootstrap.TopicRequest.TopicId!),
                ConsumerGroupId);

            response.Should().NotBeNull();
            response!.Id.Should().Be(GROUP_ID);
            response.PartitionsCount.Should().Be(ConsumerGroupFixtureBootstrap.TopicRequest.PartitionsCount);
            response.MembersCount.Should().Be(0);
        })).ToArray();
        await Task.WhenAll(tasks);
    }

    [Fact, TestPriority(4)]
    public async Task JoinConsumerGroup_Should_JoinConsumerGroup_Successfully()
    {
        var sut = _fixture.SubjectsUnderTest[0];
        await sut.Invoking(x => x.JoinConsumerGroupAsync(_joinConsumerGroupRequest))
            .Should()
            .NotThrowAsync();
    }

    [Fact, TestPriority(5)]
    public async Task GetConsumerGroupById_Should_Return_ValidMembersCount()
    {
        var sut = _fixture.SubjectsUnderTest[0];
        var response = await sut.GetConsumerGroupByIdAsync(
            Identifier.Numeric((int)ConsumerGroupFixtureBootstrap.StreamRequest.StreamId!), Identifier.Numeric((int)ConsumerGroupFixtureBootstrap.TopicRequest.TopicId!),
            ConsumerGroupId);
        response!.MembersCount.Should().Be(1);
    }

    [Fact, TestPriority(6)]
    public async Task LeaveConsumerGroup_Should_LeaveConsumerGroup_Successfully()
    {
        var sut = _fixture.SubjectsUnderTest[0];
        await sut.Invoking(x => x.LeaveConsumerGroupAsync(_leaveConsumerGroupRequest))
            .Should()
            .NotThrowAsync();
    }

    [Fact, TestPriority(7)]
    public async Task DeleteConsumerGroup_Should_DeleteConsumerGroup_Successfully()
    {
        var tasks = _fixture.SubjectsUnderTest.Select(sut => Task.Run(async () =>
        {
            await sut.Invoking(x => x.DeleteConsumerGroupAsync(_deleteConsumerGroupRequest))
                .Should()
                .NotThrowAsync();
        })).ToArray();
        await Task.WhenAll(tasks);
    }

    [Fact, TestPriority(8)]
    public async Task JoinConsumerGroup_Should_Throw_InvalidResponse()
    {
        var sut = _fixture.SubjectsUnderTest[0];
        await sut.Invoking(x => x.JoinConsumerGroupAsync(_joinConsumerGroupRequest))
            .Should()
            .ThrowExactlyAsync<InvalidResponseException>();
    }

    [Fact, TestPriority(9)]
    public async Task DeleteConsumerGroup_Should_Throw_InvalidResponse()
    {
        var tasks = _fixture.SubjectsUnderTest.Select(sut => Task.Run(async () =>
        {
            await sut.Invoking(x => x.DeleteConsumerGroupAsync(_deleteConsumerGroupRequest))
                .Should()
                .ThrowExactlyAsync<InvalidResponseException>();
        })).ToArray();
        await Task.WhenAll(tasks);
    }
}