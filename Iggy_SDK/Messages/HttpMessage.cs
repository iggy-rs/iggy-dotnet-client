using System.Text.Json.Serialization;
using Iggy_SDK.Headers;
using Iggy_SDK.JsonConfiguration;
using Iggy_SDK.Kinds;

namespace Iggy_SDK.Messages;

[JsonConverter(typeof(MessageConverter))]
public sealed class HttpMessage
{
	public required UInt128 Id { get; set; }
	public required string Payload { get; set; }
	public Dictionary<HeaderKey, HeaderValue>? Headers { get; init; }
}