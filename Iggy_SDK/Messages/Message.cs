namespace Iggy_SDK.Messages;

public sealed class Message
{
	public Guid Id { get; set; }
	public byte[] Payload { get; set; }
}