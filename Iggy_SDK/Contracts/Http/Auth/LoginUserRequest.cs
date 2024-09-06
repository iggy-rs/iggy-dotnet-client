namespace Iggy_SDK.Contracts.Http;

public sealed class LoginUserRequest
{
    public required string Username { get; init; }
    public required string Password { get; init; }
}