using Iggy_SDK.JsonConfiguration;
using Iggy_SDK.Kinds;
using Iggy_SDK.Messages;
using System.Text.Json.Serialization;

namespace Iggy_SDK.Contracts.Http;

[JsonConverter(typeof(MessagesConverter))]
public sealed class MessageSendRequest
{
    public required Identifier StreamId { get; init; }
    public required Identifier TopicId { get; init; }
    public required Partitioning Partitioning { get; init; }
    public required IList<Message> Messages { get; init; }
}