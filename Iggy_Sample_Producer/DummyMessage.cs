using Iggy_SDK.Messages;

namespace Iggy_Sample_Producer
{
	public sealed class DummyMessage : IMessage
	{
		public required Guid Id { get; set; }
		public required string Payload { get; set; }
	}
}