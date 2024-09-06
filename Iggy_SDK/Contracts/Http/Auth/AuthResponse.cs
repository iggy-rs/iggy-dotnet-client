namespace Iggy_SDK.Contracts.Http;

public record AuthResponse(int UserId, TokenInfo AccessToken)
{
    // public required int UserId { get; init; }
    // public TokenInfo AccessToken { get; init; }
}
