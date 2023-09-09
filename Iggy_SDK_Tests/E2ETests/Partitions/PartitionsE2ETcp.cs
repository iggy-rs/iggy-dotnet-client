using FluentAssertions;
using Iggy_SDK;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Exceptions;
using Iggy_SDK_Tests.E2ETests.Fixtures.Tcp;
using Iggy_SDK_Tests.Utils;
using Iggy_SDK_Tests.Utils.Partitions;

namespace Iggy_SDK_Tests.E2ETests.Partitions;

[TestCaseOrderer("Iggy_SDK_Tests.Utils.PriorityOrderer", "Iggy_SDK_Tests")]
public sealed class PartitionsE2ETcp : IClassFixture<IggyTcpPartitionFixture>
{
    private readonly IggyTcpPartitionFixture _fixture;
    private readonly CreatePartitionsRequest _partitionsRequest;
    private readonly DeletePartitionsRequest _deletePartitionsRequest;
    public PartitionsE2ETcp(IggyTcpPartitionFixture fixture)
    {
        _fixture = fixture;
        _partitionsRequest =
            PartitionFactory.CreatePartitionsRequest(_fixture.StreamRequest.StreamId, _fixture.TopicRequest.TopicId);
        _deletePartitionsRequest = PartitionFactory.CreateDeletePartitionsRequest(_fixture.StreamRequest.StreamId,
            _fixture.TopicRequest.TopicId, _partitionsRequest.PartitionsCount);
    }

    [Fact, TestPriority(1)]
    public async Task CreatePartition_HappyPath_Should_CreatePartition_Successfully()
    {
        await _fixture.sut.Invoking(x => x.CreatePartitionsAsync(_partitionsRequest))
            .Should()
            .NotThrowAsync();
    }

    [Fact, TestPriority(2)]
    public async Task DeletePartition_Should_DeletePartition_Successfully()
    {
        await _fixture.sut.Invoking(x => x.DeletePartitionsAsync(_deletePartitionsRequest))
            .Should()
            .NotThrowAsync();
    }

    [Fact, TestPriority(3)]
    public async Task DeletePartition_Should_Throw_WhenTopic_DoesNotExist()
    {
        await _fixture.sut.CreatePartitionsAsync(_partitionsRequest);
        await _fixture.sut.DeleteTopicAsync(Identifier.Numeric(_fixture.StreamRequest.StreamId),
            Identifier.Numeric(_fixture.TopicRequest.TopicId));
        await _fixture.sut.Invoking(x => x.DeletePartitionsAsync(_deletePartitionsRequest))
            .Should()
            .ThrowExactlyAsync<InvalidResponseException>();
    }

    [Fact, TestPriority(4)]
    public async Task DeletePartition_Should_Throw_WhenStream_DoesNotExist()
    {
        await _fixture.sut.CreateTopicAsync(Identifier.Numeric(_fixture.StreamRequest.StreamId), _fixture.TopicRequest);
        await _fixture.sut.DeleteStreamAsync(Identifier.Numeric(_fixture.StreamRequest.StreamId));
        await _fixture.sut.Invoking(x => x.DeletePartitionsAsync(_deletePartitionsRequest))
            .Should()
            .ThrowExactlyAsync<InvalidResponseException>();
    }
}