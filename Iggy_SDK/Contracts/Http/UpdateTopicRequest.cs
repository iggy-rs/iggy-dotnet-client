namespace Iggy_SDK.Contracts.Http;

public sealed class UpdateTopicRequest
{
    public required string Name { get; init; }
    public int? MessageExpiry { get; init; }
}