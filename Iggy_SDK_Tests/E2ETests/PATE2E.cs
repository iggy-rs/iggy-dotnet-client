using FluentAssertions;
using FluentAssertions.Common;
using Iggy_SDK_Tests.E2ETests.Fixtures;
using Iggy_SDK_Tests.E2ETests.Fixtures.Bootstraps;
using Iggy_SDK_Tests.Utils;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Exceptions;

namespace Iggy_SDK_Tests.E2ETests;


[TestCaseOrderer("Iggy_SDK_Tests.Utils.PriorityOrderer", "Iggy_SDK_Tests")]
public sealed class PATE2E : IClassFixture<IggyPATFixture>
{
    private const string SkipMessage = "TCP implementation needs to be aligned with Iggyrs core changes";
    private readonly IggyPATFixture _fixture;
    
    public PATE2E(IggyPATFixture fixture)
    {
        _fixture = fixture;
    }
    
    [Fact(Skip = SkipMessage), TestPriority(1)]
    public async Task CreatePersonalAccessToken_HappyPath_Should_CreatePersonalAccessToken_Successfully()
    {
        var tasks = _fixture.SubjectsUnderTest.Select(sut => Task.Run(async () =>
        {
            await sut.Invoking(x => x.CreatePersonalAccessTokenAsync(PATFixtureBootstrap.CreatePersonalAccessTokenRequest))
                .Should()
                .NotThrowAsync();
        })).ToArray();
        await Task.WhenAll(tasks);
    }
    
    [Fact(Skip = SkipMessage), TestPriority(2)]
    public async Task CreatePersonalAccessToken_Duplicate_Should_Throw_InvalidResponse()
    {
        var tasks = _fixture.SubjectsUnderTest.Select(sut => Task.Run(async () =>
        {
            await sut.Invoking(x => x.CreatePersonalAccessTokenAsync(PATFixtureBootstrap.CreatePersonalAccessTokenRequest))
                .Should()
                .ThrowExactlyAsync<InvalidResponseException>();
        })).ToArray();
        await Task.WhenAll(tasks);
    }
    
    [Fact(Skip = SkipMessage), TestPriority(3)]
    public async Task GetPersonalAccessTokens_Should_ReturnValidResponse()
    {
        var tasks = _fixture.SubjectsUnderTest.Select( sut => Task.Run(async () =>
        {
            var response = await sut.GetPersonalAccessTokensAsync();
            response.Should().NotBeNull();
            response.Count.Should().Be(1);
            response[0].Name.Should().Be(PATFixtureBootstrap.CreatePersonalAccessTokenRequest.Name);
            var tokenExpiryDateTimeOffset = DateTime.UtcNow.AddSeconds((double)PATFixtureBootstrap.CreatePersonalAccessTokenRequest.Expiry!).ToDateTimeOffset();
            response[0].Expiry!.Value.Date.Should().Be(tokenExpiryDateTimeOffset.Date);
        })).ToArray();
        await Task.WhenAll(tasks);
    }
    
    [Fact(Skip = SkipMessage), TestPriority(4)]
    public async Task LoginWithPersonalAccessToken_Should_Be_Successfull()
    {
        var tasks = _fixture.SubjectsUnderTest.Select(sut => Task.Run(async () =>
        {
            var response = await sut.CreatePersonalAccessTokenAsync(new CreatePersonalAccessTokenRequest
            {
                Name = "test-login",
                Expiry = 69420
            });
            await sut.LogoutUser();
            await sut.Invoking(x => x.LoginWithPersonalAccessToken(new LoginWithPersonalAccessToken
            {
                Token = response!.Token
            })).Should().NotThrowAsync();
        })).ToArray();
        await Task.WhenAll(tasks);
    }
    
    [Fact(Skip = SkipMessage), TestPriority(5)]
    public async Task DeletePersonalAccessToken_Should_DeletePersonalAccessToken_Successfully()
    {
        var tasks = _fixture.SubjectsUnderTest.Select(sut => Task.Run(async () =>
        {
            await sut.Invoking(x => x.DeletePersonalAccessTokenAsync(new DeletePersonalAccessTokenRequest
            {
                Name = PATFixtureBootstrap.CreatePersonalAccessTokenRequest.Name
            }))
            .Should()
            .NotThrowAsync();
        })).ToArray();
        await Task.WhenAll(tasks);
    }
}