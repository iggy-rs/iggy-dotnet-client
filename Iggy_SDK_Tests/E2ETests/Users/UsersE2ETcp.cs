using FluentAssertions;
using Iggy_SDK;
using Iggy_SDK_Tests.E2ETests.Fixtures.Tcp;
using Iggy_SDK_Tests.Utils;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;
using Iggy_SDK.Exceptions;
namespace Iggy_SDK_Tests.E2ETests.Users;

[TestCaseOrderer("Iggy_SDK_Tests.Utils.PriorityOrderer", "Iggy_SDK_Tests")]
public sealed class UsersE2ETcp : IClassFixture<IggyTcpUsersFixture>
{
    private readonly IggyTcpUsersFixture _fixture;
    public UsersE2ETcp(IggyTcpUsersFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact, TestPriority(1)]
    public async Task CreateUser_Should_CreateUser_Successfully()
    {
        await _fixture.sut.Invoking(async x =>
            await x.CreateUser(_fixture.UserRequest)
        ).Should().NotThrowAsync();
    }

    [Fact, TestPriority(2)]
    public async Task CreateUser_Duplicate_Should_Throw_InvalidResponse()
    {
        await _fixture.sut.Invoking(async x =>
            await x.CreateUser(_fixture.UserRequest)
        ).Should().ThrowExactlyAsync<InvalidResponseException>();
    }
    [Fact, TestPriority(3)]
    public async Task GetUser_Should_ReturnValidResponse()
    {
        var response = await _fixture.sut.GetUser(Identifier.Numeric(2));
        
        response.Should().NotBeNull();
        response!.Id.Should().Be(2);
        response.Username.Should().Be(_fixture.UserRequest.Username);
        response.Status.Should().Be(_fixture.UserRequest.Status);
        response.Permissions!.Global.Should().NotBeNull();
        response.Permissions.Global.Should().BeEquivalentTo(_fixture.UserRequest.Permissions!.Global);
    }
    [Fact, TestPriority(4)]
    public async Task GetUsers_Should_ReturnValidResponse()
    {
        var response = await _fixture.sut.GetUsers();
        response.Should().NotBeEmpty();
        response.Should().HaveCount(2);
    }
    [Fact, TestPriority(5)]
    public async Task UpdateUser_Should_UpdateUser_Successfully()
    {
        await _fixture.sut.Invoking(async x =>
            await x.UpdateUser(new UpdateUserRequest
            {
                UserId = Identifier.Numeric(2), Username = _fixture.NewUsername, UserStatus = UserStatus.Active
            })).Should().NotThrowAsync();
        var user = await _fixture.sut.GetUser(Identifier.Numeric(2));
        user!.Username.Should().Be(_fixture.NewUsername);
    }
    [Fact, TestPriority(6)]
    public async Task ChangePermissions_Should_ChangePermissions_Successfully()
    {
        await _fixture.sut.Invoking(async x =>
            await x.UpdatePermissions(new UpdateUserPermissionsRequest
            {
                UserId = Identifier.Numeric(2), Permissions = _fixture.UpdatePermissionsRequest
            })).Should().NotThrowAsync();
        var user = await _fixture.sut.GetUser(Identifier.Numeric(2));
        user!.Permissions!.Global.Should().NotBeNull();
        user.Permissions.Should().BeEquivalentTo(_fixture.UpdatePermissionsRequest);
    }
    [Fact, TestPriority(7)]
    public async Task DeleteUser_Should_DeleteUser_Successfully()
    {
        await _fixture.sut.Invoking(async x =>
            await x.DeleteUser(Identifier.Numeric(2))).Should().NotThrowAsync();
    }
    [Fact, TestPriority(8)]
    public async Task LogoutUser_Should_LogoutUser_Successfully()
    {
        await _fixture.sut.Invoking(async x =>
            await x.LogoutUser()).Should().NotThrowAsync();
    }
}

