using Iggy_SDK.Messages;

namespace ConsoleApp;

public sealed class DummyMessage : IMessage
{
	public int Id { get; set; }
	public string Payload { get; set; }
}