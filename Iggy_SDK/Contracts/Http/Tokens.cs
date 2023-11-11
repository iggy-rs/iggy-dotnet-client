namespace Iggy_SDK.Contracts.Http;

public sealed class Tokens
{
    public TokenInfo? AccessToken { get; init; }
    public TokenInfo? RefreshToken { get; init; }
}