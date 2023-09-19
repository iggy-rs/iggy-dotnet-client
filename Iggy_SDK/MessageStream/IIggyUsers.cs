using Iggy_SDK.Contracts.Http;
namespace Iggy_SDK.MessageStream;

public interface IIggyUsers
{
    public Task<UserResponse?> GetUser(Identifier userId, CancellationToken token = default);
    public Task<IReadOnlyList<UserResponse>> GetUsers();
    public Task CreateUser(CreateUserRequest request, CancellationToken token = default);
    public Task DeleteUser(Identifier userId, CancellationToken token = default);
    public Task UpdateUser(UpdateUserRequest request, CancellationToken token = default);
    public Task UpdatePermissions(UpdateUserPermissionsRequest request, CancellationToken token = default);
    public Task ChangePassword(ChangePasswordRequest request, CancellationToken token = default);
    public Task LoginUser(LoginUserRequest request, CancellationToken token = default);
    public Task LogoutUser(CancellationToken token = default);
}