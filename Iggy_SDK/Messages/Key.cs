using Iggy_SDK.Enums;

namespace Iggy_SDK.Messages;

public sealed class Key
{
	public required KeyKind Kind { get; init; }
	public required int Length { get; init; }
	public required byte[] Value { get; init; }
}
