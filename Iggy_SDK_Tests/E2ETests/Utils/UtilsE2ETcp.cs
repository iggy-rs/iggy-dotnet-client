using FluentAssertions;
using Iggy_SDK_Tests.E2ETests.Fixtures.Tcp;
using Iggy_SDK_Tests.Utils;

namespace Iggy_SDK_Tests.E2ETests.Utils;

[TestCaseOrderer("Iggy_SDK_Tests.Utils.PriorityOrderer", "Iggy_SDK_Tests")]
public sealed class UtilsE2ETcp : IClassFixture<IggyTcpGeneralFixture>
{
    private readonly IggyTcpGeneralFixture _fixture;

    public UtilsE2ETcp(IggyTcpGeneralFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact, TestPriority(1)]
    public async Task GetStats_Should_ReturnValidResponse()
    {
        var response =await _fixture.sut.GetStatsAsync();
        response.Should().NotBeNull();
        response!.MessagesCount.Should().Be(0);
        response.PartitionsCount.Should().Be(0);
        response.StreamsCount.Should().Be(0);
        response.TopicsCount.Should().Be(0);
    }
}