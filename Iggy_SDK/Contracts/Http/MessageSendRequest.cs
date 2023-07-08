using Iggy_SDK.Enums;
using Iggy_SDK.Messages;

namespace Iggy_SDK.Contracts;

public sealed class MessageSendRequest
{
	public required int StreamId { get; init; }
	public required int TopicId { get; init; }
	public required Keykind KeyKind { get; init; }
	public required int KeyValue { get; init; }
	public required IEnumerable<IMessage> Messages { get; init; }
}