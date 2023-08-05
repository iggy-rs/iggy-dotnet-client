using Iggy_SDK.Enums;
using Iggy_SDK.Identifiers;

namespace Iggy_SDK.Contracts.Http;

public sealed class OffsetRequest
{
	public required ConsumerType ConsumerType { get; init; }
	public required Identifier StreamId { get; init; }
	public required Identifier TopicId { get; init; }
	public required int ConsumerId { get; init; }
	public required int PartitionId { get; init; }
}