using FluentAssertions;
using FluentAssertions.Common;
using Iggy_SDK_Tests.E2ETests.Fixtures.Tcp;
using Iggy_SDK_Tests.Utils;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Exceptions;
namespace Iggy_SDK_Tests.E2ETests.PAT;


[TestCaseOrderer("Iggy_SDK_Tests.Utils.PriorityOrderer", "Iggy_SDK_Tests")]
public sealed class PATE2ETcp : IClassFixture<IggyTcpPATFixture>
{
    private readonly IggyTcpPATFixture _fixture;
    public PATE2ETcp(IggyTcpPATFixture fixture)
    {
        _fixture = fixture;
    }
    [Fact, TestPriority(1)]
    public async Task CreatePersonalAccessToken_HappyPath_Should_CreatePersonalAccessToken_Successfully()
    {
        await _fixture.sut.Invoking(x => x.CreatePersonalAccessTokenAsync(_fixture.CreatePersonalAccessTokenRequest))
            .Should()
            .NotThrowAsync();
    }
    [Fact, TestPriority(2)]
    public async Task CreatePersonalAccessToken_Duplicate_Should_Throw_InvalidResponse()
    {
        await _fixture.sut.Invoking(x => x.CreatePersonalAccessTokenAsync(_fixture.CreatePersonalAccessTokenRequest))
            .Should()
            .ThrowExactlyAsync<InvalidResponseException>();
    }
    [Fact, TestPriority(3)]
    public async Task GetPersonalAccessTokens_Should_ReturnValidResponse()
    {
        var response = await _fixture.sut.GetPersonalAccessTokensAsync();
        response.Should().NotBeNull();
        response.Count.Should().Be(1);
        response[0].Name.Should().Be(_fixture.CreatePersonalAccessTokenRequest.Name);
        var tokenExpiryDateTimeOffset = DateTime.UtcNow.AddSeconds((double)_fixture.CreatePersonalAccessTokenRequest.Expiry).ToDateTimeOffset();
        response[0].Expiry.Value.Date.Should().Be(tokenExpiryDateTimeOffset.Date);
    }
    [Fact, TestPriority(4)]
    public async Task LoginWithPersonalAccessToken_Should_Be_Successfull()
    {
        var response = await _fixture.sut.CreatePersonalAccessTokenAsync(new CreatePersonalAccessTokenRequest()
        {
            Name = "test-login",
            Expiry = 69420,
        });
        await _fixture.sut.LogoutUser();
        await _fixture.sut.Invoking(x => x.LoginWithPersonalAccessToken(new LoginWithPersonalAccessToken
        {
            Token = response.Token
        })).Should().NotThrowAsync();
    }
    [Fact, TestPriority(5)]
    public async Task DeletePersonalAccessToken_Should_DeletePersonalAccessToken_Successfully()
    {
        await _fixture.sut.Invoking(x => x.DeletePersonalAccessTokenAsync(new DeletePersonalAccessTokenRequest
            {
                Name = _fixture.CreatePersonalAccessTokenRequest.Name
            }))
            .Should()
            .NotThrowAsync();
    }
}