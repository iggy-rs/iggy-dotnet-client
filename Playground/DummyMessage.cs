using Iggy_SDK.Messages;

namespace ConsoleApp;

public sealed class DummyMessage : IMessage
{
	public required int Id { get; set; }
	public required string Payload { get; set; }
}