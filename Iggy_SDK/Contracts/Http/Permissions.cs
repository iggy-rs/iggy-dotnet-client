namespace Iggy_SDK.Contracts.Http;

public sealed class Permissions
{
    public required GlobalPermissions Global { get; init; }
    public required Dictionary<uint, StreamPermissions> Streams { get; init; }
}