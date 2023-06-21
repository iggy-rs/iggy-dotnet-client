namespace Iggy_SDK.Contracts;

public sealed class PartitionResponse
{
	public required int Id { get; init; }

	public required int SegmentsCount { get; init; }

	public required int CurrentOffset { get; init; }

	public required int SizeBytes { get; init; }
}