using System.Text.Json.Serialization;

namespace Iggy_SDK.Contracts;

public sealed class StreamResponse
{
	public required int Id { get; init; }
	public required string Name { get; init; }
	public required int TopicsCount { get; init; }
	public required IEnumerable<TopicsResponse> Topics { get; init; } 
	
}