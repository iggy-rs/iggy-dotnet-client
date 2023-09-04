using FluentAssertions;
using Iggy_SDK_Tests.E2ETests.Fixtures.Tcp;
using Iggy_SDK_Tests.Utils;
using Iggy_SDK_Tests.Utils.Offset;
using Iggy_SDK.Contracts.Http;

namespace Iggy_SDK_Tests.E2ETests.Offset;

[TestCaseOrderer("Iggy_SDK_Tests.Utils.PriorityOrderer", "Iggy_SDK_Tests")]
public sealed class OffsetE2ETcp : IClassFixture<IggyTcpOffsetFixture>
{
    private readonly IggyTcpOffsetFixture _fixture;
    private readonly StoreOffsetRequest _storeOffsetIndividualConsumer;
    private readonly OffsetRequest _offsetIndividualConsumer;

    private const int GET_INDIVIDUAL_CONSUMER_ID = 1;
    private const int GET_PARTITION_ID = 1;
    private const ulong GET_OFFSET = 0;

    public OffsetE2ETcp(IggyTcpOffsetFixture fixture)
    {
        _fixture = fixture;
        _storeOffsetIndividualConsumer = OffsetFactory.CreateOffsetContract(
            _fixture.StreamRequest.StreamId, _fixture.TopicRequest.TopicId, GET_INDIVIDUAL_CONSUMER_ID, GET_OFFSET,
            GET_PARTITION_ID);
        _offsetIndividualConsumer = OffsetFactory.CreateOffsetRequest(_fixture.StreamRequest.StreamId,
            _fixture.TopicRequest.TopicId, GET_PARTITION_ID, GET_INDIVIDUAL_CONSUMER_ID);
    }
    
    [Fact, TestPriority(1)]
    public async Task StoreOffset_IndividualConsumer_Should_StoreOffset_Successfully()
    {
        await _fixture.sut.Invoking(x => x.StoreOffsetAsync(_storeOffsetIndividualConsumer))
            .Should()
            .NotThrowAsync();
    }

    [Fact, TestPriority(2)]
    public async Task GetOffset_IndividualConsumer_Should_GetOffset_Successfully()
    {
        var offset = await _fixture.sut.GetOffsetAsync(_offsetIndividualConsumer);
        offset.Should().NotBeNull();
        offset!.StoredOffset.Should().Be(_storeOffsetIndividualConsumer.Offset);
    }
}