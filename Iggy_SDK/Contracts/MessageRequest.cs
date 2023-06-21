namespace Iggy_SDK.Contracts;

public sealed class MessageRequest
{
	public required int Id { get; init; }
	public required string Payload { get; init; }
}