namespace Iggy_SDK.Contracts.Http;

public sealed class TopicResponse
{
	public required int Id { get; init; }
	public required string Name { get; init; }
	public required int PartitionsCount { get; init; }
	public IEnumerable<PartitionContract>? Partitions { get; init; }
}
