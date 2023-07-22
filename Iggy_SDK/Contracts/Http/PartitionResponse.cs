namespace Iggy_SDK.Contracts.Http;

public sealed class PartitionContract
{
	public required int Id { get; init; }
	public required ulong MessagesCount { get; init; }

	public required int SegmentsCount { get; init; }

	public required int CurrentOffset { get; init; }

	public required int SizeBytes { get; init; }
}