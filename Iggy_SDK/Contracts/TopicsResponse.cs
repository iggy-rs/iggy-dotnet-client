using System.Text.Json.Serialization;

namespace Iggy_SDK.Contracts;

public sealed class TopicsResponse
{
	public required int Id { get; init; }
	public required string Name { get; init; }
	public required int PartitionsCount { get; init; }
	public IEnumerable<PartitionResponse> Partitions { get; init; }
}
