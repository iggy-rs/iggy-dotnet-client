namespace Iggy_SDK.Contracts;

public sealed class OffsetContract
{
	public required int ConsumerId { get; init; }
	public required int PartitionId { get; init; }
	public required int Offset { get; init; }
}