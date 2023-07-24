namespace Iggy_SDK.Messages;

public sealed class HttpMessage
{
	public required UInt128 Id { get; set; }
	public required string Payload { get; set; }
}