namespace Iggy_SDK.Contracts;

public sealed class OffsetResponse
{
	public required int ConsumerId { get; init; }
	public required int Offset { get; init; }
}