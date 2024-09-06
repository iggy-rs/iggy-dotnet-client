namespace Iggy_SDK.Contracts.Http;

public sealed class AuthResponse
{
    public required int UserId { get; init; }
    public required TokenInfo? AccessToken { get; init; }
}
