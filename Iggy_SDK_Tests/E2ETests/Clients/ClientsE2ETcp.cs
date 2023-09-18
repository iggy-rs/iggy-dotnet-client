using FluentAssertions;
using Iggy_SDK_Tests.E2ETests.Fixtures.Tcp;
using Iggy_SDK_Tests.Utils;
namespace Iggy_SDK_Tests.E2ETests.Clients;

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
        var clients = await _fixture.sut.GetClientsAsync();
        clients.Count.Should().Be(_fixture.TotalClientsCount);
        clients.Should().AllSatisfy(x => x.Transport.Should().Be("TCP"));
    }
    [Fact, TestPriority(2)]
    public async Task GetClient_Should_Return_CorrectClient()
    {
        var clients = await _fixture.sut.GetClientsAsync();
        clients.Count.Should().Be(_fixture.TotalClientsCount);
        clients.Should().AllSatisfy(x => x.Transport.Should().Be("TCP"));
        var id = clients[0].Id;
        var response = await _fixture.sut.GetClientByIdAsync(id);
        response!.Id.Should().Be(id);
        response.Transport.Should().Be("TCP");
    }
}