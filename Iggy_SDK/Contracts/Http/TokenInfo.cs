namespace Iggy_SDK.Contracts.Http;

public sealed class TokenInfo
{
    public required string Token { get; init; }
    public DateTimeOffset Expiry { get; set; }
}