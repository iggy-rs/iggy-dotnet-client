namespace Iggy_SDK.Contracts.Http;

public sealed class TopicRequest
{
	public required int TopicId { get; init; }
	public required string Name { get; init; }
	public int? MessageExpiry { get; init; }
	public required int PartitionsCount{ get; init; }
}