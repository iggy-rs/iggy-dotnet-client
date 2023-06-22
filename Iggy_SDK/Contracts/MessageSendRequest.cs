using Iggy_SDK.Enums;
using Iggy_SDK.Messages;

namespace Iggy_SDK.Contracts;

public sealed class MessageSendRequest
{
	public int StreamId { get; set; }
	public int TopicId { get; set; }
	public Keykind KeyKind { get; set; }
	public int KeyValue { get; set; }
	public IEnumerable<IMessage> Messages { get; set; }
}