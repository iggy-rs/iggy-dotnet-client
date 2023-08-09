using Iggy_SDK.Enums;
using Iggy_SDK.Kinds;

namespace Iggy_SDK.Contracts.Http;

public sealed class OffsetContract
{
	public required Consumer Consumer { get; init; }
	public required int PartitionId { get; init; }
	public required ulong Offset { get; init; }
}