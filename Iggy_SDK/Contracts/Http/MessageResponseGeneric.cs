
using Iggy_SDK.Enums;
using Iggy_SDK.Headers;
using Iggy_SDK.Kinds;

namespace Iggy_SDK.Contracts.Http;

public sealed class MessageResponse<T>
{
	public required ulong Offset { get; init; }
	public required MessageState State { get; init; }
	public required ulong Timestamp { get; init; }
	public Guid Id { get; init; }
	public required uint Checksum { get; init; }
	public Dictionary<HeaderKey, HeaderValue>? Headers { get; init; }
	public required T Message { get; init; }
}