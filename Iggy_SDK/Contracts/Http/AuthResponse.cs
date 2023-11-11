namespace Iggy_SDK.Contracts.Http;

public sealed class AuthResponse
{
    public required int UserId { get; init; }
    public Tokens? Tokens { get; init; } 
}
