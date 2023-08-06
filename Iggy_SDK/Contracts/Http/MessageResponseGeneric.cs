
namespace Iggy_SDK.Contracts.Http;

public sealed class MessageResponse<T>
{
	public required ulong Offset { get; init; }
	public required ulong Timestamp { get; init; }
	public Guid Id { get; init; }
	public required T Message { get; init; }
}