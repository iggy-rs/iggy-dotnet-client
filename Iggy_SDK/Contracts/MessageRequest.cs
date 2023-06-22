using Iggy_SDK.Enums;

namespace Iggy_SDK.Contracts;

public sealed class MessageRequest
{
	public required int StreamId { get; init; }	
	public required int TopicId { get; init; }
	public required int ConsumerId { get; init; }
	public required int PartitionId { get; init; }
	public required MessagePolling PollingStrategy { get; init; }
	public required int Value { get; init; }
	public required int Count { get; init; }
	public required bool AutoCommit { get; init; }
}