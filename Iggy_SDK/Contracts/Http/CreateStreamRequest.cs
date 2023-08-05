using Iggy_SDK.Identifiers;

namespace Iggy_SDK.Contracts.Http;

public sealed class StreamRequest
{
    public required int StreamId { get; init; }
    public required string Name { get; init; }
}