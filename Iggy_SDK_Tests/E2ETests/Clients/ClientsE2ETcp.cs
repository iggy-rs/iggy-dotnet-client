using FluentAssertions;
using Iggy_SDK_Tests.E2ETests.Fixtures.Bootstraps;
using Iggy_SDK_Tests.E2ETests.Fixtures.Tcp;
using Iggy_SDK_Tests.Utils;

namespace Iggy_SDK_Tests.E2ETests.Clients;

//TODO(numinex): The clients query doesn't work for http in this test case, but works in general, figure that shit out.
[TestCaseOrderer("Iggy_SDK_Tests.Utils.PriorityOrderer", "Iggy_SDK_Tests")]
public sealed class ClientsE2ETcp : IClassFixture<IggyTcpClientsFixture>
{
    private readonly IggyTcpClientsFixture _fixture;

    public ClientsE2ETcp(IggyTcpClientsFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact, TestPriority(1)]
    public async Task GetClients_Should_Return_CorrectClientsCount()
    {
        var sut = _fixture.SubjectsUnderTest[0];
        var clients = await sut.GetClientsAsync();
        clients.Count.Should().Be(ClientsFixtureBootstrap.TotalClientsCount);
    }
        
    [Fact, TestPriority(2)]
    public async Task GetClient_Should_Return_CorrectClient()
    {
        var sut = _fixture.SubjectsUnderTest[0];
        var clients = await sut.GetClientsAsync();
        clients.Count.Should().Be(ClientsFixtureBootstrap.TotalClientsCount);
        uint id = clients[0].ClientId;
        var response = await sut.GetClientByIdAsync(id);
        response!.ClientId.Should().Be(id);
    }
}