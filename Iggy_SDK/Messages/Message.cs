namespace Iggy_SDK.Messages;

public struct Message
{
	public required Guid Id { get; init; }
	public required byte[] Payload { get; init; }
}