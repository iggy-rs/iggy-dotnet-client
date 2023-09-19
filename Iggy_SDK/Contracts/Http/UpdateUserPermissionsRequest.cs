namespace Iggy_SDK.Contracts.Http;

public sealed class UpdateUserPermissionsRequest
{
    public required Identifier UserId { get; init; }
    public Permissions? Permissions { get; init; }
}