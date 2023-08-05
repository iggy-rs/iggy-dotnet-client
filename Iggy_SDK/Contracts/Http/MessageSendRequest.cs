using System.Text.Json.Serialization;
using Iggy_SDK.Identifiers;
using Iggy_SDK.JsonConfiguration;
using Iggy_SDK.Messages;

namespace Iggy_SDK.Contracts.Http;

[JsonConverter(typeof(MessagesConverter))]
public sealed class MessageSendRequest
{
	public required Key Key { get; init; }
	public required IEnumerable<Message> Messages { get; init; }
}