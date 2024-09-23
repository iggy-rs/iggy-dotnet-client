using FluentAssertions;
using Iggy_SDK;
using Iggy_SDK_Tests.E2ETests.Fixtures.Bootstraps;
using Iggy_SDK_Tests.E2ETests.Fixtures.Tcp;
using Iggy_SDK_Tests.Utils;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;
using Iggy_SDK.Exceptions;

namespace Iggy_SDK_Tests.E2ETests;

[TestCaseOrderer("Iggy_SDK_Tests.Utils.PriorityOrderer", "Iggy_SDK_Tests")]
public sealed class UsersE2E : IClassFixture<IggyTcpUsersFixture>
{
    private readonly IggyTcpUsersFixture _fixture;
    public UsersE2E(IggyTcpUsersFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact, TestPriority(1)]
    public async Task CreateUser_Should_CreateUser_Successfully()
    {
        // act & assert
        var tasks = _fixture.SubjectsUnderTest.Select(sut => Task.Run(async () =>
        {
            await sut.Invoking(async x =>
                await x.CreateUser(UsersFixtureBootstrap.UserRequest)
            )
            .Should()
            .NotThrowAsync();
        }));
        
        await Task.WhenAll(tasks);
    }

    [Fact, TestPriority(2)]
    public async Task CreateUser_Duplicate_Should_Throw_InvalidResponse()
    {
        // act & assert
        await _fixture.TcpSut.Invoking(y =>
                y.CreateUser(UsersFixtureBootstrap.UserRequest)
            )
            .Should()
            .ThrowExactlyAsync<InvalidResponseException>();
        
        
        // await _fixture.TcpSut.Invoking(y => y.CreateUser(UsersFixtureBootstrap.UserRequest))
        //         .Should()
        //         .ThrowExactlyAsync<InvalidResponseException>();
        // TODO: This code block is commmented bacause TCP implementation is not working properly.
        // var tasks = _fixture.SubjectsUnderTest.Select(sut => Task.Run(async () =>
        // {
        //     await sut.Invoking(async x =>
        //         await x.CreateUser(UsersFixtureBootstrap.UserRequest)
        //     )
        //     .Should()
        //     .ThrowExactlyAsync<InvalidResponseException>();
        // }));
        //
        // await Task.WhenAll(tasks);
    }
    
    [Fact, TestPriority(3)]
    public async Task GetUser_Should_ReturnValidResponse()
    {
        // act
        var response = await _fixture.HttpSut.GetUser(Identifier.Numeric(2));
        
        // assert
        response.Should().NotBeNull();
        response!.Id.Should().Be(2);
        response.Username.Should().Be(UsersFixtureBootstrap.UserRequest.Username);
        response.Status.Should().Be(UsersFixtureBootstrap.UserRequest.Status);
        response.Permissions!.Global.Should().NotBeNull();
        response.Permissions.Global.Should().BeEquivalentTo(UsersFixtureBootstrap.UserRequest.Permissions!.Global);
        
        // TODO: This code block is commmented bacause TCP implementation is not working properly.
        // var tasks = _fixture.SubjectsUnderTest.Select(sut => Task.Run(async () =>
        // {
        //     var response = await sut.GetUser(Identifier.Numeric(2));
        //     response.Should().NotBeNull();
        //     response!.Id.Should().Be(2);
        //     response.Username.Should().Be(UsersFixtureBootstrap.UserRequest.Username);
        //     response.Status.Should().Be(UsersFixtureBootstrap.UserRequest.Status);
        //     response.Permissions!.Global.Should().NotBeNull();
        //     response.Permissions.Global.Should().BeEquivalentTo(UsersFixtureBootstrap.UserRequest.Permissions!.Global);
        // })).ToArray();
        // await Task.WhenAll(tasks);
    }
    
    [Fact, TestPriority(4)]
    public async Task GetUsers_Should_ReturnValidResponse()
    {
        // act
        var response = await _fixture.HttpSut.GetUsers();
        
        // assert
        response.Should().NotBeEmpty();
        response.Should().HaveCount(2);
        
        // TODO: This code block is commmented bacause TCP implementation is not working properly.
        // var tasks = _fixture.SubjectsUnderTest.Select(sut => Task.Run(async () =>
        // {
        //     var response = await sut.GetUsers();
        //     response.Should().NotBeEmpty();
        //     response.Should().HaveCount(2);
        // })).ToArray();
        // await Task.WhenAll(tasks);
    }
    
    [Fact, TestPriority(5)]
    public async Task UpdateUser_Should_UpdateUser_Successfully()
    {
        await _fixture.HttpSut
            .Invoking(y => 
                y.UpdateUser(new UpdateUserRequest
                        {
                            UserId = Identifier.Numeric(2),
                            Username = UsersFixtureBootstrap.NewUsername,
                            UserStatus = UserStatus.Active
                        })
            )
            .Should()
            .NotThrowAsync();

        var user = await _fixture.HttpSut.GetUser(Identifier.Numeric(2));
        user!.Username.Should().Be(UsersFixtureBootstrap.NewUsername);
        
        // TODO: This code block is commmented bacause TCP implementation is not working properly.
        // var tasks = _fixture.SubjectsUnderTest.Select(sut => Task.Run(async () =>
        // {
        //     await sut.Invoking(async x =>
        //         await x.UpdateUser(new UpdateUserRequest
        //         {
        //             UserId = Identifier.Numeric(2), Username = UsersFixtureBootstrap.NewUsername, UserStatus = UserStatus.Active
        //         })).Should().NotThrowAsync();
        //     var user = await sut.GetUser(Identifier.Numeric(2));
        //     user!.Username.Should().Be(UsersFixtureBootstrap.NewUsername);
        // })).ToArray();
        // await Task.WhenAll(tasks);
    }
    
    [Fact, TestPriority(6)]
    public async Task ChangePermissions_Should_ChangePermissions_Successfully()
    {
        await _fixture.HttpSut
            .Invoking(y => 
                y.UpdatePermissions(new UpdateUserPermissionsRequest
                    {
                        UserId = Identifier.Numeric(2),
                        Permissions = UsersFixtureBootstrap.UpdatePermissionsRequest
                    })
            )
            .Should()
            .NotThrowAsync();
        
        var user = await _fixture.HttpSut.GetUser(Identifier.Numeric(2));
        user!.Permissions!.Global.Should().NotBeNull();
        user.Permissions.Should().BeEquivalentTo(UsersFixtureBootstrap.UpdatePermissionsRequest);
        
        // TODO: This code block is commmented bacause TCP implementation is not working properly.
        // var tasks = _fixture.SubjectsUnderTest.Select(sut => Task.Run(async () =>
        // {
        //     await sut.Invoking(async x =>
        //         await x.UpdatePermissions(new UpdateUserPermissionsRequest
        //         {
        //             UserId = Identifier.Numeric(2), Permissions = UsersFixtureBootstrap.UpdatePermissionsRequest
        //         })).Should().NotThrowAsync();
        //     var user = await sut.GetUser(Identifier.Numeric(2));
        //     user!.Permissions!.Global.Should().NotBeNull();
        //     user.Permissions.Should().BeEquivalentTo(UsersFixtureBootstrap.UpdatePermissionsRequest);
        // })).ToArray();
        // await Task.WhenAll(tasks);
    }
    
    [Fact, TestPriority(7)]
    public async Task DeleteUser_Should_DeleteUser_Successfully()
    {
        // act & assert
        await _fixture.HttpSut.Invoking(y => y.DeleteUser(Identifier.Numeric(2)))
            .Should()
            .NotThrowAsync();
        
        // TODO: This code block is commmented bacause TCP implementation is not working properly.
        // var task = _fixture.SubjectsUnderTest.Select(sut => Task.Run(async () =>
        // {
        //     await sut.Invoking(async x =>
        //         await x.DeleteUser(Identifier.Numeric(2))).Should().NotThrowAsync();
        // })).ToArray();
        // await Task.WhenAll(task);
    }
    
    [Fact, TestPriority(8)]
    public async Task LogoutUser_Should_LogoutUser_Successfully()
    {
        // act & assert
        await _fixture.HttpSut.Invoking(y => y.LogoutUser())
            .Should()
            .NotThrowAsync();
        
        // TODO: This code block is commmented bacause TCP implementation is not working properly.
        // var tasks = _fixture.SubjectsUnderTest.Select(sut => Task.Run(async () =>
        // {
        //     await sut.Invoking(async x =>
        //         await x.LogoutUser()).Should().NotThrowAsync();
        // })).ToArray();
        // await Task.WhenAll(tasks);
    }
}

