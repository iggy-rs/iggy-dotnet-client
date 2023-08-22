using System.Text.Json.Serialization;
using Iggy_SDK.Headers;
using Iggy_SDK.JsonConfiguration;
using Iggy_SDK.Kinds;

namespace Iggy_SDK.Messages;

public struct Message
{
	public required Guid Id { get; init; }
	public required byte[] Payload { get; init; }
	public Dictionary<HeaderKey, HeaderValue>? Headers { get; init; }
}