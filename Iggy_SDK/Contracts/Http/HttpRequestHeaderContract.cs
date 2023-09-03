namespace Iggy_SDK.Contracts.Http;

public sealed class HttpRequestHeaderContract
{
    public required string Name { get; init; }
    public required IEnumerable<string> Values { get; init; }
}