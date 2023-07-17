using System.Numerics;

namespace Iggy_SDK.Contracts;

public sealed class MessageResponse
{
	public required ulong Offset { get; init; }
	public required ulong Timestamp { get; init; }
	public UInt128 Id { get; init; }
	public required string Payload { get; init; }
}