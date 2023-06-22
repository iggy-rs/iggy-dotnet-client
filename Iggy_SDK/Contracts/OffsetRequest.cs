namespace Iggy_SDK.Contracts;

public sealed class OffsetRequest
{
	public required int StreamId { get; init; }
	public required int TopicId { get; init; }
	public required int ConsumerId { get; init; }
	public required int PartitionId { get; init; }
}