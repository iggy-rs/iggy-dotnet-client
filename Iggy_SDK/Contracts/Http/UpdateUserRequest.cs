namespace Iggy_SDK.Contracts.Http;

public sealed class UpdateUserRequest
{
    public required Identifier UserId { get; init; }
    public string? Username { get; init; }
    public string? Password { get; init; }
}