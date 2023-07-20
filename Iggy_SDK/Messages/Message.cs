namespace Iggy_SDK.Messages;

public struct Message
{
	public Guid Id { get; set; }
	public byte[] Payload { get; set; }
}