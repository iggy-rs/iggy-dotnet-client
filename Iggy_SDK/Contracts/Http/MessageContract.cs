namespace Iggy_SDK.Contracts;

public sealed class MessageContract
{
	public required int Id { get; init; }
	public required string Payload { get; init; }
}