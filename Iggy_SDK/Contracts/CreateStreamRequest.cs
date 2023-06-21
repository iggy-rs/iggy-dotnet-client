namespace Iggy_SDK.Contracts;

public sealed class CreateStreamRequest
{
    public required int StreamId { get; init; }
    public required string Name { get; init; }
}