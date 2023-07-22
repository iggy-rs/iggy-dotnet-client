
namespace Iggy_SDK.Contracts.Http;

public sealed class StreamResponse
{
	public required int Id { get; init; }
	public required string Name { get; init; }
	public required ulong SizeBytes { get; init; }
	public required ulong MessagesCount { get; init; }
	public required int TopicsCount { get; init; }
	public IEnumerable<TopicResponse>? Topics { get; init; }
}