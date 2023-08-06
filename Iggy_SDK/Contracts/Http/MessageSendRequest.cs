using System.Text.Json.Serialization;
using Iggy_SDK.Identifiers;
using Iggy_SDK.JsonConfiguration;
using Iggy_SDK.Messages;

namespace Iggy_SDK.Contracts.Http;

[JsonConverter(typeof(MessagesConverter))]
public sealed class MessageSendRequest
{
	public required Partitioning Partitioning { get; init; }
	public required ICollection<Message> Messages { get; init; }
}