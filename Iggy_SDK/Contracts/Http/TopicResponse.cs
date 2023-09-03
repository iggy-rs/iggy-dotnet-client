using System.Text.Json.Serialization;
using Iggy_SDK.JsonConfiguration;

namespace Iggy_SDK.Contracts.Http;

[JsonConverter(typeof(TopicResponseConverter))]
public sealed class TopicResponse
{
	public required int Id { get; init; }
	public required DateTimeOffset CreatedAt { get; init; }
	public required string Name { get; init; }
	public required ulong SizeBytes { get; init; }
	public int MessageExpiry { get; init; }
	public required ulong MessagesCount { get; init; }
	public required int PartitionsCount { get; init; }
	public IEnumerable<PartitionContract>? Partitions { get; init; }
}
