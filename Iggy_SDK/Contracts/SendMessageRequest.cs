namespace Iggy_SDK.Contracts;

public sealed class SendMessageRequest
{
	public required string KeyKind { get; init; }
	public required int KeyValue { get; init; }
	public required IEnumerable<MessageRequest> Messages { get; init; }
}