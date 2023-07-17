using Iggy_SDK.Messages;

namespace Iggy_Sample_Producer
{
	public sealed class DummyMessage : IMessage
	{
		public required ulong Id { get; set; }
		public required string Payload { get; set; }
	}
}